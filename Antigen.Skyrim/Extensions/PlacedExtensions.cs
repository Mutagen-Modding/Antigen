using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Extensions;

public static class PlacedExtensions
{
    public static IFormLinkGetter<ILocationGetter> GetPersistLocation(this IPlacedGetter placed)
    {
        return placed switch
        {
            IPlacedObjectGetter obj => obj.PersistentLocation,
            IPlacedNpcGetter npc => npc.PersistentLocation,
            _ => FormLink<ILocationGetter>.Null,
        };
    }
}
