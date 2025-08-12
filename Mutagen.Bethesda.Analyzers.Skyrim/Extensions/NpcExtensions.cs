using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

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

    public static Skill? GetTrainerType(this INpcGetter npc, ILinkCache linkCache)
    {
        const string JobTrainerPart = "JobTrainer";

        return npc.Factions
            .Select(x => x.Faction.TryResolve(linkCache))
            .WhereNotNull()
            .Select<IFactionGetter, Skill?>(f =>
            {
                if (f.EditorID is null) return null;

                var index = f.EditorID.IndexOf(JobTrainerPart, StringComparison.OrdinalIgnoreCase);
                if (index < 0) return null;

                // If the EditorID ends with "JobTrainer", it is not a specialization
                if (index + JobTrainerPart.Length >= f.EditorID.Length) return null;

                // Extract the specialization part
                var specialization = f.EditorID[(index + JobTrainerPart.Length)..];
                specialization = specialization.TrimStringFromEnd("Faction");
                if (Enum.TryParse<Skill>(specialization, out var actorValue)) return actorValue;

                return null;

            })
            .WhereNotNull()
            .FirstOrDefault();
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
}
