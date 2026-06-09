using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record;

public class ConditionAnalyzer : IContextualRecordAnalyzer<ISkyrimMajorRecordGetter>
{
    public static readonly TopicDefinition<string?> InvalidConditionReference = MutagenTopicBuilder.FromDiscussion(
            213,
            "Condition Runs on Null Reference",
            Severity.Error)
        .WithFormatting<string?>("Condition {0} runs on reference, but reference is null");

    public static readonly TopicDefinition<int, IConditionGetter> InvalidStageCondition = MutagenTopicBuilder.FromDiscussion(
            360,
            "Invalid Quest Stage referenced in Condition",
            Severity.Error)
        .WithFormatting<int, IConditionGetter>("Quest stage {0} referenced in condition {0} is invalid");

    public static readonly TopicDefinition<INpcGetter> GetDeadCondition = MutagenTopicBuilder.FromDiscussion(
            361,
            "GetDead condition used on unique npc",
            Severity.Warning)
        .WithFormatting<INpcGetter>("GetDead used on unique npc {0} instead of GetDeadCount");

    public static readonly TopicDefinition GetCurrentTimeConditionWithOrOnDayBreak = MutagenTopicBuilder.FromDiscussion(
            543,
            "GetCurrentTime conditions with OR operator are always true",
            Severity.Error)
        .WithoutFormatting("GetCurrentTime conditions with OR operator are always true");

    public static readonly TopicDefinition GetCurrentTimeConditionWithAndOnDayBreak = MutagenTopicBuilder.FromDiscussion(
            544,
            "GetCurrentTime conditions with AND operator on Day Break are never true",
            Severity.Error)
        .WithoutFormatting("GetCurrentTime conditions with AND operator on day break can never be true");

    public static readonly TopicDefinition GetCrimeGoldRunOnPlayer = MutagenTopicBuilder.FromDiscussion(
            545,
            "CrimeGold conditions running on Player with Null Faction Reference",
            Severity.Error)
        .WithoutFormatting("CrimeGold conditions running on player with null faction reference will not work correctly as this would check if the player has committed a crime against themselves");

    public static readonly TopicDefinition<ILeveledItemGetter> LeveledItemParameter = MutagenTopicBuilder.FromDiscussion(
            570,
            "Leveled item parameter",
            Severity.Error)
        .WithFormatting<ILeveledItemGetter>("Condition used with leveled item {0} as parameter");

    public static readonly TopicDefinition<IConditionGetter, IFormLinkGetter<IRaceGetter>> NoVampireRace = MutagenTopicBuilder.DevelopmentTopic(
            "No vampire condition",
            Severity.Warning)
        .WithFormatting<IConditionGetter, IFormLinkGetter<IRaceGetter>>("Condition {0} checks for mortal race {1} but not its vampire equivalent");

    public IEnumerable<TopicDefinition> Topics { get; } =
    [
        InvalidConditionReference,
        InvalidStageCondition,
        GetDeadCondition,
        GetCurrentTimeConditionWithOrOnDayBreak,
        GetCurrentTimeConditionWithAndOnDayBreak,
        GetCrimeGoldRunOnPlayer,
        LeveledItemParameter,
        NoVampireRace,
    ];

    private static readonly Dictionary<IFormLinkGetter<IRaceGetter>, IFormLinkGetter<IRaceGetter>> VampireRaceLookup = new()
    {
        { FormKeys.SkyrimSE.Skyrim.Race.ArgonianRace, FormKeys.SkyrimSE.Skyrim.Race.ArgonianRaceVampire },
        { FormKeys.SkyrimSE.Skyrim.Race.BretonRace, FormKeys.SkyrimSE.Skyrim.Race.BretonRaceVampire },
        { FormKeys.SkyrimSE.Skyrim.Race.DarkElfRace, FormKeys.SkyrimSE.Skyrim.Race.DarkElfRaceVampire },
        { FormKeys.SkyrimSE.Skyrim.Race.HighElfRace, FormKeys.SkyrimSE.Skyrim.Race.HighElfRaceVampire },
        { FormKeys.SkyrimSE.Skyrim.Race.ImperialRace, FormKeys.SkyrimSE.Skyrim.Race.ImperialRaceVampire },
        { FormKeys.SkyrimSE.Skyrim.Race.KhajiitRace, FormKeys.SkyrimSE.Skyrim.Race.KhajiitRaceVampire },
        { FormKeys.SkyrimSE.Skyrim.Race.NordRace, FormKeys.SkyrimSE.Skyrim.Race.NordRaceVampire },
        { FormKeys.SkyrimSE.Skyrim.Race.OrcRace, FormKeys.SkyrimSE.Skyrim.Race.OrcRaceVampire },
        { FormKeys.SkyrimSE.Skyrim.Race.RedguardRace, FormKeys.SkyrimSE.Skyrim.Race.RedguardRaceVampire },
        { FormKeys.SkyrimSE.Skyrim.Race.WoodElfRace, FormKeys.SkyrimSE.Skyrim.Race.WoodElfRaceVampire },
    };

