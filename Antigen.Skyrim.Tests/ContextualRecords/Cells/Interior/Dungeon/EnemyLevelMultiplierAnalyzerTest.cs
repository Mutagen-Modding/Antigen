using Antigen.Skyrim.Record.Cell.Interior.Dungeon;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Antigen.Skyrim.Tests.ContextualRecords.Cells.Interior.Dungeon;

using Fixture = ContextualRecordTestFixture<EnemyLevelMultiplierAnalyzer, Cell, ICellGetter>;

public class EnemyLevelMultiplierAnalyzerTest
{
    // Configure a cell as a dungeon interior and add the given placed npcs
    void Setup(ISkyrimMod mod, Cell cell, params PlacedNpc[] npcs)
    {
        var location = new Location(mod);
        mod.Locations.Add(location);
        location.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeDungeon];
        cell.Location.SetTo(location);

        cell.Flags |= Cell.Flag.IsInteriorCell;
        mod.Cells.AddInteriorCell(cell);

        foreach (var npc in npcs)
        {
            cell.Temporary.Add(npc);
        }
    }

    // A dungeon with fewer than 30% leveled npcs should be reported
    [Theory, MutagenModAutoData]
    public void TooFewLeveledEnemies(Fixture fixture)
    {
        PlacedNpc? npc2 = null;
        PlacedNpc? npc3 = null;

        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var npc1 = new PlacedNpc(mod) { LevelModifier = Level.Easy };
                npc2 = new PlacedNpc(mod);
                npc3 = new PlacedNpc(mod);
                Setup(mod, rec, npc1, npc2, npc3, new PlacedNpc(mod));
            },
            prepForFix: (rec, mod) =>
            {
                npc2!.LevelModifier = Level.Medium;
                npc3!.LevelModifier = Level.Hard;
            },
            EnemyLevelMultiplierAnalyzer.HasTooFewLeveledEnemies);
    }

    // A dungeon with at least 30% leveled npcs should not be reported
    [Theory, MutagenModAutoData]
    public void EnoughLeveledEnemies(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            var npc1 = new PlacedNpc(mod) { LevelModifier = Level.Easy };
            var npc2 = new PlacedNpc(mod) { LevelModifier = Level.Medium };
            Setup(mod, rec, npc1, npc2, new PlacedNpc(mod), new PlacedNpc(mod));
        });
    }

    // A dungeon with no placed npcs should not be reported
    [Theory, MutagenModAutoData]
    public void NoNpcs(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            Setup(mod, rec);
        });
    }

    // A non-dungeon interior cell should not be reported
    [Theory, MutagenModAutoData]
    public void NotDungeon(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            var location = new Location(mod);
            mod.Locations.Add(location);
            location.Keywords = [FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeHouse];
            rec.Location.SetTo(location);

            rec.Flags |= Cell.Flag.IsInteriorCell;
            mod.Cells.AddInteriorCell(rec);

            rec.Temporary.Add(new PlacedNpc(mod));
            rec.Temporary.Add(new PlacedNpc(mod));
        });
    }
}
