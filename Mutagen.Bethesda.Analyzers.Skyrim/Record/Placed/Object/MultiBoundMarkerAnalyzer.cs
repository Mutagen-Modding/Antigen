using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class MultiBoundMarkerAnalyzer : IIsolatedRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition MultiBoundMarker = MutagenTopicBuilder.FromDiscussion(
            288,
            "MultiBound Marker Placement",
            Severity.Warning)
        .WithoutFormatting("Placed Object is a MultiBound Marker which doesn't work in Skyrim");

    public IEnumerable<TopicDefinition> Topics { get; } = [MultiBoundMarker];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        if (placedObject.Base.FormKey == FormKeys.SkyrimSE.Skyrim.Static.MultiBoundMarker.FormKey)
        {
            param.AddTopic(MultiBoundMarker.Format());
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Base;
    }
}
