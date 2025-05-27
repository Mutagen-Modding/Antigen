using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Spell;

public class MissingFieldsAnalyzer : IIsolatedRecordAnalyzer<ISpellGetter>
{
    public static readonly TopicDefinition EmptyEffectList = MutagenTopicBuilder.FromDiscussion(
            257,
            "Empty Effect List",
            Severity.Suggestion)
        .WithoutFormatting("Spell has no effect");

    public IEnumerable<TopicDefinition> Topics { get; } = [EmptyEffectList];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ISpellGetter> param)
    {
        var spell = param.Record;

        if (spell.Effects.Count == 0)
        {
            param.AddTopic(EmptyEffectList.Format());
        }
    }

    public IEnumerable<Func<ISpellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Effects;
    }
}
