namespace Mutagen.Bethesda.Analyzers.SDK.Caches;

public interface IProvideCaches
{
    TAnalyzerCache Resolve<TAnalyzerCache>();
}
