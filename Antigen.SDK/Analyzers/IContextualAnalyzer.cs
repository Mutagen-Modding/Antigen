namespace Antigen.SDK.Analyzers;

public interface IContextualAnalyzer : IAnalyzer
{
    void Analyze(ContextualAnalyzerParams param);
}
