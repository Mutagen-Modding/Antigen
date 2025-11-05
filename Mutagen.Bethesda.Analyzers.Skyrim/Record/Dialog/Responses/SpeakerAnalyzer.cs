using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Records.Assets.VoiceType;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class SpeakerAnalyzer : IContextualRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition MissingSpeaker = MutagenTopicBuilder.FromDiscussion(
            392,
            "Missing Speaker",
            Severity.Error)
        .WithoutFormatting("Dialog has no possible speaker based on its conditions and its quest's dialogue conditions");

    public static readonly TopicDefinition<IDialogResponsesGetter> DifferentSpeakerInSharedInfo = MutagenTopicBuilder.FromDiscussion(
            467,
            "Different Speaker in Shared Info",
            Severity.Error)
        .WithFormatting<IDialogResponsesGetter>(
            "Dialog uses a shared info {0} that has no speakers in common with itself");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingSpeaker, DifferentSpeakerInSharedInfo];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        var voiceTypeAssetLookup = param.ResolveCache<VoiceTypeAssetLookup>();
        var speakers = voiceTypeAssetLookup.GetSpeakers(dialogResponses).ToHashSet();
        if (speakers.Capacity == 0)
        {
            param.AddTopic(
                MissingSpeaker.Format());
        }

        if (!dialogResponses.ResponseData.IsNull)
        {
            var sharedInfo = dialogResponses.ResponseData.TryResolve(param.LinkCache);
            if (sharedInfo is null) return;

            var sharedInfoSpeakers = voiceTypeAssetLookup.GetSpeakers(sharedInfo).ToHashSet();
            if (!speakers.Intersect(sharedInfoSpeakers).Any())
            {
                param.AddTopic(
                    DifferentSpeakerInSharedInfo.Format(sharedInfo));
            }
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Conditions;
    }
}
