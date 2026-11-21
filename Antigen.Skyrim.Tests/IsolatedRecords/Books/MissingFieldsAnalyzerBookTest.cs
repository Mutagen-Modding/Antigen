using Antigen.Skyrim.Record.Book;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Antigen.Skyrim.Tests.IsolatedRecords.Books;

using Keywords = FormKeys.SkyrimSE.Skyrim.Keyword;
using Statics = FormKeys.SkyrimSE.Skyrim.Static;

using Fixture = IsolatedRecordTestFixture<MissingFieldsAnalyzerBook, Book, IBookGetter>;

public class MissingFieldsAnalyzerBookTest
{
    [Theory, MutagenModAutoData]
    public void DummyItemExcluded(Fixture fixture)
    {
        fixture.RunShouldBeNoError(
            rec =>
            {
                rec.Keywords = [Keywords.Dummy];
            });
    }

    [Theory, MutagenModAutoData]
    public void NoTeachesSpell(Fixture fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.InventoryArt.SetTo(Statics.HighPolyBasicBook01);
                rec.Teaches = new BookSpell();
            },
            prepForFix: rec =>
            {
                (rec.Teaches as BookSpell)!.Spell.SetTo(FormKeys.SkyrimSE.Skyrim.Spell.Flames);
            },
            MissingFieldsAnalyzerBook.NoTeachesSpell);
    }

    [Theory, MutagenModAutoData]
    public void NoNoteUpSound(Fixture fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.InventoryArt.SetTo(Statics.HighPolyNote);
                rec.Model = new() { File = @"Clutter\Books\Note01.nif" };
            },
            prepForFix: rec =>
            {
                rec.PickUpSound.SetTo(FormKeys.SkyrimSE.Skyrim.SoundDescriptor.ITMNoteUp);
            },
            MissingFieldsAnalyzerBook.NoNoteUpSound);
    }

    [Theory, MutagenModAutoData]
    public void NoInventoryArt(Fixture fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.InventoryArt.SetToNull();
            },
            prepForFix: rec =>
            {
                rec.InventoryArt.SetTo(Statics.HighPolyBasicBook01);
            },
            MissingFieldsAnalyzerBook.NoInventoryArt);
    }
}
