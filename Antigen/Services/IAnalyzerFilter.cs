using Mutagen.Bethesda.Analyzers.SDK.Analyzers;

namespace Antigen.Services;

public interface IAnalyzerFilter
{
    bool ShouldAnalyze(IAnalyzer analyzer);
}