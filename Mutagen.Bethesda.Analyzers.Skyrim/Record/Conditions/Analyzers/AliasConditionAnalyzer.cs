using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;

public class AliasConditionAnalyzer : IConditionAnalyzer
{
    public static readonly TopicDefinition<IConditionGetter> AliasWithoutQuest = MutagenTopicBuilder.FromDiscussion(
            611,
            "Alias condition without owning quest",
            Severity.Error)
        .WithFormatting<IConditionGetter>("Condition {0} runs on an alias, but its record is not owned by a quest");

    public static readonly TopicDefinition<IConditionGetter, int, IQuestGetter> InvalidAliasIndex = MutagenTopicBuilder.FromDiscussion(
            610,
            "Invalid alias index",
            Severity.Error)
        .WithFormatting<IConditionGetter, int, IQuestGetter>("Condition {0} runs on alias {1} of quest {2}, which does not exist");

    public IEnumerable<TopicDefinition> Topics { get; } =
    [
        AliasWithoutQuest,
        InvalidAliasIndex,
    ];

    public IEnumerable<Type> ConditionTypesOfInterest()
    {
        yield return typeof(IConditionDataGetter);
    }

    public void AnalyzeCondition(ConditionAnalyzerContext context)
    {
        var condition = context.Condition;

        // Invalid aliases may coexist with other topics on the same condition
        if (condition.Data is IGetIsAliasRefConditionDataGetter getIsAliasRef)
            CheckAlias(context, getIsAliasRef.ReferenceAliasIndex);
        if (condition.Data.RunOnType == Condition.RunOnType.QuestAlias)
            CheckAlias(context, condition.Data.RunOnTypeIndex);
    }

    private static void CheckAlias(ConditionAnalyzerContext context, int index)
    {
        var param = context.Param;
        var quest = param.Record.GetOwningQuest(param.LinkCache);
        if (quest == null)
        {
            param.AddTopic(AliasWithoutQuest.Format(context.Condition));
        }
        else if (quest.GetAlias((uint)index) == null)
        {
            param.AddTopic(InvalidAliasIndex.Format(context.Condition, index, quest));
        }
    }
}
