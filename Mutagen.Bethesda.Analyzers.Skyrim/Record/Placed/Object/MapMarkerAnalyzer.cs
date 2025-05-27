using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class MapMarkerAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition NoMenuDisplayObject = MutagenTopicBuilder.FromDiscussion(
            286,
            "Not Persistent",
            Severity.Error)
        .WithoutFormatting("Map marker not persistent");

    public static readonly TopicDefinition NoLocRefType = MutagenTopicBuilder.FromDiscussion(
            346,
            "No Loc Ref Type",
            Severity.Suggestion)
        .WithoutFormatting("Map marker missing location reference type MapMarkerRefType");

    public static readonly TopicDefinition NoEditorID = MutagenTopicBuilder.FromDiscussion(
            347,
            "No EditorID",
            Severity.None)
        .WithoutFormatting("Map marker missing EditorID");

    public static readonly TopicDefinition NoLinkedReference = MutagenTopicBuilder.FromDiscussion(
            348,
            "No Linked Reference",
            Severity.Suggestion)
        .WithoutFormatting("Map marker missing linked reference for player spawn location");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoMenuDisplayObject, NoLocRefType, NoEditorID, NoLinkedReference];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        if (placedObject.IsDeleted) return;
        if (placedObject.Base.FormKey != FormKeys.SkyrimSE.Skyrim.Static.MapMarker.FormKey) return;

        // Not Persistent
        if ((placedObject.SkyrimMajorRecordFlags & (SkyrimMajorRecord.SkyrimMajorRecordFlag)PlacedObject.DefaultMajorFlag.Persistent) == 0)
        {
            param.AddTopic(
                NoMenuDisplayObject.Format());
        }

        // No Loc Ref Type
        if (placedObject.LocationRefTypes is null || placedObject.LocationRefTypes.All(link => link.FormKey != FormKeys.SkyrimSE.Skyrim.LocationReferenceType.MapMarkerRefType.FormKey))
        {
            param.AddTopic(
                NoLocRefType.Format());
        }

        // No EditorID
        if (placedObject.EditorID is null)
        {
            param.AddTopic(
                NoEditorID.Format());
        }

        // No Linked Reference
        if (placedObject.LinkedReferences
            .Select(link => link.Reference.TryResolve<IPlacedObjectGetter>(param.LinkCache))
            .WhereNotNull()
            .All(link => link.Base.FormKey != FormKeys.SkyrimSE.Skyrim.Static.XMarkerHeading.FormKey))
        {
            param.AddTopic(
                NoLinkedReference.Format());
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.EditorID;
        yield return x => x.LinkedReferences;
        yield return x => x.LocationRefTypes;
        yield return x => x.SkyrimMajorRecordFlags;
    }
}
