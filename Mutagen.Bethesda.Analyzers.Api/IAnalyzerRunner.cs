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
using Mutagen.Bethesda.Analyzers.Skyrim;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Environments.DI;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Api;

public static class AnalyzerExtensions
{
    public static IAnalyzerRunner Analyzer(this IGameEnvironment gameEnvironment, AnalyzerOptions? options = null)
    {
        return new AnalyzerRunner(gameEnvironment, options);
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
    private readonly IGameEnvironment _gameEnvironment;
    private readonly IWorkDropoff _workDropoff;
    private readonly IContainer _container;

    public IDriverProvider<IContextualDriver> ContextualModDrivers { get; }
    public IDriverProvider<IIsolatedDriver> IsolatedModDrivers { get; }

    public AnalyzerRunner(IGameEnvironment gameEnvironment, AnalyzerOptions? options = null)
    {
        _gameEnvironment = gameEnvironment;

        options ??= new AnalyzerOptions();
        var builder = new ContainerBuilder();

        builder.RegisterInstance(new FileSystem()).As<IFileSystem>();

        builder.RegisterInstance(new GameReleaseInjection(gameEnvironment.GameRelease))
            .AsSelf()
            .AsImplementedInterfaces();

        builder.RegisterModule<MainModule>();
        builder.RegisterModule<SkyrimAnalyzerModule>();

        // Custom topic and analyzer config
        builder.RegisterInstance(options.TopicConfig)
            .AsSelf()
            .AsImplementedInterfaces();

        builder.RegisterInstance(new InjectedBlacklistedModsProvider([]))
            .AsSelf()
            .AsImplementedInterfaces();

        builder.RegisterInstance(GameConstants.Get(gameEnvironment.GameRelease))
            .As<GameConstants>();

        builder.RegisterInstance(options)
            .AsImplementedInterfaces();

        builder.RegisterInstance(new NumWorkThreadsConstant(options.NumberOfThreads))
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
            _gameEnvironment.LinkCache,
            _gameEnvironment.LoadOrder,
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
            new ReportContextParameters(_gameEnvironment.LinkCache),
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
            _gameEnvironment.LinkCache,
            _gameEnvironment.LoadOrder,
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
