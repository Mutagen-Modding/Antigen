using Autofac;
using Antigen.SDK.Caches;
using Antigen.Skyrim;

namespace Antigen.Testing.Frameworks;

/// <summary>
/// Resolves every <see cref="ICacheConstructor"/> registered by the game's cache module, so that
/// newly added cache types are picked up automatically rather than hand-listed in each fixture.
/// </summary>
public static class TestCacheConstructors
{
    public static ICacheConstructor[] All { get; } = Build();

    private static ICacheConstructor[] Build()
    {
        var builder = new ContainerBuilder();
        builder.RegisterModule<SkyrimCacheModule>();
        var container = builder.Build();
        return container.Resolve<IEnumerable<ICacheConstructor>>().ToArray();
    }
}
