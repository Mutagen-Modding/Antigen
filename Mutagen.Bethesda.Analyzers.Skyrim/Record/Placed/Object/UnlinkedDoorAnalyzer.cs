using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class UnlinkedDoorAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition NoDoorLinkedInTeleport = MutagenTopicBuilder.FromDiscussion(
            379,
            "Load Door Linked Without Teleport Destination",
            Severity.Warning)
        .WithoutFormatting("Places load door with teleport enabled but doesn't link to another door");

    public static readonly TopicDefinition<IDoorGetter> NoTeleportSetOnLoadDoor = MutagenTopicBuilder.FromDiscussion(
            380,
            "Load Door Placement Without Teleport",
            Severity.Warning)
        .WithFormatting<IDoorGetter>("Places load door {0} but does not set teleport destination");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoDoorLinkedInTeleport, NoTeleportSetOnLoadDoor];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        // Skip deleted objects
        if (placedObject.IsDeleted) return;

        if (placedObject.TeleportDestination is {} destination)
        {
            if (destination.Door.IsNull)
            {
                param.AddTopic(NoDoorLinkedInTeleport.Format());
            }

            return;
        }

        // Skip inaccessible doors
        if (((PlacedObject.DoorMajorFlag)placedObject.SkyrimMajorRecordFlags).HasFlag(PlacedObject.DoorMajorFlag.Inaccessible)) return;

        // Skip non-doors
        var door = placedObject.Base.TryResolve<IDoorGetter>(param.LinkCache);
        if (door?.EditorID != null && door.EditorID.Contains("load"))
        {
            param.AddTopic(NoTeleportSetOnLoadDoor.Format(door));
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.SkyrimMajorRecordFlags;
        yield return x => x.TeleportDestination;
        yield return x => x.Base;
    }
}
