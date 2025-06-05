using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Assets;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Records.Assets.VoiceType;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class MissingSpeakerAnalyzer : IContextualRecordAnalyzer<IDialogResponsesGetter>
{
    private static readonly Lock Lock = new();
    private static VoiceTypeAssetLookup? _voiceTypeAssetLookup;

    public static readonly TopicDefinition MissingSpeaker = MutagenTopicBuilder.FromDiscussion(
            392,
            "Missing Speaker",
            Severity.Error)
        .WithoutFormatting("Dialog has no possible speaker based on its conditions and its quest's dialogue conditions");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingSpeaker];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        // Skip shared infos
        if (dialogResponses.ResponseData.IsNull == false) return;

        // TODO: Replace with something that is updated when the link cache is updated
        lock (Lock)
        {
            if (_voiceTypeAssetLookup is null)
            {
                var immutableAssetLinkCache = param.LinkCache.CreateImmutableAssetLinkCache();

                _voiceTypeAssetLookup = new VoiceTypeAssetLookup();
                _voiceTypeAssetLookup.Prep(immutableAssetLinkCache);
            }
        }

        if (!_voiceTypeAssetLookup.GetSpeakers(dialogResponses).Any())
        {
            param.AddTopic(
                MissingSpeaker.Format());
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Conditions;
    }
}
