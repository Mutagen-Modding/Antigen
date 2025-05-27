using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Key;

public class MissingFieldsAnalyzer : IIsolatedRecordAnalyzer<IKeyGetter>
{
    public static readonly TopicDefinition NoPickupSound = MutagenTopicBuilder.FromDiscussion(
            227,
            "No Pickup Sound",
            Severity.Suggestion)
        .WithoutFormatting("Key has no pickup sound");

    public static readonly TopicDefinition NoPutDownSound = MutagenTopicBuilder.FromDiscussion(
            309,
            "No Put Down Sound",
            Severity.Suggestion)
        .WithoutFormatting("Key has no put down sound");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoPickupSound, NoPutDownSound];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IKeyGetter> param)
    {
        var key = param.Record;
        if (key.IsDeleted) return;

        if (key.PickUpSound.IsNull)
        {
            param.AddTopic(NoPickupSound.Format());
        }

        if (key.PutDownSound.IsNull)
        {
            param.AddTopic(NoPutDownSound.Format());
        }
    }

    public IEnumerable<Func<IKeyGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.PickUpSound;
        yield return x => x.PutDownSound;
    }
}
