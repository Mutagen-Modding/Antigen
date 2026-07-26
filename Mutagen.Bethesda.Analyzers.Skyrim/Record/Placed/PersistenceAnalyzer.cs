using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed;

using Statics = FormKeys.SkyrimSE.Skyrim.Static;

public class PersistenceAnalyzer : IContextualRecordAnalyzer<IPlacedGetter>
{
    public static readonly TopicDefinition UnnecessaryPersistence = MutagenTopicBuilder.FromDiscussion(
            250,
            "Unnecessary Persistence",
            Severity.Warning)
        .WithoutFormatting("Placed record is persistent but does not need to be");

    public static readonly TopicDefinition NotPersistent = MutagenTopicBuilder.FromDiscussion(
            286,
            "Not Persistent",
            Severity.Error)
        .WithoutFormatting("Placed record is not persistent but needs to be");

    public IEnumerable<TopicDefinition> Topics { get; } = [UnnecessaryPersistence, NotPersistent];

    // Base objects that are always set as persistent by the CK
    private static readonly HashSet<FormKey> AlwaysPersistentBases = [
        Statics.DragonMarker.FormKey,
        Statics.DragonMarkerCrashStrip.FormKey,
        Statics.MapMarker.FormKey,
        Statics.RoomMarker.FormKey,
        Statics.XMarker.FormKey,
        Statics.XMarkerHeading.FormKey,
    ];

    public bool RequiresPersistent(IPlacedGetter placed, ILinkCache linkCache, ILinkUsageCache usageCache)
    {
        // A record should be persistent if it is referenced by another record
        if (usageCache.GetUsagesOf(placed).UsageLinks
            // Exception: Locations list their ref types and persistent NPCs but don't require them to be persistent. UNLESS it is a marker
            .Where(u => !u.TryResolve<ILocationGetter>(linkCache, out var location) || location.HorseMarkerRef.Equals(placed) || location.WorldLocationMarkerRef.Equals(placed))
            // TODO: Usage cache link.Type always returns ILandscapeTextureGetter. Bug in Mutagen? Comparing link.Type would be faster than resolve
            // Exception: Worldspaces list their large refs but don't require them to be persistent
            .Where(u => !u.TryResolve<IWorldspaceGetter>(linkCache, out var _))
            .Any())
        {
            return true;
        }

        if (placed.GetPersistLocation().Equals(FormKeys.SkyrimSE.Skyrim.Location.PersistAll))
            return true;

        if (placed is IPlacedObjectGetter placedObject)
        {
            // Full LOD references need to be persistent. Lights are never full LOD and reuse the flag for never fades
            if (placedObject.SkyrimMajorRecordFlags.HasFlag((SkyrimMajorRecord.SkyrimMajorRecordFlag)PlacedObject.DefaultMajorFlag.IsFullLod))
                if (!placedObject.Base.TryResolve<ILightGetter>(linkCache, out var _))
                    return true;

            // The CK sets certain base objects as persistent
            if (AlwaysPersistentBases.Contains(placedObject.Base.FormKey))
                return true;

            // The CK sets water activators as persistent
            if (placedObject.Base.TryResolve<IActivatorGetter>(linkCache, out var activator) && !activator.WaterType.IsNull)
                return true;

            // The CK sets decals as persistent
            if (placedObject.Base.TryResolve<ITextureSetGetter>(linkCache, out var _))
                return true;
        }


        return false;
    }

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedGetter> param)
    {
        var placed = param.Record;

        var persistent = placed.SkyrimMajorRecordFlags.HasFlag((SkyrimMajorRecord.SkyrimMajorRecordFlag)PlacedObject.DefaultMajorFlag.Persistent);
        var expected = RequiresPersistent(placed, param.LinkCache, param.ResolveCache<ILinkUsageCache>());

        if (persistent && !expected)
        {
            param.AddTopic(UnnecessaryPersistence.Format());
        }
        else if (!persistent && expected)
        {
            param.AddTopic(NotPersistent.Format());
        }
    }

    public IEnumerable<Func<IPlacedGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.SkyrimMajorRecordFlags;
    }
}
