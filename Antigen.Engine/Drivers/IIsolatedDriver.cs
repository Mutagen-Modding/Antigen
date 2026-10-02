namespace Antigen.Drivers;

public interface IIsolatedDriver : IDriver
{
    Task Drive(IsolatedDriverParams driverParams);
}
