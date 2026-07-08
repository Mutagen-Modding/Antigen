using Autofac;
using Mutagen.Bethesda.Analyzers.SDK.Caches;
using Mutagen.Bethesda.Analyzers.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Testing.Frameworks;

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
