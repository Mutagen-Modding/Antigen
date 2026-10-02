namespace Antigen.SDK.Caches;

public interface IProvideCaches
{
    TAnalyzerCache Resolve<TAnalyzerCache>();
}
