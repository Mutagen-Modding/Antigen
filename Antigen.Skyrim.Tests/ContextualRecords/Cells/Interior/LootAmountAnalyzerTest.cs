using Antigen.Skyrim.Record.Cell.Interior;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Antigen.Skyrim.Tests.ContextualRecords.Cells.Interior;

using Fixture = ContextualRecordTestFixture<LootAmountAnalyzer, Cell, ICellGetter>;

public class LootAmountAnalyzerTest
{
    static MiscItem Setup(Fixture fixture, ISkyrimMod mod, Cell cell)
    {
        cell.Flags |= Cell.Flag.IsInteriorCell;
        mod.Cells.AddInteriorCell(cell);

        var item = mod.MiscItems.AddNew();

        var placed = fixture.Create<PlacedObject>();
        placed.Base.SetTo(item);
        cell.Persistent.Add(placed);

        return item;
    }

    [Theory, MutagenModAutoData]
    public void TooMuchLoot(Fixture fixture)
    {
        MiscItem? item = null;
        fixture.Run(
            prepForError: (cell, mod) =>
            {
                item = Setup(fixture, mod, cell);
                item.Value = LootAmountAnalyzer.LootValueThreshold + 1;
            },
            prepForFix: (_, _) =>
            {
                item!.Value = LootAmountAnalyzer.LootValueThreshold;
            },
            LootAmountAnalyzer.TooMuchLootTopic);
    }
}
