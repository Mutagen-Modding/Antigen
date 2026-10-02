using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.MusicType;

public class ZeroDecibelDuckingAnalyzer : IIsolatedRecordAnalyzer<IMusicTypeGetter>
{
    public static readonly TopicDefinition ZeroDecibelDucking = MutagenTopicBuilder.FromDiscussion(
            402,
            "Zero Decibel Ducking",
            Severity.Warning)
        .WithoutFormatting("Music type has DucksCurrentTrack flag set, but the ducking decibel level is set to 0");

    public IEnumerable<TopicDefinition> Topics { get; } = [ZeroDecibelDucking];

    void IIsolatedRecordAnalyzer<IMusicTypeGetter>.AnalyzeRecord(IsolatedRecordAnalyzerParams<IMusicTypeGetter> param)
    {
        var record = param.Record;

        if (record.Flags.HasFlag(Mutagen.Bethesda.Skyrim.MusicType.Flag.DucksCurrentTrack))
        {
            if (record.Data is { DuckingDecibel: 0 })
            {
                param.AddTopic(ZeroDecibelDucking.Format());
            }
        }
    }

    IEnumerable<Func<IMusicTypeGetter, object?>> IIsolatedRecordAnalyzer<IMusicTypeGetter>.FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.Data?.DuckingDecibel;
    }
}
