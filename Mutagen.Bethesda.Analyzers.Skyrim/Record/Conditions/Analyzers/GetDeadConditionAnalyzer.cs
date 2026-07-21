using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;

public class GetDeadConditionAnalyzer : IConditionAnalyzer
{
    public static readonly TopicDefinition<INpcGetter> GetDeadCondition = MutagenTopicBuilder.FromDiscussion(
            361,
            "GetDead condition used on unique npc",
            Severity.Warning)
        .WithFormatting<INpcGetter>("GetDead used on unique npc {0} instead of GetDeadCount");

    public IEnumerable<TopicDefinition> Topics { get; } = [GetDeadCondition];

    public IEnumerable<Type> ConditionTypesOfInterest()
    {
        yield return typeof(IGetDeadConditionDataGetter);
    }

    public void AnalyzeCondition(ConditionAnalyzerContext context)
    {
        var param = context.Param;
        var data = context.Condition.Data;
        if (data.RunOnType == Condition.RunOnType.Reference
            && data.Reference.TryResolve<IPlacedNpcGetter>(param.LinkCache, out var placedNpc)
            && placedNpc.Base.TryResolve(param.LinkCache, out var npc)
            && npc.IsUnique())
        {
            param.AddTopic(GetDeadCondition.Format(npc));
        }
    }
}
