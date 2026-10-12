using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Caches;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Placed;

public class PersistenceLocationAnalyzer : IContextualRecordAnalyzer<IPlacedGetter>
{
    public static readonly TopicDefinition<IFormLinkGetter<ILocationGetter>, ICellGetter> PersistenceLocationWithCellWithoutLocation = MutagenTopicBuilder.FromDiscussion(
            385,
            "Placed Persistence Location With Cell Without Location",
            Severity.Error)
        .WithFormatting<IFormLinkGetter<ILocationGetter>, ICellGetter>("Placed has persistence location {0} but the cell it is placed in {1} has no location defined");

    public static readonly TopicDefinition<IFormLinkGetter<ILocationGetter>, ICellGetter, ILocationGetter> NotInsidePersistenceLocation = MutagenTopicBuilder.FromDiscussion(
            386,
            "Placed Not Inside Persistence Location",
            Severity.Error)
        .WithFormatting<IFormLinkGetter<ILocationGetter>, ICellGetter, ILocationGetter>("Placed has persistence location {0} but the cell it is placed in {1} has location {2} which is not in the persistence location");

    public IEnumerable<TopicDefinition> Topics { get; } = [PersistenceLocationWithCellWithoutLocation, NotInsidePersistenceLocation,];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedGetter> param)
    {
        var placed = param.Record;

        var persitLoc = placed.GetPersistLocation();

        if (persitLoc.IsNull) return;
        if (persitLoc.FormKey == FormKeys.SkyrimSE.Skyrim.Location.PersistAll.FormKey) return;
        if (persitLoc.FormKey == FormKeys.SkyrimSE.Skyrim.Location.VirtualLocation.FormKey) return;

        var cell = placed.GetCell(param.LinkCache, param.ResolveCache<IExteriorCellCache>());
        if (cell is null) return;

        var location = cell.GetLocation(param.LinkCache);
        if (location == null)
        {
            param.AddTopic(
                PersistenceLocationWithCellWithoutLocation.Format(persitLoc, cell));
            return;
        }

        if (!cell.GetAllLocations(param.LinkCache).Any(l => l.Equals(persitLoc)))
        {
            param.AddTopic(
                NotInsidePersistenceLocation.Format(persitLoc, cell, location));
        }
    }

    public IEnumerable<Func<IPlacedGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.GetPersistLocation();
    }
}
