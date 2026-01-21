using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Skyrim.Records.Assets.VoiceType;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class LinksToDifferentSpeakerAnalyzer : IContextualRecordAnalyzer<IDialogTopicGetter>
{
    public static readonly TopicDefinition<IDialogTopicGetter> LinksToDifferentSpeaker = MutagenTopicBuilder.FromDiscussion(
            468,
            "Links to Different Speaker",
            Severity.Error)
        .WithFormatting<IDialogTopicGetter>("Topic has response that links to topic {0} that has no speakers in common with the responses linking to it");

    public IEnumerable<TopicDefinition> Topics { get; } = [LinksToDifferentSpeaker];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogTopicGetter> param)
    {
        var dialogTopic = param.Record;
        var voiceTypeAssetLookup = param.ResolveCache<VoiceTypeAssetLookup>();

        var topicsPerLink = dialogTopic.Responses
            .SelectMany(responses => responses.LinkTo.Select(link => (link, responses)))
            .GroupBy(x => x.link)
            .ToDictionary(g => g.Key, g => g.Select(x => x.responses).ToList());

        var speakersPerResponse = dialogTopic.Responses
            .ToDictionary(
                responses => responses,
                responses => voiceTypeAssetLookup.GetSpeakers(responses).ToHashSet());

        foreach (var (linkTopicLink, responses) in topicsPerLink)
        {
            var linkTopic = linkTopicLink.TryResolve(param.LinkCache);
            if (linkTopic?.Responses is null) continue;

            if (linkTopic.Responses
                .All(linkedResponses =>
                {
                    var linkedSpeakers = voiceTypeAssetLookup.GetSpeakers(linkedResponses).ToHashSet();

                    return responses
                        .Select(r => speakersPerResponse.FirstOrDefault(s => s.Key.FormKey == r.FormKey).Value)
                        .WhereNotNull()
                        .All(speakers => !speakers.Intersect(linkedSpeakers).Any());
                }))
            {
                param.AddTopic(
                    LinksToDifferentSpeaker.Format(linkTopic),
                    ("Responses the link comes from", responses));
            }
        }
    }

    public IEnumerable<Func<IDialogTopicGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Responses;
    }
}
