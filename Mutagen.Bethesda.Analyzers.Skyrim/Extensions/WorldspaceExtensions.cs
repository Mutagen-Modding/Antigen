using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class WorldspaceExtensions
{
    public static IEnumerable<ILocationGetter> GetWorldLocations(this IWorldspaceGetter world, ILinkCache linkCache)
    {
        var worldLocations = new List<ILocationGetter>();

        var worldLocation = world.Location.TryResolve(linkCache);
        while (worldLocation is not null)
        {
            worldLocations.Add(worldLocation);
            worldLocation = worldLocation.ParentLocation.TryResolve(linkCache);
        }

        return worldLocations;
    }

}
