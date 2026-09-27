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
            Severity.Suggestion)
        .WithFormatting<IDialogResponsesGetter>(
            "Dialog has speakers not in common with its shared info {0}");

    public static readonly TopicDefinition<IDialogResponsesGetter> DifferentVoiceInSharedInfo = MutagenTopicBuilder.FromDiscussion(
            656,
            "Different Voices in Shared Info",
            Severity.Error)
        .WithFormatting<IDialogResponsesGetter>(
            "Dialog has voices not in common with its shared info {0}");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingSpeaker, DifferentSpeakerInSharedInfo, DifferentVoiceInSharedInfo];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        var voiceTypeAssetLookup = param.ResolveCache<VoiceTypeAssetLookup>();
        // TODO: Would it be faster to only produce a HashSet if needed for IsSubsetOf? Wait until after lookup is optimised to test.
        var speakers = voiceTypeAssetLookup.GetSpeakers(dialogResponses).ToHashSet();
        if (speakers.Count == 0)
        {
            param.AddTopic(
                MissingSpeaker.Format());
        }

        if (!dialogResponses.ResponseData.IsNull)
        {
            var sharedInfo = dialogResponses.ResponseData.TryResolve(param.LinkCache);
            if (sharedInfo is null) return;

            var sharedInfoSpeakers = voiceTypeAssetLookup.GetSpeakers(sharedInfo);
            if (!speakers.IsSubsetOf(sharedInfoSpeakers))
            {
                param.AddTopic(
                    DifferentSpeakerInSharedInfo.Format(sharedInfo), ("Missing", speakers.Except(sharedInfoSpeakers)));
            }

            // TODO: Would be nice to have GetSpeakerData that can give both speakers and voices at once
            var voices = voiceTypeAssetLookup.GetVoiceTypes(dialogResponses).ToHashSet();
            var sharedInfoVoices = voiceTypeAssetLookup.GetVoiceTypes(sharedInfo);
            if (!voices.IsSubsetOf(sharedInfoVoices))
            {
                param.AddTopic(
                    DifferentVoiceInSharedInfo.Format(sharedInfo), ("Missing", voices.Except(sharedInfoVoices)));
            }
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Conditions;
        yield return x => x.ResponseData;
    }
}
