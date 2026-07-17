using Antigen.Models.Analyzer;
using Mutagen.Bethesda.Analyzers.SDK.Topics;

namespace Antigen.Services.Transient;

public interface IModWatcher : IDisposable
{
    IObservable<IObservable<AnalyzerResultInfo>> AnalysisCompleted { get; }
    IObservable<StatusUpdate> StatusUpdates { get; }
    Severity MinimumSeverity { get; set; }
    void Start();
    void Stop();
}