using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior;

public class MissingFieldsAnalyzer : IIsolatedRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition NoMusic = MutagenTopicBuilder.FromDiscussion(
            261,
            "No Music",
            Severity.Suggestion)
        .WithoutFormatting("Interior cell has no music");

    public static readonly TopicDefinition NoLightingTemplate = MutagenTopicBuilder.FromDiscussion(
            332,
            "No Lighting Template",
            Severity.Suggestion)
        .WithoutFormatting("Interior cell has no lighting template");

    public static readonly TopicDefinition NoAcousticSpace = MutagenTopicBuilder.FromDiscussion(
            333,
            "No Acoustic Space",
            Severity.Suggestion)
        .WithoutFormatting("Interior cell has no acoustic space");

    public static readonly TopicDefinition NoLocation = MutagenTopicBuilder.FromDiscussion(
            334,
            "No Location",
            Severity.Suggestion)
        .WithoutFormatting("Interior cell has no location");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoMusic, NoLightingTemplate, NoAcousticSpace, NoLocation];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;
        if (cell.IsExteriorCell()) return;

        if (cell.Music.IsNull)
        {
            param.AddTopic(
                NoMusic.Format());
        }

        if (cell.LightingTemplate.IsNull)
        {
            param.AddTopic(
                NoLightingTemplate.Format());
        }

        if (cell.AcousticSpace.IsNull)
        {
            param.AddTopic(
                NoAcousticSpace.Format());
        }

        if (cell.Location.IsNull)
        {
            param.AddTopic(
                NoLocation.Format());
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Music;
        yield return x => x.LightingTemplate;
        yield return x => x.AcousticSpace;
        yield return x => x.Location;
    }
}
