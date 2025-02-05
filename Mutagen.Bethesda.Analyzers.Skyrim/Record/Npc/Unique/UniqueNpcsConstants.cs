using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public static class UniqueNpcsConstants
{
    public static bool IsEligibleForTest(this INpcGetter npc)
    {
        return npc.IsUnique() && npc.HasKeyword(FormKeys.SkyrimSE.Skyrim.Keyword.ActorTypeNPC);
    }
}
