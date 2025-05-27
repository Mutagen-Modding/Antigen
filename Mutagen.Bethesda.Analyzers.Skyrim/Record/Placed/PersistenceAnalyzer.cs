using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed;

public class PersistenceAnalyzer : IContextualRecordAnalyzer<IPlacedGetter>
{
    public static readonly TopicDefinition UnnecessaryPersistence = MutagenTopicBuilder.FromDiscussion(
            250,
            "Unnecessary Persistence",
            Severity.Warning)
        .WithoutFormatting("Placed record is persistent but does not need to be");

    public IEnumerable<TopicDefinition> Topics { get; } = [UnnecessaryPersistence];

    private static readonly HashSet<FormKey> AllowedPersistentObjects =
    [
        FormKeys.SkyrimSE.Skyrim.Static.COCMarkerHeading.FormKey,
        FormKeys.SkyrimSE.Skyrim.Static.PlaneMarker.FormKey,
        FormKeys.SkyrimSE.Skyrim.Static.RoomMarker.FormKey,
        FormKeys.SkyrimSE.Skyrim.Static.PortalMarker.FormKey,
        FormKeys.SkyrimSE.Skyrim.Static.MultiBoundMarker.FormKey,
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedGetter> param)
    {
        var placed = param.Record;

        if (!placed.SkyrimMajorRecordFlags.HasFlag((SkyrimMajorRecord.SkyrimMajorRecordFlag)PlacedObject.DefaultMajorFlag.Persistent)) return;

        // TODO: placed records that is referenced are likely meant to be be persistent
        // if (references.Any()) return result;

        switch (placed)
        {
            case IPlacedObjectGetter placedObjectGetter:
                if (AllowedPersistentObjects.Contains(placedObjectGetter.Base.FormKey)) return;

                if (placedObjectGetter.MapMarker is not null) return;
                if (placedObjectGetter.VirtualMachineAdapter is not null) return;
                if (placedObjectGetter.LinkedReferences.Any()) return;
                if (placedObjectGetter.LocationRefTypes is not null) return;

                // Base types that are allowed to be persistent
                if (param.LinkCache.TryResolve<IDoorGetter>(placedObjectGetter.Base.FormKey, out _)) return;
                if (param.LinkCache.TryResolve<ITextureSetGetter>(placedObjectGetter.Base.FormKey, out _)) return;

                break;
            // case IPlacedNpcGetter placedNpcGetter:
            //     if (placedNpcGetter.VirtualMachineAdapter is not null) return result;
            //     if (placedNpcGetter.LocationRefTypes is not null) return result;
            //
            //     var npc = placedNpcGetter.Base.TryResolve(param.LinkCache);
            //     if (npc is not null) {
            //         Console.WriteLine("Removing actor persistence: " + (npc.Name?.String ?? npc.EditorID ?? npc.FormKey.ToString()));
            //     }
            //
            //     break;
            // case IAPlacedTrapGetter placedTrapGetter:
            //     if (placedTrapGetter.VirtualMachineAdapter is not null) return result;
            //     if (placedTrapGetter.LinkedReferences.Any()) return result;
            //     if (placedTrapGetter.LocationRefTypes is not null) return result;
            //
            //     break;
        }
    }

    public IEnumerable<Func<IPlacedGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.SkyrimMajorRecordFlags;
    }
}
