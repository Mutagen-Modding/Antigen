using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.LeveledSpell;

public class SkillLevelAnalyzer : IContextualRecordAnalyzer<ILeveledSpellGetter>
{
    public static readonly TopicDefinition<ISpellGetter, int, IMagicEffectGetter, uint> SkillLevelTooLow = MutagenTopicBuilder.FromDiscussion(
            365,
            "Skill Level Too Low",
            Severity.Error)
        .WithFormatting<ISpellGetter, int, IMagicEffectGetter, uint>("Spell entry {0} is available at skill level {1} which is lower than the spell's magic effect {2} skill level requirement {3}");

    public IEnumerable<TopicDefinition> Topics { get; } = [SkillLevelTooLow];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ILeveledSpellGetter> param)
    {
        var leveledSpell = param.Record;
        if (leveledSpell.Entries is null) return;

        // Ignore use all leveled spells, as you can have multiple spells there and if one spell will not be available at that skill level it should be fine
        if (leveledSpell.Flags.HasFlag(Bethesda.Skyrim.LeveledSpell.Flag.UseAllSpells)) return;

        foreach (var entry in leveledSpell.Entries)
        {
            var entryData = entry.Data;
            if (entryData is null) continue;

            if (entryData.Reference.TryResolve(param.LinkCache, out var spellRef))
            {
                switch (spellRef)
                {
                    case ILeveledSpellGetter:
                    {
                        // Don't check nested leveled spells - they will be checked directly
                        break;
                    }
                    case IShoutGetter shout:
                    {
                        foreach (var word in shout.WordsOfPower)
                        {
                            if (word.Spell.TryResolve(param.LinkCache, out var spell))
                            {
                                CheckSpell(spell);
                            }
                        }
                        break;
                    }
                    case ISpellGetter spell:
                    {
                        CheckSpell(spell);
                        break;
                    }

                }
            }

            void CheckSpell(ISpellGetter s)
            {
                foreach (var effect in s.Effects)
                {
                    var magicEffect = effect.BaseEffect.TryResolve(param.LinkCache);
                    if (magicEffect is null) continue;

                    var skillLevel = magicEffect.MinimumSkillLevel;
                    if (skillLevel > entryData.Level)
                    {
                        param.AddTopic(
                            SkillLevelTooLow.Format(
                                s,
                                entryData.Level,
                                magicEffect,
                                skillLevel));
                    }
                }
            }
        }
    }

    public IEnumerable<Func<ILeveledSpellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Entries;
    }
}
