using System.IO.Abstractions;
using System.Reactive.Linq;
using Autofac;
using Mutagen.Bethesda.Analyzers.Autofac;
using Mutagen.Bethesda.Analyzers.Config.Run;
using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.Modules;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Services;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Noggog;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers;

public record AnalyzerRunnerBuilderNeedsTarget
{
    private readonly GameRelease _gameRelease;

    public AnalyzerRunnerBuilderNeedsTarget(GameRelease gameRelease)
    {
        _gameRelease = gameRelease;
    }

    public AnalyzerRunnerBuilder WithLinkCache(ILinkCache linkCache)
    {
        return new AnalyzerRunnerBuilder(
            _gameRelease,
            linkCache);
    }

    public AnalyzerRunnerBuilder WithLoadOrder(ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder)
    {
        return new AnalyzerRunnerBuilder(
            _gameRelease,
            loadOrder);
    }

    public AnalyzerRunnerBuilder WithLoadOrder(ILoadOrderGetter<IModGetter> loadOrder)
    {
        return WithLoadOrder(loadOrder
            .Transform(x => new ModListing<IModGetter>(x, enabled: true)));
    }

    public AnalyzerRunnerBuilder WithGameEnvironment(IGameEnvironment gameEnvironment)
    {
        return WithLinkCache(gameEnvironment.LinkCache);
    }

    public AnalyzerRunnerBuilderTargetMod WithTargetMod(ModKey modKey)
    {
        return new AnalyzerRunnerBuilderTargetMod(
            _gameRelease,
            modKey);
    }
}

public record AnalyzerRunnerBuilderTargetMod
{
    private readonly GameRelease _gameRelease;
    private readonly ModKey _modKey;

    public AnalyzerRunnerBuilderTargetMod(
        GameRelease gameRelease,
        ModKey modKey)
    {
        _gameRelease = gameRelease;
        _modKey = modKey;
    }

    public AnalyzerRunnerBuilder WithLoadOrder(ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder)
    {
        return new AnalyzerRunnerBuilder(
            _gameRelease,
            loadOrder.TrimAt(_modKey));
        // ToDo
        // Add any applicable filters
    }

    public AnalyzerRunnerBuilder WithLoadOrder(ILoadOrderGetter<IModGetter> loadOrder)
    {
        return WithLoadOrder(loadOrder
            .Transform(x => new ModListing<IModGetter>(x, enabled: true)));
    }

    public AnalyzerRunnerBuilder WithGameEnvironment(IGameEnvironment gameEnvironment)
    {
        return WithLoadOrder(gameEnvironment.LoadOrder);
    }
}

public record AnalyzerRunnerBuilder
{
    private readonly GameRelease _gameRelease;
    private readonly ILinkCache _linkCache;
    private readonly ILoadOrderGetter<IModListingGetter<IModGetter>> _loadOrder;

    private IFileSystem? _fileSystem { get; init; }
    private IObservable<int?>? _numWorkThreads { get; init; }
    private IWorkDropoff? _workDropoff { get; init; }
    private Severity _minimumSeverity { get; init; } = Severity.Suggestion;
    private TopicConfig? _topicConfig { get; init; }
    private DirectoryPath? _dataDirectory { get; init; }
    private bool _addTypicalAnalyzers { get; init; }
    private IReadOnlyCollection<IAnalyzer> _customAnalyzers { get; init; } = [];
    private IReadOnlyCollection<ModKey> _blacklistedMods { get; init; } = [];

    internal AnalyzerRunnerBuilder(
        GameRelease gameRelease,
        ILinkCache linkCache)
    {
        _gameRelease = gameRelease;
        _linkCache = linkCache;
        _loadOrder = new LoadOrder<IModListingGetter<IModGetter>>(
            linkCache
                .ListedOrder
                .Select(x => new ModListing<IModGetter>(x.ModKey, x, enabled: true)));
    }

    internal AnalyzerRunnerBuilder(
        GameRelease gameRelease,
        ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder)
    {
        _gameRelease = gameRelease;
        _linkCache = loadOrder.ToUntypedImmutableLinkCache();
        _loadOrder = loadOrder;
    }

    public static AnalyzerRunnerBuilderNeedsTarget Create(
        GameRelease gameRelease)
    {
        return new AnalyzerRunnerBuilderNeedsTarget(gameRelease);
    }

    public AnalyzerRunnerBuilder WithFileSystem(IFileSystem fileSystem)
    {
        return this with
        {
            _fileSystem = fileSystem
        };
    }

    public AnalyzerRunnerBuilder WithThreads(int threads)
    {
        if (_workDropoff != null)
        {
            throw new InvalidOperationException("Cannot configure threads when a work dropoff has already been registered. Use either WithThreads or WithWorkDropoff, not both.");
        }

        return this with
        {
            _numWorkThreads = Observable.Return<int?>(threads)
        };
    }

