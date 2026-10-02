using Antigen.Config.Topic;
using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;

namespace Antigen.Drivers;

public interface IAnalyzerProvider<out TAnalyzer>
    where TAnalyzer : IAnalyzer
{
    IEnumerable<TAnalyzer> GetAnalyzers();
}

public class FilteredAnalyzerProvider<TAnalyzer>(TAnalyzer[] analyzers, ISeverityLookup severityLookup) : IAnalyzerProvider<TAnalyzer>
    where TAnalyzer : IAnalyzer
{

    public IEnumerable<TAnalyzer> GetAnalyzers()
    {
        return analyzers
            .Where(a => a.Topics.Any(topic => severityLookup.LookupSeverity(topic) != Severity.None));
    }
}
