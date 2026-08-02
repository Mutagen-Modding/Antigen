using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class OcclusionMarkerAnalyzer : IIsolatedRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition NoPrimitive = MutagenTopicBuilder.FromDiscussion(
            389,
            "Occlusion Marker Without Primitive",
            Severity.Warning)
        .WithoutFormatting("Occlusion marker placement has no primitive data");

    public static readonly TopicDefinition OcclusionBoxShouldBePlaneMarker = MutagenTopicBuilder.FromDiscussion(
            390,
            "Occlusion Box Should Be Occlusion Plane",
            Severity.Warning)
        .WithoutFormatting("Occlusion Box placement has half-extent smaller than 32, so it should be a Occlusion Plane instead");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoPrimitive, OcclusionBoxShouldBePlaneMarker];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        if (placedObject.Base.FormKey != FormKeys.SkyrimSE.Skyrim.Static.PlaneMarker.FormKey) return;

        if (placedObject.Primitive is not {} primitive)
        {
            param.AddTopic(
                NoPrimitive.Format());
            return;
        }

        if (primitive.Type == PlacedPrimitive.TypeEnum.Box && (primitive.Bounds.X < 32 || primitive.Bounds.Y < 32 || primitive.Bounds.Z < 32))
        {
            param.AddTopic(
                OcclusionBoxShouldBePlaneMarker.Format());
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Base;
        yield return x => x.Primitive?.Bounds;
    }
}
