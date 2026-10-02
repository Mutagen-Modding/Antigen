using Antigen.Drivers;

namespace Antigen.Engines;

public interface IEngine
{
    IEnumerable<IDriver> Drivers { get; }
}