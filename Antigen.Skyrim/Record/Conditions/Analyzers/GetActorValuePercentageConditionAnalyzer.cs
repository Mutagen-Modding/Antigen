using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Conditions.Analyzers;

public sealed class GetActorValuePercentageConditionAnalyzer : IConditionAnalyzer
{
    public static readonly TopicDefinition<float> GetActorValuePercentageRunOnPlayer = MutagenTopicBuilder.FromDiscussion(
            650,
            "GetActorValuePercentage condition is compared to value outside of range 0.0 - 1.0",
            Severity.Error)
        .WithFormatting<float>("GetActorValuePercentage condition is compared to {0} which is outside of range 0.0 - 1.0");

    public static readonly TopicDefinition<IFormLinkGetter<IGlobalGetter>, float> GetActorValuePercentageRunOnPlayerGlobal = MutagenTopicBuilder.FromDiscussion(
            651,
            "GetActorValuePercentage condition is compared to global outside of range 0.0 - 1.0",
            Severity.Error)
        .WithFormatting<IFormLinkGetter<IGlobalGetter>, float>("GetActorValuePercentage condition is compared to global {0} which has the initial value {1} outside of range 0.0 - 1.0");

    public IEnumerable<TopicDefinition> Topics { get; } = [GetActorValuePercentageRunOnPlayer];

    public IEnumerable<Type> ConditionTypesOfInterest()
    {
        yield return typeof(IGetActorValuePercentConditionDataGetter);
    }

    public void AnalyzeCondition(ConditionAnalyzerContext context)
    {
        switch (context.Condition) {
            case IConditionFloatGetter { Data: IGetActorValuePercentConditionDataGetter, ComparisonValue: > 1.0f or < 0.0f } condition:
                context.Param.AddTopic(
                    GetActorValuePercentageRunOnPlayer.Format(condition.ComparisonValue));
                break;
            case IConditionGlobalGetter { Data: IGetActorValuePercentConditionDataGetter } condition
                when condition.GetInitialValue(context.Param.LinkCache) is {} global and (> 1.0f or < 0.0f):
                context.Param.AddTopic(
                    GetActorValuePercentageRunOnPlayerGlobal.Format(condition.ComparisonValue, global));
                break;
        }
    }
}
