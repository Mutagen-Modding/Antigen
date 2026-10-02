using Antigen.Reporting.Handlers;

namespace Antigen;

public interface IAnalyzerRunner
{
    /// <summary>
    /// Run the analysis
    /// </summary>
    /// <returns>Analysis results for topics found in the run</returns>
    IAsyncEnumerable<AnalyzerResult> Analyze();
}
