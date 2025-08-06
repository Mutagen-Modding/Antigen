using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public class NoShortNameAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<string, Language> NoShortNameFaction = MutagenTopicBuilder.FromDiscussion(
            403,
            "Npc has no Short Name",
            Severity.Suggestion)
        .WithFormatting<string, Language>("Npc has name {0} with a space in {1}, but no short name");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoShortNameFaction];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.IsEligibleForTest(param.LinkCache)) return;

        if (npc.ShortName is not null) return;
        if (npc.Name is null) return;

        foreach (var (language, name) in npc.Name) {
            if (name.Contains(' '))
            {
                param.AddTopic(
                    NoShortNameFaction.Format(name, language));
            }
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.ShortName;
        yield return x => x.Name;
    }
}