    public AnalyzerRunnerBuilder WithThreads(IObservable<int?> threads)
    {
        if (_workDropoff != null)
        {
            throw new InvalidOperationException("Cannot configure threads when a work dropoff has already been registered. Use either WithThreads or WithWorkDropoff, not both.");
        }

        return this with
        {
            _numWorkThreads = threads
        };
    }

    public AnalyzerRunnerBuilder WithWorkDropoff(IWorkDropoff workDropoff)
    {
        if (_numWorkThreads != null)
        {
            throw new InvalidOperationException("Cannot register a work dropoff when threads have already been configured. Use either WithThreads or WithWorkDropoff, not both.");
        }

        return this with
        {
            _workDropoff = workDropoff
        };
    }

    public AnalyzerRunnerBuilder WithMinimumSeverity(Severity minimumSeverity)
    {
        return this with
        {
            _minimumSeverity = minimumSeverity
        };
    }

    public AnalyzerRunnerBuilder WithTopicConfig(TopicConfig? topicConfig)
    {
        return this with
        {
            _topicConfig = topicConfig
        };
    }

    public AnalyzerRunnerBuilder WithDataDirectory(DirectoryPath dataDirectory)
    {
        return this with
        {
            _dataDirectory = dataDirectory
        };
    }

    public AnalyzerRunnerBuilder WithTypicalAnalyzers()
    {
        return this with
        {
            _addTypicalAnalyzers = true
        };
    }

    public AnalyzerRunnerBuilder WithAnalyzers(params IEnumerable<IAnalyzer> analyzers)
    {
        var combined = _customAnalyzers.Concat(analyzers).ToArray();

        return this with
        {
            _customAnalyzers = combined
        };
    }

    public AnalyzerRunnerBuilder WithBlacklistedMods(params IEnumerable<ModKey> modKeys)
    {
        var combined = _blacklistedMods.Concat(modKeys).ToArray();

        return this with
        {
            _blacklistedMods = combined
        };
    }

    public IAnalyzerRunner Build()
    {
        var builder = new ContainerBuilder();

        builder.RegisterModule<AnalyzersModule>();

        // Dynamically load the appropriate analyzer module based on game release
        if (_addTypicalAnalyzers)
        {
            DynamicAnalyzerModuleLoader.LoadAnalyzerModule(builder, _gameRelease);
        }

        builder
            .RegisterInstance(_fileSystem ?? new FileSystem())
            .As<IFileSystem>();

        builder
            .RegisterInstance(new GameReleaseInjection(_gameRelease))
            .AsImplementedInterfaces();

        builder
            .RegisterInstance(GameConstants.Get(_gameRelease))
            .AsSelf()
            .AsImplementedInterfaces();

        builder
            .RegisterInstance(_loadOrder)
            .As<ILoadOrderGetter<IModListingGetter<IModGetter>>>();

        builder
            .RegisterInstance(_linkCache)
            .AsImplementedInterfaces();

        builder
            .RegisterInstance(_topicConfig ?? new TopicConfig())
            .AsSelf()
            .AsImplementedInterfaces();

        builder.RegisterInstance(new MinimumSeverityConfiguration(_minimumSeverity))
            .AsImplementedInterfaces();

        if (_dataDirectory != null)
        {
            builder
                .RegisterInstance(new DataDirectoryInjection(_dataDirectory.Value))
                .AsImplementedInterfaces();
        }

        foreach (var analyzer in _customAnalyzers
                     .DistinctBy(x => x.GetType()))
        {
            builder.RegisterInstance(analyzer).AsImplementedInterfaces();
        }

        // Register blacklisted mods provider
        builder.RegisterInstance(new BuilderBlacklistedModsProvider(_blacklistedMods))
            .As<IBlacklistedModsProvider>()
            .SingleInstance();

        var cont = builder.Build();

        var factory = cont.Resolve<AnalyzerRunner.Factory>();

        return factory(_workDropoff, _numWorkThreads);
    }

    /// <summary>
    /// Private implementation of IBlacklistedModsProvider for the builder
    /// </summary>
    private class BuilderBlacklistedModsProvider : IBlacklistedModsProvider
    {
        private readonly HashSet<ModKey> _blacklistedMods;

        public BuilderBlacklistedModsProvider(IEnumerable<ModKey> blacklistedMods)
        {
            _blacklistedMods = blacklistedMods.ToHashSet();
        }

        public bool IsBlacklisted(ModKey modKey)
        {
            return _blacklistedMods.Contains(modKey);
        }
    }
}
