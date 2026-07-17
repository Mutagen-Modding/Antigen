using Antigen.Models.Analyzer;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;

namespace Antigen.Services.Singleton;

public interface IAnalyzerService
{
    IObservable<StatusUpdate> StatusUpdates { get; }
    Severity MinimumSeverity { get; set; }
    IAsyncEnumerable<AnalyzerResultInfo> AnalyzeAsync(ModKey modKey, CancellationToken cancellationToken = default);
}