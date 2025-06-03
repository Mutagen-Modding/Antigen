using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Npc;

public class PersistenceLocationAnalyzer : IContextualRecordAnalyzer<IPlacedNpcGetter>
{
    public static readonly TopicDefinition<ILocationGetter, ICellGetter> PersistenceLocationWithCellWithoutLocation = MutagenTopicBuilder.FromDiscussion(
            385,
            "Placed Object Persistence Location With Cell Without Location",
            Severity.Error)
        .WithFormatting<ILocationGetter, ICellGetter>("Placed Object has persistence location {0} but the cell it is placed in {1} has no location defined");

    public static readonly TopicDefinition<ILocationGetter, ICellGetter, ILocationGetter> NotInsidePersistenceLocation = MutagenTopicBuilder.FromDiscussion(
            386,
            "Placed Object Not Inside Persistence Location",
            Severity.Error)
        .WithFormatting<ILocationGetter, ICellGetter, ILocationGetter>("Placed Object has persistence location {0} but the cell it is placed in {1} has location {2} which is not in the persistence location");

    public IEnumerable<TopicDefinition> Topics { get; } = [PersistenceLocationWithCellWithoutLocation, NotInsidePersistenceLocation,];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedNpcGetter> param)
    {
        var placedObject = param.Record;
        if (placedObject.IsDeleted) return;

        if (placedObject.PersistentLocation.IsNull) return;
        if (placedObject.PersistentLocation.FormKey == FormKeys.SkyrimSE.Skyrim.Location.PersistAll.FormKey) return;
        if (placedObject.PersistentLocation.FormKey == FormKeys.SkyrimSE.Skyrim.Location.VirtualLocation.FormKey) return;

        var persistenceLocation = placedObject.PersistentLocation.TryResolve(param.LinkCache);
        if (persistenceLocation is null) return;

        var cell = placedObject.GetCell(param.LinkCache);
        if (cell is null) return;

        var location = cell.Location.TryResolve(param.LinkCache);
        if (location is null)
        {
            param.AddTopic(
                PersistenceLocationWithCellWithoutLocation.Format(persistenceLocation, cell));
            return;
        }

        if (location
            .GetParentLocations(param.LinkCache, true)
            .All(parentLocation => parentLocation.FormKey != persistenceLocation.FormKey))
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
