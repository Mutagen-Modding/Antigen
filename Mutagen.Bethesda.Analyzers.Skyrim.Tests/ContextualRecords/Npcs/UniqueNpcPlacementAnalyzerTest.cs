using Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Npcs;

using Fixture = ContextualRecordTestFixture<UniqueNpcPlacementAnalyzer, Npc, INpcGetter>;

public class UniqueNpcPlacementAnalyzerTest
{
    // A unique NPC must be placed exactly once
    [Theory, MutagenModAutoData]
    public void PlacedMultiple(
        Fixture fixture)
    {
        var cell = fixture.Create<Cell>();
        var npc1 = fixture.Create<PlacedNpc>();
        var npc2 = fixture.Create<PlacedNpc>();

        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.Configuration.Flags |= NpcConfiguration.Flag.Unique;

                cell.Flags |= Cell.Flag.IsInteriorCell;
                mod.Cells.AddInteriorCell(cell);
                npc1.Base.SetTo(rec);
                cell.Temporary.Add(npc1);
                npc2.Base.SetTo(rec);
                cell.Temporary.Add(npc2);
            },
            prepForFix: (rec, mod) =>
            {
                cell.Temporary.Remove(npc2);
            },
            UniqueNpcPlacementAnalyzer.PlacedMultiple);
    }

    // A unique NPC must be placed exactly once
    [Theory, MutagenModAutoData]
    public void PlacedNever(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.Configuration.Flags |= NpcConfiguration.Flag.Unique;
            },
            prepForFix: (rec, mod) =>
            {
                var cell = fixture.Create<Cell>();
                cell.Flags |= Cell.Flag.IsInteriorCell;
                mod.Cells.AddInteriorCell(cell);
                var npc = fixture.Create<PlacedNpc>();
                npc.Base.SetTo(rec);
                cell.Temporary.Add(npc);
            },
            UniqueNpcPlacementAnalyzer.PlacedNever);
    }

    // A non-unique NPC may be placed multiple times
    [Theory, MutagenModAutoData]
    public void NotUnique(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            rec.Configuration.Flags &= ~NpcConfiguration.Flag.Unique;
            var cell = fixture.Create<Cell>();
            cell.Flags |= Cell.Flag.IsInteriorCell;
            mod.Cells.AddInteriorCell(cell);

            var npc1 = fixture.Create<PlacedNpc>();
            npc1.Base.SetTo(rec);
            cell.Temporary.Add(npc1);

            var npc2 = fixture.Create<PlacedNpc>();
            npc2.Base.SetTo(rec);
            cell.Temporary.Add(npc2);
        });
    }

    // A unique NPC may be referenced by fields other than PlacedNpc.Base
    [Theory, MutagenModAutoData]
    public void ReferencedOtherField(Fixture fixture)
    {
        var npc = fixture.Create<Npc>();
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            rec.Configuration.Flags |= NpcConfiguration.Flag.Unique;
            var cell = fixture.Create<Cell>();
            cell.Flags |= Cell.Flag.IsInteriorCell;
            mod.Cells.AddInteriorCell(cell);

            var npc1 = fixture.Create<PlacedNpc>();
            npc1.Base.SetTo(rec);
            cell.Temporary.Add(npc1);

            var npc2 = fixture.Create<PlacedNpc>();
            npc2.Owner.SetTo(rec);
            cell.Temporary.Add(npc2);
        });
    }
}
