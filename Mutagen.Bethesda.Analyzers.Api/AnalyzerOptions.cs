using Mutagen.Bethesda.Analyzers.Config.Topic;
using Mutagen.Bethesda.Analyzers.SDK.Topics;

namespace Mutagen.Bethesda.Analyzers.Api;

public class AnalyzerOptions : IMinimumSeverityConfiguration
{
    public TopicConfig TopicConfig { get; set; } = new();
    public Severity MinimumSeverity { get; set; } = Severity.Suggestion;
    public int? NumberOfThreads { get; set; } = null;
}
