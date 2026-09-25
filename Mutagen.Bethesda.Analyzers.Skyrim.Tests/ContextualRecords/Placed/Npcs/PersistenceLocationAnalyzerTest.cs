using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Npc;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Placed.Npcs;

using Fixture = ContextualRecordTestFixture<PersistenceLocationAnalyzer, PlacedNpc, IPlacedNpcGetter>;

public class PersistenceLocationAnalyzerTest
{
    Cell Setup(PlacedNpc rec, ISkyrimMod mod)
    {
        var cell = new Cell(mod);
        cell.Flags |= Cell.Flag.IsInteriorCell;
        mod.Cells.AddInteriorCell(cell);
        cell.Temporary.Add(rec);

        return cell;
    }

    // An actor with a persist location must be in a cell with a location
    [Theory, MutagenModAutoData]
    public void CellWithoutLocation(Fixture fixture, Location location)
    {
        Cell? cell = null;
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                cell = Setup(rec, mod);
                rec.PersistentLocation.SetTo(location);
            },
            prepForFix: (rec, mod) =>
            {
                cell!.Location.SetTo(rec.PersistentLocation);
            },
            PersistenceLocationAnalyzer.PersistenceLocationWithCellWithoutLocation);
    }

    // A cell may inherit its location from a worldspace
    [Theory, MutagenModAutoData]
    public void LocationFromWorldspace(Fixture fixture, Worldspace world, Location location)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var cell = new Cell(mod) { Grid = new() { Point = new(0, 0) } };
                world.AddCell(cell);

                cell.Temporary.Add(rec);
                rec.PersistentLocation.SetTo(location);
            },
            prepForFix: (rec, mod) =>
            {
                world.Location.SetTo(rec.PersistentLocation);
            },
            PersistenceLocationAnalyzer.PersistenceLocationWithCellWithoutLocation);
    }

    // An npcs persist location must be the same as its cell or a parent of the cell location
    [Theory, MutagenModAutoData]
    public void NotInLocation(Fixture fixture, Location cellLoc, Location persistLoc)
    {
        Cell? cell = null;
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                cell = Setup(rec, mod);

                cell.Location.SetTo(cellLoc);
                rec.PersistentLocation.SetTo(persistLoc);
            },
            prepForFix: (rec, mod) =>
            {
                cellLoc.ParentLocation.SetTo(persistLoc);
            },
            PersistenceLocationAnalyzer.NotInsidePersistenceLocation);
    }
}
