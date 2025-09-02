using System.IO.Abstractions;
using Autofac;
using Mutagen.Bethesda.Analyzers.Autofac;
using Mutagen.Bethesda.Analyzers.Config.Run;
using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.Drivers;
using Mutagen.Bethesda.Analyzers.Reporting.Drops;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Api;

public class AnalyzerRunnerBuilder
{
    private readonly GameRelease _gameRelease;
    private readonly ILinkCache _linkCache;
    private readonly ILoadOrderGetter<IModListingGetter<IModGetter>> _loadOrder;

    private IFileSystem? _fileSystem;
    private INumWorkThreadsController? _numWorkThreadsController;
    private Severity _minimumSeverity = Severity.Suggestion;
    private TopicConfig? _topicConfig;

    private AnalyzerRunnerBuilder(
        GameRelease gameRelease,
        ILinkCache linkCache,
        ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder)
    {
        _gameRelease = gameRelease;
        _linkCache = linkCache;
        _loadOrder = loadOrder;
    }

    public static AnalyzerRunnerBuilder Create(
        GameRelease gameRelease,
        ILinkCache linkCache,
        ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder)
    {
        return new AnalyzerRunnerBuilder(gameRelease, linkCache, loadOrder);
    }

    public static AnalyzerRunnerBuilder Create(
        IGameEnvironment gameEnvironment)
    {
        return new AnalyzerRunnerBuilder(
            gameEnvironment.GameRelease,
            gameEnvironment.LinkCache,
            gameEnvironment.LoadOrder);
    }

    public AnalyzerRunnerBuilder WithFileSystem(IFileSystem fileSystem) {
        _fileSystem = fileSystem;
        return this;
    }

    public AnalyzerRunnerBuilder WithThreads(int threads) {
        _numWorkThreadsController = new NumWorkThreadsConstant(threads);
        return this;
    }

    public AnalyzerRunnerBuilder WithThreads(IObservable<int> threads)
    {
        // TODO: replace with implementation accepting an observable
        _numWorkThreadsController = new NumWorkThreadsConstant(-1);
        return this;
    }

    public AnalyzerRunnerBuilder WithMinimumSeverity(Severity minimumSeverity)
    {
        _minimumSeverity = minimumSeverity;
        return this;
    }

    public AnalyzerRunnerBuilder WithTopicConfig(TopicConfig? topicConfig)
    {
        _topicConfig = topicConfig;
        return this;
    }

    public IAnalyzerRunner Build()
    {
        return new AnalyzerRunner(
            _fileSystem ?? new FileSystem(),
            _gameRelease,
            _linkCache,
            _loadOrder,
            _topicConfig ?? new TopicConfig(),
            _minimumSeverity,
            _numWorkThreadsController ?? new NumWorkThreadsConstant(null));
    }
}

public static class AnalyzerExtensions
{
    public static AnalyzerRunnerBuilder CreateAnalyzerRunner(this IGameEnvironment gameEnvironment)
    {
        return AnalyzerRunnerBuilder.Create(gameEnvironment);
    }
}

public interface IAnalyzerRunner
{
    /// <summary>
    /// Analyze a mod for issues.
    /// </summary>
    /// <param name="mod">Mod to analyze</param>
    /// <returns>Analysis results for topics found in the mod </returns>
    Task<IEnumerable<AnalyzerResult>> Analyze(IModGetter mod);

    /// <summary>
    /// Analyze a major record for issues.
    /// </summary>
    /// <param name="record">Major record to analyze</param>
    /// <returns>Analysis results for topics found in the record</returns>
    Task<IEnumerable<AnalyzerResult>> Analyze<TMajorRecord>(TMajorRecord record)
        where TMajorRecord : IMajorRecordGetter;
}

public class AnalyzerRunner : IAnalyzerRunner
{
    private readonly IWorkDropoff _workDropoff;
    private readonly IContainer _container;
    private readonly ILinkCache _linkCache;
    private readonly ILoadOrderGetter<IModListingGetter<IModGetter>> _loadOrder;

    public IDriverProvider<IContextualDriver> ContextualModDrivers { get; }
    public IDriverProvider<IIsolatedDriver> IsolatedModDrivers { get; }

