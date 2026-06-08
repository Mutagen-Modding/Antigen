using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Extensions;

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
            IObjectEffectGetter objectEffect => objectEffect.Effects.SelectMany(effect => effect.Conditions),
            IPackageGetter package => package.Conditions.Concat(package.ProcedureTree.SelectMany(b => b.Conditions)),
            IPerkGetter perk => perk.Conditions
                .Concat(perk.Effects.SelectMany(effect => effect.Conditions.SelectMany(tab => tab.Conditions))),
            IQuestGetter quest => quest.DialogConditions
                .Concat(quest.EventConditions)
                .Concat(quest.Aliases.SelectMany(a => a.Conditions))
                .Concat(quest.Stages.SelectMany(s => s.LogEntries.SelectMany(e => e.Conditions)))
                .Concat(quest.Objectives.SelectMany(o => o.Targets.SelectMany(t => t.Conditions))),
            ISceneGetter scene => scene.Conditions
                .Concat(scene.Phases.SelectMany(phase => phase.StartConditions))
                .Concat(scene.Phases.SelectMany(phase => phase.CompletionConditions)),
            IScrollGetter scroll => scroll.Effects.SelectMany(effect => effect.Conditions),
            ISoundDescriptorGetter soundDescriptor => soundDescriptor.Conditions,
            ISpellGetter spell => spell.Effects.SelectMany(effect => effect.Conditions),
            IAStoryManagerNodeGetter storyManagerNode => storyManagerNode.Conditions,
            _ => null
        };
    }
}
