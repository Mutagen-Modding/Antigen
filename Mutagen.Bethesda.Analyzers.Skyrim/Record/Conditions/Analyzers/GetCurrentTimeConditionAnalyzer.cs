using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;

public class GetCurrentTimeConditionAnalyzer : IConditionAnalyzer
{
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

    public IEnumerable<TopicDefinition> Topics { get; } =
    [
        GetCurrentTimeConditionWithOrOnDayBreak,
        GetCurrentTimeConditionWithAndOnDayBreak,
    ];

    public IEnumerable<Type> ConditionTypesOfInterest()
    {
        yield return typeof(IGetCurrentTimeConditionDataGetter);
    }

    public void AnalyzeCondition(ConditionAnalyzerContext context)
    {
        if (context.Condition is not IConditionFloatGetter currentFloatCondition) return;

        var conditions = context.Conditions;
        var i = context.Index;
        if (i + 1 >= conditions.Count) return;

        var nextCondition = conditions[i + 1];
        if (nextCondition is not IConditionFloatGetter { Data: IGetCurrentTimeConditionDataGetter } nextFloatCondition) return;

        var param = context.Param;
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
    }
}
