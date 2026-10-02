namespace Antigen.Drivers;

public interface IDriverProvider<TDriver>
    where TDriver : IDriver
{
    IReadOnlyCollection<TDriver> Drivers { get; }
}

public class InjectionDriverProvider<TDriver> : IDriverProvider<TDriver>
    where TDriver : IDriver
{
    public IReadOnlyCollection<TDriver> Drivers { get; }

    public InjectionDriverProvider(IEnumerable<TDriver> drivers)
    {
        Drivers = drivers
            .Where(x => x.Applicable)
            .ToArray();
    }
}