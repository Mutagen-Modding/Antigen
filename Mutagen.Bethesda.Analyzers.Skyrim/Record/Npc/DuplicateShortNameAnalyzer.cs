using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class DuplicateShortNameAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<string?> DuplicateShortName = MutagenTopicBuilder.FromDiscussion(
            245,
            "Duplicate short name",
            Severity.Suggestion)
        .WithFormatting<string?>("Npc short name {0} is the same as the full name");

    public IEnumerable<TopicDefinition> Topics { get; } = [DuplicateShortName];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        if (npc.Name is not null && npc.ShortName is not null && npc.Name.String == npc.ShortName.String)
        {
            param.AddTopic(
                DuplicateShortName.Format(npc.Name.String));
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
        yield return x => x.ShortName;
    }
}
