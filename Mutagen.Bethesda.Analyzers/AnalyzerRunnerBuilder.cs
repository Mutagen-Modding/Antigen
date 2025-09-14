using System.IO.Abstractions;
using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers;

public record AnalyzerRunnerBuilder
{
    private readonly GameRelease _gameRelease;
    private readonly ILinkCache _linkCache;
    private readonly ILoadOrderGetter<IModListingGetter<IModGetter>> _loadOrder;

    private IFileSystem? _fileSystem { get; init; }
    private INumWorkThreadsController? _numWorkThreadsController { get; init; }
    private Severity _minimumSeverity { get; init; } = Severity.Suggestion;
    private TopicConfig? _topicConfig { get; init; }

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
