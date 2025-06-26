using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class RaceExtensions
{
    public static bool IsChildRace(this IRaceGetter race)
    {
        return (race.Flags & Race.Flag.Child) != 0;
    }

    public static int GetSkillBoost(this IRaceGetter race, Skill skill)
    {
        var actorValue = (ActorValue)skill;
        if (race.SkillBoost0.Skill == actorValue) return race.SkillBoost0.Boost;
        if (race.SkillBoost1.Skill == actorValue) return race.SkillBoost1.Boost;
        if (race.SkillBoost2.Skill == actorValue) return race.SkillBoost2.Boost;
        if (race.SkillBoost3.Skill == actorValue) return race.SkillBoost3.Boost;
        if (race.SkillBoost4.Skill == actorValue) return race.SkillBoost4.Boost;
        if (race.SkillBoost5.Skill == actorValue) return race.SkillBoost5.Boost;
        if (race.SkillBoost6.Skill == actorValue) return race.SkillBoost6.Boost;

        return 0;
    }
}
