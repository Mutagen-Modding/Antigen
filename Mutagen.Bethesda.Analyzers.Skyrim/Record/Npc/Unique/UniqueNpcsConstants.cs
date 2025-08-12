using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public static class UniqueNpcsConstants
{
    public static bool IsEligibleForTest(this INpcGetter npc, ILinkCache linkCache)
    {
        return npc.IsUnique() && npc.IsActorTypeNpc(linkCache);
    }
}
