using Mutagen.Bethesda.Analyzers.Reporting.Handlers;

namespace Mutagen.Bethesda.Analyzers;

public interface IAnalyzerRunner
{
    /// <summary>
    /// Run the analysis
    /// </summary>
    /// <returns>Analysis results for topics found in the run</returns>
    IAsyncEnumerable<AnalyzerResult> Analyze();
}
