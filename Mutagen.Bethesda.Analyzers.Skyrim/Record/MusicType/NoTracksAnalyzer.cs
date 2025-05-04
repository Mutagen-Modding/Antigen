using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.MusicType;

public class NoTracksAnalyzer : IIsolatedRecordAnalyzer<IMusicTypeGetter>
{
    public static readonly TopicDefinition NoTracks = MutagenTopicBuilder.FromDiscussion(
            185,
            "No tracks",
            Severity.CTD)
        .WithoutFormatting("MusicType has no tracks");

    IEnumerable<TopicDefinition> IAnalyzer.Topics { get; } = [NoTracks];

    void IIsolatedRecordAnalyzer<IMusicTypeGetter>.AnalyzeRecord(IsolatedRecordAnalyzerParams<IMusicTypeGetter> param)
    {
        var record = param.Record;

        var tracks = record.Tracks;

        if (tracks is null || tracks.Count == 0)
        {
            param.AddTopic(NoTracks.Format());
        }
    }

    IEnumerable<Func<IMusicTypeGetter, object?>> IIsolatedRecordAnalyzer<IMusicTypeGetter>.FieldsOfInterest()
    {
        yield return x => x.Tracks;
    }
}

