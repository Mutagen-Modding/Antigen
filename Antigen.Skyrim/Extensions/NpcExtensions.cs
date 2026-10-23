using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Antigen.Skyrim.Extensions;

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

    public static bool HasKeyword(this INpcGetter npc, ILinkCache linkCache, IFormLinkGetter<IKeywordGetter> keyword)
    {
        if (npc.HasKeyword(keyword)) return true;

        return npc.Race.TryResolve(linkCache, out var race) && race.HasKeyword(keyword);
    }

    public static bool IsActorTypeNpc(this INpcGetter npc, ILinkCache linkCache)
    {
        return npc.HasKeyword(linkCache, FormKeys.SkyrimSE.Skyrim.Keyword.ActorTypeNPC);
    }

    public static bool IsActorTypeCreature(this INpcGetter npc, ILinkCache linkCache)
    {
        return npc.HasKeyword(linkCache, FormKeys.SkyrimSE.Skyrim.Keyword.ActorTypeCreature);
    }

    public static bool IsUnique(this INpcGetter npc)
    {
        return npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.Unique);
    }

    public static bool IsGhost(this INpcGetter npc)
    {
        return npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.IsGhost)
         || (npc.Keywords is not null && npc.Keywords.Contains(FormKeys.SkyrimSE.Skyrim.Keyword.ActorTypeGhost));
    }

    public static Skill? GetTrainerType(this INpcGetter npc, ILinkCache linkCache)
    {
        if (!npc.Class.TryResolve(linkCache, out var npcClass)) return null;

        // A trainer type of 0 means the NPC does not train any skills, so return null in that case
        if (npcClass.MaxTrainingLevel == 0) return null;

        return npcClass.Teaches;
    }

    public static int GetBaseSkillLevel(this INpcGetter npc, Skill skill, ILinkCache linkCache, int iAVDSkillStart = 15)
    {
        if (!npc.Race.TryResolve(linkCache, out var race)) return 0;

        var raceBoost = race.GetSkillBoost(skill);

        // Base Skill = iAVDSkillStart + race boosts
        return iAVDSkillStart + raceBoost;
    }

    public static int GetSkillLevel(this INpcGetter npc, Skill skill, int npcLevel, ILinkCache linkCache)
    {
        if (npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.AutoCalcStats))
        {
            // Get auto-calculated skill level based on class
            var baseSkillLevel = npc.GetBaseSkillLevel(skill, linkCache);

            if (!npc.Class.TryResolve(linkCache, out var npcClass)) return baseSkillLevel;

            var classSkill = npcClass.GetSkillLevel(skill, npcLevel);
            return baseSkillLevel + classSkill;
        }

        // Get explicit skill offset
        if (npc.PlayerSkills is null) return 0;

        if (!npc.PlayerSkills.SkillValues.TryGetValue(skill, out var skillValue)) return 0;
        if (!npc.PlayerSkills.SkillOffsets.TryGetValue(skill, out var skillOffset)) return skillValue;

        return skillValue + skillOffset;
    }

    public static int GetMinimumSkillLevel(this INpcGetter npc, Skill skill, ILinkCache linkCache)
    {
        var minimumLevel = npc.GetMinimumLevel();
        return npc.GetSkillLevel(skill, minimumLevel, linkCache);
    }

    public static short GetMinimumLevel(this INpcGetter npc)
    {
        return npc.Configuration.Level switch
        {
            INpcLevel npcLevel => npcLevel.Level,
            IPcLevelMult => npc.Configuration.CalcMinLevel,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static IEnumerable<IItemGetter> SampleItems(this INpcGetter npc, ILinkCache linkCache, int level, bool useChanceNone, Random random)
    {
        // Handle template npcs
        if (npc.Configuration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.Inventory) && npc.Template.TryResolve(linkCache, out var template))
        {
            return template switch
            {
                INpcGetter templateNpc => templateNpc.SampleItems(linkCache, level, useChanceNone, random),
                ILeveledNpcGetter leveledNpc => leveledNpc.SampleNpc(linkCache, level, random)?
                        .SampleItems(linkCache, level, useChanceNone, random)
                 ?? [],
                _ => []
            };
        }

        // Handle regular npcs
        if (npc.Items is null) return [];

        return npc.Items
            .SelectMany(entry =>
            {
                return entry.Item.Item.TryResolve(linkCache) switch
                {
                    ILeveledItemGetter leveledItem => leveledItem.SampleItems(linkCache, level, useChanceNone, random),
                    {} item => Enumerable.Repeat(item, entry.Item.Count),
                    _ => []
                };
            })
            .WhereNotNull();
    }

    public static long GetAverageGoldValue(this INpcGetter npc, ILinkCache linkCache, int level)
    {
        // Handle template npcs
        if (npc.Configuration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.Inventory) && npc.Template.TryResolve(linkCache, out var template))
        {
            return template switch
            {
                INpcGetter templateNpc => templateNpc.GetAverageGoldValue(linkCache, level),
                ILeveledNpcGetter leveledNpc => leveledNpc.GetAverageGoldValue(linkCache, level),
                _ => 0
            };
        }

        // Handle regular npcs
        if (npc.Items is null) return 0;

        return npc.Items
            .Select(entry =>
            {
                return entry.Item.Item.TryResolve(linkCache) switch
                {
                    ILeveledItemGetter leveledItem => leveledItem.GetAverageGoldValue(linkCache, level),
                    IWeightValueGetter item => item.Value * entry.Item.Count,
                    _ => 0
                };
            })
            .Sum();
    }
}
