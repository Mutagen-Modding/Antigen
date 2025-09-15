using System.IO.Abstractions;
using Autofac;
using Mutagen.Bethesda.Analyzers.Autofac;
using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
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

    public AnalyzerRunnerBuilder WithLoadOrder(ILoadOrder<IModListingGetter<IModGetter>> loadOrder)
    {
        return new AnalyzerRunnerBuilder(
            _gameRelease,
            loadOrder);
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
}

public record AnalyzerRunnerBuilder
{
    private readonly GameRelease _gameRelease;
    private readonly ILinkCache _linkCache;
    private readonly ILoadOrderGetter<IModListingGetter<IModGetter>> _loadOrder;

    private IFileSystem? _fileSystem { get; init; }
    private INumWorkThreadsController? _numWorkThreadsController { get; init; }
    private Severity _minimumSeverity { get; init; } = Severity.Suggestion;
    private TopicConfig? _topicConfig { get; init; }
    private DirectoryPath? _dataDirectory { get; init; }

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
        return this with
        {
            _numWorkThreadsController = new NumWorkThreadsConstant(threads)
        };
    }

    public AnalyzerRunnerBuilder WithThreads(IObservable<int?> threads)
    {
        return this with
        {
            _numWorkThreadsController = new NumWorkThreadsByObservable(threads)
        };
    }

    private class NumWorkThreadsByObservable(IObservable<int?> numThreads) : INumWorkThreadsController
    {
        public IObservable<int?> NumDesiredThreads { get; } = numThreads;
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
            _dataDirectory = _dataDirectory
        };
    }

    public IAnalyzerRunner Build()
    {
        var builder = new ContainerBuilder();

        builder.RegisterModule<MainModule>();
        builder.RegisterModule<SkyrimAnalyzerModule>();

        builder
            .RegisterInstance(_fileSystem ?? new FileSystem())
            .As<IFileSystem>();

        builder
            .RegisterInstance(new GameReleaseInjection(_gameRelease))
            .AsImplementedInterfaces();

        builder
            .RegisterInstance(_loadOrder)
            .AsImplementedInterfaces();

        builder
            .RegisterInstance(_linkCache)
            .AsImplementedInterfaces();

        builder
            .RegisterInstance(_topicConfig ?? new TopicConfig())
            .AsSelf()
            .AsImplementedInterfaces();

        builder.RegisterInstance(new MinimumSeverityConfiguration(_minimumSeverity))
            .AsImplementedInterfaces();

        builder
            .RegisterInstance(_numWorkThreadsController ?? new NumWorkThreadsConstant(null))
            .AsImplementedInterfaces();

        if (_dataDirectory != null)
        {
            builder
                .RegisterInstance(new DataDirectoryInjection(_dataDirectory.Value))
                .AsImplementedInterfaces();
        }

        var cont = builder.Build();

        return cont.Resolve<AnalyzerRunner>();
    }
}
