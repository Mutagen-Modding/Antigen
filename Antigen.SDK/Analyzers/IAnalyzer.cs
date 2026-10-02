using Antigen.SDK.Topics;

namespace Antigen.SDK.Analyzers;

public interface IAnalyzer
{
    AnalyzerId Id => new(GetType().FullName ?? throw new InvalidOperationException("Analyzer type has no FullName"));
    IEnumerable<TopicDefinition> Topics { get; }
}
