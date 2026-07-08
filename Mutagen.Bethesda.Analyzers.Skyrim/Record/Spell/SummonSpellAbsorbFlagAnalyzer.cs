using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Spell;

public class SummonSpellAbsorbFlagAnalyzer: IContextualRecordAnalyzer<ISpellGetter>
{
    public static readonly TopicDefinition EmptyEffectList = MutagenTopicBuilder.FromDiscussion(
            604,
            "Summon Spell Without No Absorb Flag",
            Severity.Warning)
        .WithoutFormatting("Spell has summon effect but does not have the No Absorb Or Reflect flag set");

    public IEnumerable<TopicDefinition> Topics { get; } = [EmptyEffectList];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ISpellGetter> param)
    {
        var spell = param.Record;

        if (spell.Effects.Count == 0) return;
        if ((spell.Flags & SpellDataFlag.NoAbsorbOrReflect) != 0) return;

        foreach (var effect in spell.Effects)
        {
            if (effect.BaseEffect.TryResolve(param.LinkCache) is null) continue;
            if (effect.BaseEffect.TryResolve(param.LinkCache)!.Archetype.Type == MagicEffectArchetype.TypeEnum.SummonCreature)
            {
                param.AddTopic(EmptyEffectList.Format());
            }
        }

    }

    public IEnumerable<Func<ISpellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Effects;
    }
}
