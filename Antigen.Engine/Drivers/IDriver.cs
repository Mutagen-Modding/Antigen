using Antigen.SDK.Analyzers;

namespace Antigen.Drivers;

public interface IDriver
{
    bool Applicable { get; }
    IEnumerable<IAnalyzer> Analyzers { get; }
}