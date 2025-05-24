using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record;

public class ConditionAnalyzer : IIsolatedRecordAnalyzer<ISkyrimMajorRecordGetter>
{
    private static readonly TopicDefinition<string?> InvalidConditionReference = MutagenTopicBuilder.FromDiscussion(
            213,
            "Condition Runs on Null Reference",
            Severity.Error)
        .WithFormatting<string?>("Condition {0} runs on reference, but reference is null");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidConditionReference];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ISkyrimMajorRecordGetter> param)
    {
        var conditions = param.Record.GetConditions();
        if (conditions is null) return;

        foreach (var condition in conditions)
        {
            if (condition.Data.RunOnType != Condition.RunOnType.Reference) continue;
            if (!condition.Data.Reference.IsNull) continue;

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
            IPackageGetter package => package.Conditions,
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
