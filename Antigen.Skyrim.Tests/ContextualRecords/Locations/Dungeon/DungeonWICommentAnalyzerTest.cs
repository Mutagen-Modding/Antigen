using Antigen.Skyrim.Record.Location.Dungeon;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;
using Activator = Mutagen.Bethesda.Skyrim.Activator;

namespace Antigen.Skyrim.Tests.ContextualRecords.Locations.Dungeon;

using Fixture = ContextualRecordTestFixture<DungeonWICommentAnalyzer, Location, ILocationGetter>;

public class DungeonWICommentAnalyzerTest
{
    const string WICommentTriggerScript = "WICommentTriggerScript";

    // Configure a location as a dungeon and add an interior cell belonging to it.
    // The location is already part of the mod, as injected by the test fixture.
    Cell SetupDungeon(Fixture fixture, ISkyrimMod mod, Location location)
    {
        location.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeDungeon];

        var cell = fixture.Create<Cell>();
        cell.Flags |= Cell.Flag.IsInteriorCell;
        cell.Location.SetTo(location);
        mod.Cells.AddInteriorCell(cell);
        return cell;
    }

    // Add a trigger box with the WI comment trigger script to the cell
    void AddTrigger(Fixture fixture, Cell cell)
    {
        var placed = fixture.Create<PlacedObject>();
        placed.Primitive = new PlacedPrimitive { Type = PlacedPrimitive.TypeEnum.Box };
        placed.VirtualMachineAdapter = new VirtualMachineAdapter
        {
            Scripts = [new ScriptEntry { Name = WICommentTriggerScript }]
        };
        cell.Temporary.Add(placed);
    }

    // A dungeon without a WI comment trigger should be reported
    [Theory, MutagenModAutoData]
    public void NoTrigger(Fixture fixture)
    {
        Cell? cell = null;
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                cell = SetupDungeon(fixture, mod, rec);
            },
            prepForFix: (rec, mod) =>
            {
                AddTrigger(fixture, cell!);
            },
            DungeonWICommentAnalyzer.HasNoWICommentTrigger);
    }

    // A trigger box with the script on the placed object satisfies the analyzer
    [Theory, MutagenModAutoData]
    public void TriggerOnPlacedObject(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            var cell = SetupDungeon(fixture, mod, rec);
            AddTrigger(fixture, cell);
        });
    }

    // A trigger box whose base activator has the script satisfies the analyzer
    [Theory, MutagenModAutoData]
    public void TriggerOnBaseActivator(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            var cell = SetupDungeon(fixture, mod, rec);

            var activator = fixture.Create<Activator>();
            activator.VirtualMachineAdapter = new VirtualMachineAdapter
            {
                Scripts = [new ScriptEntry { Name = WICommentTriggerScript }]
            };
            mod.Activators.Add(activator);

            var placed = fixture.Create<PlacedObject>();
            placed.Primitive = new PlacedPrimitive { Type = PlacedPrimitive.TypeEnum.Box };
            placed.Base.SetTo(activator);
            cell.Temporary.Add(placed);
        });
    }

    // Non-dungeon locations are skipped
    [Theory, MutagenModAutoData]
    public void NotDungeon(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            rec.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeHouse];

            var cell = fixture.Create<Cell>();
            cell.Flags |= Cell.Flag.IsInteriorCell;
            cell.Location.SetTo(rec);
            mod.Cells.AddInteriorCell(cell);
        });
    }
}
