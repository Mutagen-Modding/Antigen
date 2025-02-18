using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class NpcExtensions
{
    public static bool HasFaction(this INpcGetter npc, ILinkCache linkCache, Predicate<string?> stringCompare)
    {
        foreach (var rankPlacement in npc.Factions)
        {
            if (!linkCache.TryResolve<IFactionGetter>(rankPlacement.Faction.FormKey, out var faction)) continue;

            if (stringCompare(faction.EditorID)) return true;
        }

        return false;
    }

    public static bool HasFaction(this INpcGetter npc, ILinkCache linkCache, string editorId)
    {
        return npc.HasFaction(linkCache, npcEditorId => string.Equals(npcEditorId, editorId, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsUnique(this INpcGetter npc)
    {
        return npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.Unique);
    }
}
