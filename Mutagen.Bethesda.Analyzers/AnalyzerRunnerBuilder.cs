using System.IO.Abstractions;
using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers;

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
