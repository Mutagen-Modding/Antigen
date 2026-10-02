using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Npc.Unique;

public static class UniqueNpcsConstants
{
    public static bool IsUniqueActorType(this INpcGetter npc, ILinkCache linkCache)
    {
        return npc.IsUnique() && npc.IsActorTypeNpc(linkCache);
    }
}
