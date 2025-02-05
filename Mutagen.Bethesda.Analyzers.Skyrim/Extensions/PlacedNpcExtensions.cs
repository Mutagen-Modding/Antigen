using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class PlacedNpcExtensions
{
    public static bool HasLocationRefType(this IPlacedNpcGetter placedNpc, FormLink<ILocationReferenceTypeGetter> locRefType)
    {
        return placedNpc.LocationRefTypes is not null
               && placedNpc.LocationRefTypes.Any(r => r.FormKey == locRefType.FormKey);
    }
}
