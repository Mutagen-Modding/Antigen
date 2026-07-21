using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;

public class CrimeGoldConditionAnalyzer : IConditionAnalyzer
{
    public static readonly TopicDefinition GetCrimeGoldRunOnPlayer = MutagenTopicBuilder.FromDiscussion(
            545,
            "CrimeGold conditions running on Player with Null Faction Reference",
            Severity.Error)
        .WithoutFormatting("CrimeGold conditions running on player with null faction reference will not work correctly as this would check if the player has committed a crime against themselves");

    public IEnumerable<TopicDefinition> Topics { get; } = [GetCrimeGoldRunOnPlayer];

    public IEnumerable<Type> ConditionTypesOfInterest()
    {
        yield return typeof(IGetCrimeGoldConditionDataGetter);
        yield return typeof(IGetCrimeGoldNonviolentConditionDataGetter);
        yield return typeof(IGetCrimeGoldViolentConditionDataGetter);
    }

    public void AnalyzeCondition(ConditionAnalyzerContext context)
    {
        var param = context.Param;
        var data = context.Condition.Data;
        if (!data.RunsOnPlayer()) return;

        switch (data)
        {
            case IGetCrimeGoldConditionDataGetter getCrimeGold
                when getCrimeGold.Faction.UsesLink() && getCrimeGold.Faction.Link.IsNull:
                param.AddTopic(GetCrimeGoldRunOnPlayer.Format());
                break;
            case IGetCrimeGoldNonviolentConditionDataGetter getCrimeGoldNonViolent
                when getCrimeGoldNonViolent.Faction.UsesLink() && getCrimeGoldNonViolent.Faction.Link.IsNull:
                param.AddTopic(GetCrimeGoldRunOnPlayer.Format());
                break;
            case IGetCrimeGoldViolentConditionDataGetter getCrimeGoldViolent
                when getCrimeGoldViolent.Faction.UsesLink() && getCrimeGoldViolent.Faction.Link.IsNull:
                param.AddTopic(GetCrimeGoldRunOnPlayer.Format());
                break;
        }
    }
}
