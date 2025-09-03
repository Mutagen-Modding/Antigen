using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record;

public class ConditionAnalyzer : IContextualRecordAnalyzer<ISkyrimMajorRecordGetter>
{
    private static readonly TopicDefinition<string?> InvalidConditionReference = MutagenTopicBuilder.FromDiscussion(
            213,
            "Condition Runs on Null Reference",
            Severity.Error)
        .WithFormatting<string?>("Condition {0} runs on reference, but reference is null");

    private static readonly TopicDefinition<int, IConditionGetter> InvalidStageCondition = MutagenTopicBuilder.FromDiscussion(
            360,
            "Invalid Quest Stage referenced in Condition",
            Severity.Error)
        .WithFormatting<int, IConditionGetter>("Quest stage {0} referenced in condition {0} is invalid");

    private static readonly TopicDefinition<INpcGetter> GetDeadCondition = MutagenTopicBuilder.FromDiscussion(
            361,
            "GetDead condition used on unique npc",
            Severity.Warning)
        .WithFormatting<INpcGetter>("GetDead used on unique npc {0} instead of GetDeadCount");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidConditionReference, InvalidStageCondition, GetDeadCondition];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ISkyrimMajorRecordGetter> param)
    {
        var conditions = param.Record.GetConditions();
        if (conditions is null) return;

        foreach (var condition in conditions)
        {
            if (condition.Data is { RunOnType: Condition.RunOnType.Reference, Reference.IsNull: true })
            {
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
            }
            else if (condition.Data is IGetDeadConditionDataGetter
                     && condition.Data.RunOnType == Condition.RunOnType.Reference
                     && condition.Data.Reference.TryResolve(param.LinkCache, out var reference)
                     && reference is IPlacedNpcGetter placedNpc
                     && placedNpc.Base.TryResolve(param.LinkCache, out var npc)
                     && npc.IsUnique())
            {
                param.AddTopic(
                    GetDeadCondition.Format(npc));
            }
            else if (condition is IConditionFloatGetter { Data: IGetStageConditionDataGetter getStage } floatCondition
                     && getStage.Quest.UsesLink() && getStage.Quest.Link.TryResolve(param.LinkCache, out var quest)
                     && quest.Stages.All(s => s.Index != (int)floatCondition.ComparisonValue))
            {
                param.AddTopic(
                    InvalidStageCondition.Format((int)floatCondition.ComparisonValue, condition));
            }
            else if (condition.Data is IGetStageDoneConditionDataGetter getStageDone
                     && getStageDone.Quest.UsesLink() && getStageDone.Quest.Link.TryResolve(param.LinkCache, out var quest2)
                     && quest2.Stages.All(s => s.Index != getStageDone.Stage))
            {
                param.AddTopic(
                    InvalidStageCondition.Format(getStageDone.Stage, condition));
            }
        }
    }

    public IEnumerable<Func<ISkyrimMajorRecordGetter, object?>> FieldsOfInterest()
    {
        yield break;
    }
}

public static class RecordExtension
{
    public static IEnumerable<IConditionGetter>? GetConditions(this ISkyrimMajorRecordGetter record)
    {
        return record switch
        {
            ICameraPathGetter cameraPath => cameraPath.Conditions,
            IConstructibleObjectGetter constructibleObject => constructibleObject.Conditions,
            IDialogResponsesGetter dialogResponses => dialogResponses.Conditions,
            IFactionGetter faction => faction.Conditions,
            IIdleAnimationGetter idleAnimation => idleAnimation.Conditions,
            ILoadScreenGetter loadScreen => loadScreen.Conditions,
            IMagicEffectGetter magicEffect => magicEffect.Conditions,
            IMessageGetter message => message.MenuButtons.SelectMany(x => x.Conditions),
            IMusicTrackGetter musicTrack => musicTrack.Conditions,
            IPackageGetter package => package.Conditions.Concat(package.ProcedureTree.SelectMany(b => b.Conditions)),
            IPerkGetter perk => perk.Conditions,
            IQuestGetter quest => quest.DialogConditions
                .Concat(quest.EventConditions)
                .Concat(quest.Aliases.SelectMany(a => a.Conditions))
                .Concat(quest.Stages.SelectMany(s => s.LogEntries.SelectMany(e => e.Conditions)))
                .Concat(quest.Objectives.SelectMany(o => o.Targets.SelectMany(t => t.Conditions))),
            ISceneGetter scene => scene.Conditions,
            ISoundDescriptorGetter soundDescriptor => soundDescriptor.Conditions,
            IAStoryManagerNodeGetter storyManagerNode => storyManagerNode.Conditions,
            _ => null
        };
    }
}
