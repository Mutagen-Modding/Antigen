using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class KhajiitNameAnalyzer : IIsolatedRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<string, Language> KhajiitNameHasUppercaseAfterApostrophe = MutagenTopicBuilder.FromDiscussion(
            473,
            "Khajiit Name starts with Uppercase after Apostrophe",
            Severity.Suggestion)
        .WithFormatting<string, Language>("Khajiit name '{0}' in {1} has an uppercase letter after apostrophe");

    public IEnumerable<TopicDefinition> Topics { get; } = [KhajiitNameHasUppercaseAfterApostrophe];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        if (npc.Race.FormKey != FormKeys.SkyrimSE.Skyrim.Race.KhajiitRace.FormKey
            && npc.Race.FormKey != FormKeys.SkyrimSE.Skyrim.Race.KhajiitRaceVampire.FormKey) return;

        if (npc.Name is null) return;

        foreach (var (language, name) in npc.Name)
        {
            if (language is Language.Japanese or Language.Korean or Language.Chinese or Language.ChineseSimplified or Language.Russian) continue;

            var split = name.Split('\'');
            if (split.Length < 2) continue;

            var secondPart = split[1].Trim();
            if (secondPart.Length == 0) continue;

            if (char.IsUpper(secondPart[0]))
            {
                param.AddTopic(
                    KhajiitNameHasUppercaseAfterApostrophe.Format(name, language));
            }
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
        yield return x => x.Race;
    }
}