    internal AnalyzerRunner(
        IFileSystem fileSystem,
        GameRelease gameRelease,
        ILinkCache linkCache,
        ILoadOrderGetter<IModListingGetter<IModGetter>> loadOrder,
        TopicConfig topicConfig,
        Severity minimumSeverity,
        INumWorkThreadsController numWorkThreadsController)
    {
        _linkCache = linkCache;
        _loadOrder = loadOrder;

        var builder = new ContainerBuilder();

        builder.RegisterInstance(fileSystem).As<IFileSystem>();

        builder.RegisterInstance(new GameReleaseInjection(gameRelease))
            .AsSelf()
            .AsImplementedInterfaces();

        builder.RegisterModule<MainModule>();
        builder.RegisterModule<SkyrimAnalyzerModule>();

        // Custom topic and analyzer config
        builder.RegisterInstance(topicConfig)
            .AsSelf()
            .AsImplementedInterfaces();

        builder.RegisterInstance(new MinimumSeverityConfiguration(minimumSeverity))
            .AsSelf()
            .AsImplementedInterfaces();

        builder.RegisterInstance(new InjectedBlacklistedModsProvider([]))
            .AsSelf()
            .AsImplementedInterfaces();

        builder.RegisterInstance(GameConstants.Get(gameRelease))
            .As<GameConstants>();

        builder.RegisterInstance(numWorkThreadsController)
            .AsImplementedInterfaces();

        _container = builder.Build();
        _workDropoff = _container.Resolve<IWorkDropoff>();

        ContextualModDrivers = _container.Resolve<IDriverProvider<IContextualDriver>>();
        IsolatedModDrivers = _container.Resolve<IDriverProvider<IIsolatedDriver>>();
    }

    public async Task<IEnumerable<AnalyzerResult>> Analyze(IModGetter mod)
    {
        var collectionReportHandler = new CollectionReportHandler();
        var reportDropbox = GetReportDropbox(collectionReportHandler);
        var consumer = _container.Resolve<IWorkConsumer>();

        consumer.Start();

        // Isolated
        var isolatedParams = new IsolatedDriverParams(
            mod.ToUntypedImmutableLinkCache(),
            reportDropbox,
            mod,
            new ModPath(mod.ModKey, ""),
            CancellationToken.None);

        var isolated = Task.WhenAll(IsolatedModDrivers.Drivers
            .Where(d => d.Applicable)
            .Select(d => d.Drive(isolatedParams)));

        // Contextual
        var contextualParams = new ContextualDriverParams(
            _linkCache,
            _loadOrder,
            reportDropbox,
            CancellationToken.None);

        var contextual = Task.WhenAll(ContextualModDrivers.Drivers
            .Where(d => d.Applicable)
            .Select(d => d.Drive(contextualParams)));

        await Task.WhenAll(isolated, contextual);

        // Collect results
        return collectionReportHandler.Results;
    }

    public async Task<IEnumerable<AnalyzerResult>> Analyze<TMajorRecord>(TMajorRecord record)
        where TMajorRecord : IMajorRecordGetter
    {
        var collectionReportHandler = new CollectionReportHandler();
        var reportDropbox = GetReportDropbox(collectionReportHandler);
        var consumer = _container.Resolve<IWorkConsumer>();

        consumer.Start();

        // Isolated
        var isolatedParams = new IsolatedRecordAnalyzerParams<TMajorRecord>(
            record.FormKey.ModKey,
            record,
            new ReportContextParameters(_linkCache),
            reportDropbox);

        var isolatedAnalyzerProvider = _container.Resolve<IAnalyzerProvider<IIsolatedRecordAnalyzer<TMajorRecord>>>();
        var isolated = Task.WhenAll(isolatedAnalyzerProvider.GetAnalyzers().Select(analyzer =>
        {
            return _workDropoff.EnqueueAndWait(() =>
            {
                analyzer.AnalyzeRecord(isolatedParams with
                {
                    AnalyzerType = analyzer.GetType()
                });
            }, CancellationToken.None);
        }));

        // Contextual
        var contextualParams = new ContextualRecordAnalyzerParams<TMajorRecord>(
            _linkCache,
            _loadOrder,
            record.FormKey.ModKey,
            record,
            reportDropbox);

        var contextualAnalyzerProvider = _container.Resolve<IAnalyzerProvider<IContextualRecordAnalyzer<TMajorRecord>>>();
        var contextual = Task.WhenAll(contextualAnalyzerProvider.GetAnalyzers().Select(analyzer =>
        {
            return _workDropoff.EnqueueAndWait(() =>
            {
                analyzer.AnalyzeRecord(contextualParams with
                {
                    AnalyzerType = analyzer.GetType()
                });
            }, CancellationToken.None);
        }));

        await Task.WhenAll(isolated, contextual);

        return collectionReportHandler.Results;
    }

    private IReportDropbox GetReportDropbox(CollectionReportHandler collectionReportHandler)
    {
        var lifetimeScope = _container.BeginLifetimeScope(builder =>
        {
            // Last registered runs first
            builder.RegisterType<PassToHandlerReportDropbox>().AsImplementedInterfaces();
            builder.RegisterDecorator<EditorIdEnricher, IReportDropbox>();
            builder.RegisterDecorator<MinimumSeverityFilter, IReportDropbox>();
            builder.RegisterDecorator<SeverityAdjuster, IReportDropbox>();
            builder.RegisterDecorator<DisallowedParametersChecker, IReportDropbox>();
            builder.RegisterDecorator<FilterBlacklistedReports, IReportDropbox>();

            builder.RegisterInstance(collectionReportHandler)
                .AsSelf()
                .AsImplementedInterfaces();
        });

        return lifetimeScope.Resolve<IReportDropbox>();
    }
}
