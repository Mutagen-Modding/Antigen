using Mutagen.Bethesda.Analyzers.SDK.Analyzers;

namespace Antigen.Services.Game;

public class AnalyzerFilter : IAnalyzerFilter
{
    public virtual bool ShouldAnalyze(IAnalyzer analyzer) => true;
}
