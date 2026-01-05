using System.Reactive.Linq;
using System.Reactive.Subjects;
using Autofac;
using Mutagen.Bethesda.Analyzers.Engines;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Noggog.WorkEngine;

namespace Mutagen.Bethesda.Analyzers.Services;

internal class AnalyzerRunner : IAnalyzerRunner
{
    private readonly IWorkDropoff? _workDropoff;
    private readonly IObservable<int?>? _threads;
    private readonly ILifetimeScope _lifetimeScope;

    public delegate AnalyzerRunner Factory(IWorkDropoff? workDropoff, IObservable<int?>? threads);

    public AnalyzerRunner(
        IWorkDropoff? workDropoff,
        IObservable<int?>? threads,
        ILifetimeScope lifetimeScope)
    {
        _workDropoff = workDropoff;
        _threads = threads;
        _lifetimeScope = lifetimeScope;
    }

    private class NumWorkThreadsByObservable(IObservable<int?> numThreads) : INumWorkThreadsController
    {
        public IObservable<int?> NumDesiredThreads { get; } = numThreads;
    }

    private IObservable<AnalyzerResult> AnalyzeInternal()
    {
        var handler = new ReportHandler();
        var workDropoff = _workDropoff;
        WorkConsumer? workConsumer = null;
        if (workDropoff == null)
        {
            var dropoff = new WorkDropoff();
            workConsumer = new WorkConsumer(
                _threads == null ? new NumWorkThreadsUnopinionated() : new NumWorkThreadsByObservable(_threads),
                dropoff,
                dropoff);
            workDropoff = dropoff;
        }

        var s = _lifetimeScope.BeginLifetimeScope(b =>
        {
            b.RegisterInstance(handler)
                .AsImplementedInterfaces();
            b.RegisterInstance(workDropoff)
                .As<IWorkDropoff>()
                .As<IWorkQueue>();
        });
        var engine = s.Resolve<ContextualAnalyzerEngine>();
        return Observable.Merge<AnalyzerResult>(
            handler.Results,
            Observable.FromAsync(async (c) =>
            {
                workConsumer?.Start();
                try
                {
                    await Task.Run(() => engine.Run(c));
                }
                finally
                {
                    handler.MarkComplete();
                    workConsumer?.Dispose();
                }
                return Observable.Empty<AnalyzerResult>();
            }).Switch());
    }

    public IAsyncEnumerable<AnalyzerResult> Analyze()
    {
        return AnalyzeInternal().ToAsyncEnumerable();
    }

    private class ReportHandler : IReportHandler
    {
        public readonly Subject<AnalyzerResult> Results = new();

        public void Dropoff(
            ReportContextParameters parameters,
            ModKey sourceMod,
            IFormLinkIdentifier majorRecord,
            Topic topic)
        {
            Results.OnNext(new AnalyzerResult()
            {
                ModKey = sourceMod,
                Record = majorRecord,
                Topic = topic
            });
        }

        public void Dropoff(
            ReportContextParameters parameters,
            Topic topic)
        {
            Results.OnNext(new AnalyzerResult()
            {
                ModKey = null,
                Record = null,
                Topic = topic
            });
        }

        public void MarkComplete()
        {
            Results.OnCompleted();
        }
    }
}
