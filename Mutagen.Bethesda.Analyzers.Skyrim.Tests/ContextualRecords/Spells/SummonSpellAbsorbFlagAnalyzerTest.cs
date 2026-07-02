using Mutagen.Bethesda.Analyzers.Skyrim.Record.Spell;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Spells;

using Fixture = ContextualRecordTestFixture<SummonSpellAbsorbFlagAnalyzer, Spell, ISpellGetter>;

public class SummonSpellAbsorbFlagAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void TestSummonSpellWithoutFlag(Fixture fixture)
    {
        var magEffect = fixture.Create<MagicEffect>();
        magEffect.Archetype = new MagicEffectSummonCreatureArchetype();

        fixture.Run(
            prepForError: (rec, mod) =>
            {
                mod.MagicEffects.Add(magEffect);
                Effect effect = new Effect();
                effect.BaseEffect = magEffect.ToNullableLink();
                rec.Effects.Add(effect);
            },
            prepForFix: (rec, mod) =>
            {
                rec.Flags |= SpellDataFlag.NoAbsorbOrReflect;
            },
            SummonSpellAbsorbFlagAnalyzer.EmptyEffectList
        );
    }

    [Theory, MutagenModAutoData]
    public void TestNonSummonSpellWithoutFlag(Fixture fixture)
    {
        var magEffect = fixture.Create<MagicEffect>();
        magEffect.Archetype = new MagicEffectSummonCreatureArchetype();

        fixture.Run(
            prepForError: (rec, mod) =>
            {
                mod.MagicEffects.Add(magEffect);
                Effect effect = new Effect();
                effect.BaseEffect = magEffect.ToNullableLink();
                rec.Effects.Add(effect);
            },
            prepForFix: (rec, mod) =>
            {
                rec.Effects.Clear();
            },
            SummonSpellAbsorbFlagAnalyzer.EmptyEffectList
        );
    }
}
