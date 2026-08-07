using System.Reflection;
using Noggog;

namespace Antigen.Services;

public sealed class VersionProvider : ISingleton
{
    private readonly Lazy<string> _current = new(Query, LazyThreadSafetyMode.ExecutionAndPublication);

    public string Current => _current.Value;

    private static string Query()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        if (version.IsNullOrWhitespace()) return "0.0.0";

        var plusIndex = version.IndexOf('+', StringComparison.OrdinalIgnoreCase);

        return plusIndex == -1 ? version : version[..plusIndex];
    }
}
