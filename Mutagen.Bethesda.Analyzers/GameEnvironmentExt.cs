using Mutagen.Bethesda.Environments;

namespace Mutagen.Bethesda.Analyzers;

public static class GameEnvironmentExt
{
    public static AnalyzerRunnerBuilder CreateAnalyzerRunner(this IGameEnvironment gameEnvironment)
    {
        return AnalyzerRunnerBuilder.Create(gameEnvironment);
    }
}
