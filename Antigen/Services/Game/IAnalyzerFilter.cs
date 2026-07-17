using Mutagen.Bethesda.Analyzers.SDK.Analyzers;

namespace Antigen.Services.Game;

public interface IAnalyzerFilter
{
    bool ShouldAnalyze(IAnalyzer analyzer);
}