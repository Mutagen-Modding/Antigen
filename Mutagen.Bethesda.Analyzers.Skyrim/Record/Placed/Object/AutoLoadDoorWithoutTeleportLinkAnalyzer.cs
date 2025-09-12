using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class AutoLoadDoorWithoutTeleportLinkAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition AutoLoadDoorWithoutTeleportLink = MutagenTopicBuilder.FromDiscussion(
            454,
            "Auto Load Door without Teleport Link",
            Severity.CTD)
        .WithoutFormatting("Auto Load Door placement does not have a teleport link");

    public IEnumerable<TopicDefinition> Topics { get; } = [AutoLoadDoorWithoutTeleportLink];

    public static readonly HashSet<FormKey> AutoLoadDoors =
    [
        FormKeys.SkyrimSE.Skyrim.Door.AutoLoadDoor01.FormKey,
        FormKeys.SkyrimSE.Skyrim.Door.AutoLoadDoorHiddenMinUse01.FormKey,
        FormKeys.SkyrimSE.Skyrim.Door.AutoLoadDoorMinUse01.FormKey
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        if (AutoLoadDoors.Contains(placedObject.Base.FormKey)
            && (placedObject.TeleportDestination is null || placedObject.TeleportDestination.Door.IsNull))
        {
            param.AddTopic(
                AutoLoadDoorWithoutTeleportLink.Format());
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Base;
        yield return x => x.TeleportDestination;
    }
}
