using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;

public class QuestStageConditionAnalyzer : IConditionAnalyzer
{
    public static readonly TopicDefinition<int, IConditionGetter> InvalidStageCondition = MutagenTopicBuilder.FromDiscussion(
            360,
            "Invalid Quest Stage referenced in Condition",
            Severity.Error)
        .WithFormatting<int, IConditionGetter>("Quest stage {0} referenced in condition {1} is invalid");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidStageCondition];

    public IEnumerable<Type> ConditionTypesOfInterest()
    {
        yield return typeof(IGetStageConditionDataGetter);
        yield return typeof(IGetStageDoneConditionDataGetter);
    }

    public void AnalyzeCondition(ConditionAnalyzerContext context)
    {
        var param = context.Param;
        var condition = context.Condition;
        switch (condition.Data)
        {
            case IGetStageConditionDataGetter getStage:
                if (condition is IConditionFloatGetter floatCondition
                    && getStage.Quest.UsesLink() && getStage.Quest.Link.TryResolve(param.LinkCache, out var quest)
                    && floatCondition.ComparisonValue != 0
                    && quest.Stages.All(s => s.Index != (int)floatCondition.ComparisonValue))
                {
                    param.AddTopic(InvalidStageCondition.Format((int)floatCondition.ComparisonValue, condition));
                }

                break;
            case IGetStageDoneConditionDataGetter getStageDone
                when getStageDone.Quest.UsesLink() && getStageDone.Quest.Link.TryResolve(param.LinkCache, out var quest2)
                                                   && quest2.Stages.All(s => s.Index != getStageDone.Stage):
                param.AddTopic(InvalidStageCondition.Format(getStageDone.Stage, condition));
                break;
        }
    }
}