    static IFormLinkGetter<IRaceGetter> GetComparisonRace(IConditionDataGetter data)
    {
        return data switch
        {
            IGetIsRaceConditionDataGetter getRace => getRace.Race.Link,
            IGetPCIsRaceConditionDataGetter getRace => getRace.Race.Link,
            _ => FormLink<IRaceGetter>.Null
        };
    }

    static bool HasVampireCondition(IConditionGetter condition, IFormLinkGetter<IRaceGetter> vampireRace, IEnumerable<IConditionGetter> allConditions)
    {
        return allConditions.Any(
            c => c.Data.Function == condition.Data.Function
            && c.Data.RunOnType == condition.Data.RunOnType
            && c.Data.Reference.Equals(condition.Data.Reference)
            && GetComparisonRace(c.Data).Equals(vampireRace));
    }

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ISkyrimMajorRecordGetter> param)
    {
        var conditions = param.Record.GetConditions()?.ToArray();
        if (conditions is null) return;

        for (var i = 0; i < conditions.Length; i++)
        {
            var condition = conditions[i];
            switch (condition.Data)
            {
                case { RunOnType: Condition.RunOnType.Reference, Reference.IsNull: true }:
                    switch (condition.Data)
                    {
                        case IGetEventDataConditionDataGetter getEventData:
                            param.AddTopic(
                                InvalidConditionReference.Format(getEventData.Function.ToString()));
                            break;
                        case {} conditionData:
                            param.AddTopic(
                                InvalidConditionReference.Format(conditionData.Function.ToString()));
                            break;
                    }
                    break;
                case IGetDeadConditionDataGetter
                    when condition.Data.RunOnType == Condition.RunOnType.Reference
                         && condition.Data.Reference.TryResolve<IPlacedNpcGetter>(param.LinkCache, out var placedNpc)
                         && placedNpc.Base.TryResolve(param.LinkCache, out var npc)
                         && npc.IsUnique():
                    param.AddTopic(
                        GetDeadCondition.Format(npc));
                    break;
                case IGetStageConditionDataGetter getStage:
                {
                    if (condition is IConditionFloatGetter floatCondition
                        && getStage.Quest.UsesLink() && getStage.Quest.Link.TryResolve(param.LinkCache, out var quest)
                        && floatCondition.ComparisonValue != 0
                        && quest.Stages.All(s => s.Index != (int)floatCondition.ComparisonValue))
                    {
                        param.AddTopic(
                            InvalidStageCondition.Format((int)floatCondition.ComparisonValue, condition));
                    }
                    break;
                }
                case IGetStageDoneConditionDataGetter getStageDone
                    when getStageDone.Quest.UsesLink() && getStageDone.Quest.Link.TryResolve(param.LinkCache, out var quest2)
                                                       && quest2.Stages.All(s => s.Index != getStageDone.Stage):
                    param.AddTopic(
                        InvalidStageCondition.Format(getStageDone.Stage, condition));
                    break;
                case IGetCurrentTimeConditionDataGetter when condition is IConditionFloatGetter currentFloatCondition: {
                    if (i + 1 >= conditions.Length) break;

                    var nextCondition = conditions[i + 1];
                    if (nextCondition is not IConditionFloatGetter { Data: IGetCurrentTimeConditionDataGetter } nextFloatCondition) break;

                    var firstGreater = currentFloatCondition.CompareOperator is CompareOperator.GreaterThan or CompareOperator.GreaterThanOrEqualTo;
                    var thenLess = nextFloatCondition.CompareOperator is CompareOperator.LessThan or CompareOperator.LessThanOrEqualTo;
                    var firstLess = currentFloatCondition.CompareOperator is CompareOperator.LessThan or CompareOperator.LessThanOrEqualTo;
                    var thenGreater = nextFloatCondition.CompareOperator is CompareOperator.GreaterThan or CompareOperator.GreaterThanOrEqualTo;

                    if (currentFloatCondition.Flags.HasFlag(Condition.Flag.OR))
                    {
                        if (firstGreater && thenLess && currentFloatCondition.ComparisonValue < nextFloatCondition.ComparisonValue)
                        {
                            param.AddTopic(GetCurrentTimeConditionWithOrOnDayBreak.Format());
                        }

                        if (firstLess && thenGreater && currentFloatCondition.ComparisonValue > nextFloatCondition.ComparisonValue)
                        {
                            param.AddTopic(GetCurrentTimeConditionWithOrOnDayBreak.Format());
                        }
                    }
                    else
                    {
                        if (firstGreater && thenLess && currentFloatCondition.ComparisonValue >= nextFloatCondition.ComparisonValue)
                        {
                            param.AddTopic(GetCurrentTimeConditionWithAndOnDayBreak.Format());
                        }

                        if (firstLess && thenGreater && currentFloatCondition.ComparisonValue <= nextFloatCondition.ComparisonValue)
                        {
                            param.AddTopic(GetCurrentTimeConditionWithAndOnDayBreak.Format());
                        }
                    }

                    break;
                }
                case IGetCrimeGoldConditionDataGetter getCrimeGold:
                    if (condition.Data.RunsOnPlayer() && getCrimeGold.Faction.UsesLink() && getCrimeGold.Faction.Link.IsNull) {
                        param.AddTopic(GetCrimeGoldRunOnPlayer.Format());
                    }

                    break;
                case IGetCrimeGoldNonviolentConditionDataGetter getCrimeGoldNonViolent:
                    if (condition.Data.RunsOnPlayer() && getCrimeGoldNonViolent.Faction.UsesLink() && getCrimeGoldNonViolent.Faction.Link.IsNull) {
                        param.AddTopic(GetCrimeGoldRunOnPlayer.Format());
                    }

                    break;
                case IGetCrimeGoldViolentConditionDataGetter getCrimeGoldViolent:
                    if (condition.Data.RunsOnPlayer() && getCrimeGoldViolent.Faction.UsesLink() && getCrimeGoldViolent.Faction.Link.IsNull) {
                        param.AddTopic(GetCrimeGoldRunOnPlayer.Format());
                    }

                    break;
                case IGetItemCountConditionData getItemCount
                    when getItemCount.ItemOrList.Link.TryResolve<ILeveledItemGetter>(param.LinkCache, out var leveledItem):
                    param.AddTopic(LeveledItemParameter.Format(leveledItem));
                    break;
                case IGetEquippedConditionDataGetter getEquipped
                    when getEquipped.ItemOrList.Link.TryResolve<ILeveledItemGetter>(param.LinkCache, out var leveledItem):
                    param.AddTopic(LeveledItemParameter.Format(leveledItem));
                    break;

                case IGetIsRaceConditionDataGetter getRace
                    when VampireRaceLookup.TryGetValue(getRace.Race.Link, out var vampire):
                    if (!HasVampireCondition(condition, vampire, conditions))
                        param.AddTopic(NoVampireRace.Format(condition, getRace.Race.Link));
                    break;
                case IGetPCIsRaceConditionDataGetter getRace
                    when VampireRaceLookup.TryGetValue(getRace.Race.Link, out var vampire):
                    if (!HasVampireCondition(condition, vampire, conditions))
                        param.AddTopic(NoVampireRace.Format(condition, getRace.Race.Link));
                    break;
            }
        }
    }

    public IEnumerable<Func<ISkyrimMajorRecordGetter, object?>> FieldsOfInterest()
    {
        yield break;
    }
}
