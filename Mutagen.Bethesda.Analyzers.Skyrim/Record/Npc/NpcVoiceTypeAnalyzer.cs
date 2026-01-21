using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class NpcVoiceTypeAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<MaleFemaleGender, IVoiceTypeGetter, MaleFemaleGender> NpcVoiceTypeMaleFemaleMismatch = MutagenTopicBuilder.FromDiscussion(
            511,
        "Npc and assigned Voice Type have mismatching gender",
            Severity.Warning)
        .WithFormatting<MaleFemaleGender, IVoiceTypeGetter, MaleFemaleGender>("Npc is {0} but assigned Voice Type is {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [NpcVoiceTypeMaleFemaleMismatch];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        if (npc.Voice.IsNull) return;

        var voiceType = npc.Voice.TryResolve(param.LinkCache);
        if (voiceType == null) return;

        var voiceTypeMaleFemale = voiceType.Flags.HasFlag(VoiceType.Flag.Female) ? MaleFemaleGender.Female : MaleFemaleGender.Male;
        var npcMaleFemale = npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.Female) ? MaleFemaleGender.Female : MaleFemaleGender.Male;
        if (voiceTypeMaleFemale != npcMaleFemale)
        {
            param.AddTopic(
                NpcVoiceTypeMaleFemaleMismatch.Format(npcMaleFemale, voiceType, voiceTypeMaleFemale));
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Voice;
        yield return x => x.Configuration.Flags;
    }
}
