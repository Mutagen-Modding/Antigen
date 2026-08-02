using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Caches;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Npc;

public class PersistenceLocationAnalyzer : IContextualRecordAnalyzer<IPlacedNpcGetter>
{
    public static readonly TopicDefinition<ILocationGetter, ICellGetter> PersistenceLocationWithCellWithoutLocation = MutagenTopicBuilder.FromDiscussion(
            385,
            "Placed Npc Persistence Location With Cell Without Location",
            Severity.Error)
        .WithFormatting<ILocationGetter, ICellGetter>("Placed Npc has persistence location {0} but the cell it is placed in {1} has no location defined");

    public static readonly TopicDefinition<ILocationGetter, ICellGetter, ILocationGetter> NotInsidePersistenceLocation = MutagenTopicBuilder.FromDiscussion(
            386,
            "Placed Npc Not Inside Persistence Location",
            Severity.Error)
        .WithFormatting<ILocationGetter, ICellGetter, ILocationGetter>("Placed Npc has persistence location {0} but the cell it is placed in {1} has location {2} which is not in the persistence location");

    public IEnumerable<TopicDefinition> Topics { get; } = [PersistenceLocationWithCellWithoutLocation, NotInsidePersistenceLocation,];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedNpcGetter> param)
    {
        var placedNpc = param.Record;

        if (placedNpc.PersistentLocation.IsNull) return;
        if (placedNpc.PersistentLocation.FormKey == FormKeys.SkyrimSE.Skyrim.Location.PersistAll.FormKey) return;
        if (placedNpc.PersistentLocation.FormKey == FormKeys.SkyrimSE.Skyrim.Location.VirtualLocation.FormKey) return;

        var persistenceLocation = placedNpc.PersistentLocation.TryResolve(param.LinkCache);
        if (persistenceLocation is null) return;

        var cell = placedNpc.GetCell(param.LinkCache, param.ResolveCache<IExteriorCellCache>());
        if (cell is null) return;

        var location = cell.GetLocation(param.LinkCache);
        if (location == null)
        {
            param.AddTopic(
                PersistenceLocationWithCellWithoutLocation.Format(persistenceLocation, cell));
            return;
        }

        if (!cell.GetAllLocations(param.LinkCache).Any(l => l.Equals(persistenceLocation)))
        {
            param.AddTopic(
                NotInsidePersistenceLocation.Format(persistenceLocation, cell, location));
        }
    }

    public IEnumerable<Func<IPlacedNpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.PersistentLocation;
    }
}
