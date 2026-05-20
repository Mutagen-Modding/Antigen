using Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior.Settlement;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Cells.Interior.Settlement;

using Fixture = ContextualRecordTestFixture<NoMerchantChestLocRefTypeAnalyzer, Cell, ICellGetter>;

public class NoMerchantChestLocRefTypeAnalyzerTest
{
    // Configure a cell as a settlement interior and add a container
    PlacedObject Setup(Fixture fixture, ISkyrimMod mod, Cell cell)
    {
        var location = fixture.Create<Location>();
        mod.Locations.Add(location);
        location.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeHouse];
        cell.Location.SetTo(location);

        cell.Flags |= Cell.Flag.IsInteriorCell;
        mod.Cells.AddInteriorCell(cell);

        var placed = fixture.Create<PlacedObject>();
        cell.Persistent.Add(placed);
        return placed;
    }

    // Add a vendor faction that uses the provided object as its merchant container
    void AddMerchantFaction(Fixture fixture, ISkyrimMod mod, PlacedObject chest)
    {
        var faction = fixture.Create<Faction>();
        mod.Factions.Add(faction);
        faction.Flags |= Faction.FactionFlag.Vendor;
        faction.MerchantContainer.SetTo(chest);
    }

    [Theory, MutagenModAutoData]
    public void NoRefType(Fixture fixture)
    {
        PlacedObject? chest = null;
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                chest = Setup(fixture, mod, rec);
                AddMerchantFaction(fixture, mod, chest);
            },
            prepForFix: (rec, mod) =>
            {
                chest!.LocationRefTypes = [FormKeys.SkyrimSE.Skyrim.LocationReferenceType.MerchantContainerRefType];
            },
            NoMerchantChestLocRefTypeAnalyzer.NoMerchantChestLocRefType);
    }

    [Theory, MutagenModAutoData]
    public void NotMerchantChest(Fixture fixture)
    {
        PlacedObject? chest = null;
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                chest = Setup(fixture, mod, rec);
                chest.LocationRefTypes = [FormKeys.SkyrimSE.Skyrim.LocationReferenceType.MerchantContainerRefType];
            },
            prepForFix: (rec, mod) =>
            {
                AddMerchantFaction(fixture, mod, chest!);
            },
            NoMerchantChestLocRefTypeAnalyzer.InvalidMerchantChestLocRefType);
    }
}
