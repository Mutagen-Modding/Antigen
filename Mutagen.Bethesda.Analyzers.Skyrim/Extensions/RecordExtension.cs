using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

public static class RecordExtension
{
    /// <summary>
    /// Get conditions attached to the record, split by field
    /// </summary>
    /// <param name="record"></param>
    /// <returns></returns>
    public static IEnumerable<IEnumerable<IConditionGetter>> GetConditionsByField(this ISkyrimMajorRecordGetter record)
    {
        return record switch
        {
            ICameraPathGetter cameraPath => [cameraPath.Conditions],
            IConstructibleObjectGetter constructibleObject => [constructibleObject.Conditions],
            IDialogResponsesGetter dialogResponses => [dialogResponses.Conditions],
            IFactionGetter faction => [faction.Conditions ?? []],
            IIdleAnimationGetter idleAnimation => [idleAnimation.Conditions],
            ILoadScreenGetter loadScreen => [loadScreen.Conditions],
            IMagicEffectGetter magicEffect => [magicEffect.Conditions],
            IMessageGetter message => message.MenuButtons.Select(x => x.Conditions),
            IMusicTrackGetter musicTrack => [musicTrack.Conditions ?? []],
            IObjectEffectGetter objectEffect => objectEffect.Effects.Select(effect => effect.Conditions),
            IPackageGetter package => [package.Conditions, ..package.ProcedureTree.Select(b => b.Conditions)],
            IPerkGetter perk => [perk.Conditions,
                ..perk.Effects.SelectMany(effect => effect.Conditions.Select(tab => tab.Conditions))],
            IQuestGetter quest => [quest.DialogConditions,
                quest.EventConditions,
                ..quest.Aliases.Select(a => a.Conditions),
                ..quest.Stages.SelectMany(s => s.LogEntries.Select(e => e.Conditions)),
                ..quest.Objectives.SelectMany(o => o.Targets.Select(t => t.Conditions))],
            ISceneGetter scene => [scene.Conditions,
                ..scene.Phases.Select(phase => phase.StartConditions),
                ..scene.Phases.Select(phase => phase.CompletionConditions)],
            IScrollGetter scroll => scroll.Effects.Select(effect => effect.Conditions),
            ISoundDescriptorGetter soundDescriptor => [soundDescriptor.Conditions],
            ISpellGetter spell => spell.Effects.Select(effect => effect.Conditions),
            IAStoryManagerNodeGetter storyManagerNode => [storyManagerNode.Conditions],
            _ => []
        };
    }

    public static IQuestGetter? GetOwningQuest(this ISkyrimMajorRecordGetter record, ILinkCache linkCache)
    {
        switch (record)
        {
            case IDialogResponsesGetter dialogResponses:
                linkCache.TryResolveSimpleContext(dialogResponses, out var context);
                return (context?.Parent?.Record as IDialogTopicGetter)?.GetOwningQuest(linkCache);
            case IDialogTopicGetter topic: return topic.Quest.TryResolve(linkCache);
            case IDialogBranch branch: return branch.Quest.TryResolve(linkCache);
            case IMessageGetter message: return message.Quest.TryResolve(linkCache);
            case IPackageGetter package: return package.OwnerQuest.TryResolve(linkCache);
            case IQuestGetter quest: return quest;
            case ISceneGetter scene: return scene.Quest.TryResolve(linkCache);
            default: return null;
        };
    }
}
