using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public class NoVoiceTypeAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition NoVoiceType = MutagenTopicBuilder.FromDiscussion(
            284,
            "No Voice Type",
            Severity.Error)
        .WithoutFormatting("Npc has no voice type");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoVoiceType];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.IsEligibleForTest()) return;

        // Skip NPCs using templates for voice types
        if (npc.Configuration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.Traits)) return;

        if (npc.Voice.IsNull)
        {
            param.AddTopic(
                NoVoiceType.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Configuration.Flags;
        yield return x => x.Configuration.TemplateFlags;
        yield return x => x.Keywords;
        yield return x => x.Voice;
    }
}
