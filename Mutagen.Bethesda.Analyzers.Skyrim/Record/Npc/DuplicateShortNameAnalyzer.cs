using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class DuplicateShortNameAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<string?, Language> DuplicateShortName = MutagenTopicBuilder.FromDiscussion(
            245,
            "Duplicate short name",
            Severity.Suggestion)
        .WithFormatting<string?, Language>("Npc short name {0} is the same as the full name in {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [DuplicateShortName];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        if (npc.Name is null || npc.ShortName is null) return;

        foreach (var (language, name) in npc.Name)
        {
            if (!npc.ShortName.TryLookup(language, out var shortName)) continue;
            if (name != shortName) continue;

            param.AddTopic(
                DuplicateShortName.Format(name, language));
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
        yield return x => x.ShortName;
    }
}
