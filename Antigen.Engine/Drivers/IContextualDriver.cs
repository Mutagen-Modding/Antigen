namespace Antigen.Drivers;

public interface IContextualDriver : IDriver
{
    Task Drive(ContextualDriverParams driverParams);
}
