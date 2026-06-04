using Mutagen.Bethesda.Analyzers.Skyrim.Record.FormList;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.FormLists;

using Fixture = ContextualRecordTestFixture<FormListWithPlacedRecordAnalyzer, FormList, IFormListGetter>;

public class FormListWithPlacedRecordAnalyzerTest
{
    [Theory, MutagenModAutoData]
    public void DetectFormListWithIPlacedRecord(Fixture fixture)
    {
        var cell = fixture.Create<Cell>();
        PlacedNpc placedNpc = fixture.Create<PlacedNpc>();
        FormList list = fixture.Create<FormList>();

        fixture.Run(
            prepForError: (rec, mod) =>
            {
                cell.Flags |= Cell.Flag.IsInteriorCell;
                mod.Cells.AddInteriorCell(cell);
                cell.Temporary.Add(placedNpc);
                rec.Items.Add(placedNpc.ToLink());
            },
            prepForFix: (rec, mod) =>
            {
                rec.Items!.Clear();
            },
            FormListWithPlacedRecordAnalyzer.FormListWithIPlacedRecord
        );
    }
}
