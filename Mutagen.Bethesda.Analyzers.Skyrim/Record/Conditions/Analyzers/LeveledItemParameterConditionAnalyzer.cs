using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;

public class LeveledItemParameterConditionAnalyzer : IConditionAnalyzer
{
    public static readonly TopicDefinition<ILeveledItemGetter> LeveledItemParameter = MutagenTopicBuilder.FromDiscussion(
            570,
            "Leveled item parameter",
            Severity.Error)
        .WithFormatting<ILeveledItemGetter>("Condition used with leveled item {0} as parameter");

    public IEnumerable<TopicDefinition> Topics { get; } = [LeveledItemParameter];

    public IEnumerable<Type> ConditionTypesOfInterest()
    {
        yield return typeof(IGetItemCountConditionData);
        yield return typeof(IGetEquippedConditionDataGetter);
    }

    public void AnalyzeCondition(ConditionAnalyzerContext context)
    {
        var param = context.Param;
        switch (context.Condition.Data)
        {
            case IGetItemCountConditionData getItemCount
                when getItemCount.ItemOrList.Link.TryResolve<ILeveledItemGetter>(param.LinkCache, out var leveledItem):
                param.AddTopic(LeveledItemParameter.Format(leveledItem));
                break;
            case IGetEquippedConditionDataGetter getEquipped
                when getEquipped.ItemOrList.Link.TryResolve<ILeveledItemGetter>(param.LinkCache, out var leveledItem):
                param.AddTopic(LeveledItemParameter.Format(leveledItem));
                break;
        }
    }
}
