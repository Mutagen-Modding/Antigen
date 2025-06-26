using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class ClassExtensions
{
    public static int GetSkillLevel(this IClassGetter c, Skill skill, int npcLevel, int iAVDSkillStart = 8)
    {
        if (!c.SkillWeights.TryGetValue(skill, out var skillWeight)) return 0;
        if (skillWeight == 0) return 0;

        // Based on formula from https://ck.uesp.net/wiki/Class#Attribute_and_Skill_Weights
        // Total Skill Points = (NPC Level - 1) * iAVDSkillStart
        // This Skill = This Skill Base + (Total Skill Points * (This Skill Weight / Sum of Skill Weights))
        var totalSkillPoints = (npcLevel - 1) * iAVDSkillStart;
        var allSkillWeights = c.SkillWeights.Values.Sum(x => x);
        var relativeSkillWeight = skillWeight / (float) allSkillWeights;
        return (int) Math.Round(totalSkillPoints * relativeSkillWeight);
    }
}
