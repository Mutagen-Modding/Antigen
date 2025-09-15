using System.Reactive.Linq;
using System.Reactive.Subjects;
using Autofac;
using Mutagen.Bethesda.Analyzers.Engines;
using Mutagen.Bethesda.Analyzers.Reporting.Handlers;
using Mutagen.Bethesda.Analyzers.SDK.Drops;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Mutagen.Bethesda.Analyzers;

public interface IAnalyzerRunner
{
    /// <summary>
    /// Run the analysis
    /// </summary>
    /// <returns>Analysis results for topics found in the run</returns>
    IAsyncEnumerable<AnalyzerResult> Analyze();
}

public class AnalyzerRunner : IAnalyzerRunner
{
    private readonly ILifetimeScope _lifetimeScope;

    internal AnalyzerRunner(
        ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    private IObservable<AnalyzerResult> AnalyzeInternal()
    {
        var handler = new ReportHandler();
        var s = _lifetimeScope.BeginLifetimeScope(b =>
        {
            b.RegisterInstance(handler)
                .AsImplementedInterfaces();
        });
        var engine = s.Resolve<ContextualAnalyzerEngine>();
        return Observable.Merge<AnalyzerResult>(
            handler.Results,
            Observable.FromAsync(async (c) =>
            {
                await engine.Run(c);
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
            IMajorRecordIdentifierGetter majorRecord,
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
    }
}
