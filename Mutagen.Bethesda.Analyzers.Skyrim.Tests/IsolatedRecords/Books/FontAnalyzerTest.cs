using Mutagen.Bethesda.Analyzers.Skyrim.Record.Book;
using Mutagen.Bethesda.Analyzers.Testing.AutoFixture;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Books;

public class FontAnalyzerTest
{
    [Theory]
    [MutagenModTheoryData("'$BadFont'", "'$HandwrittenFont'")] // Font name must be valid
    [MutagenModTheoryData("'$BadFont'", "\"$HandwrittenFont\"")] // Single or double quotes are allowed
    [MutagenModTheoryData("'$BadFont'", "'$HandwrittenFont\"")] // Mismatched quote types are ok
    [MutagenModTheoryData("$HandwrittenFont", "'$HandwrittenFont'")] // Quotes are required
    [MutagenModTheoryData("'$HandwrittenFont", "'$HandwrittenFont'")] // Quotes must be closed
    [MutagenModTheoryData("'$BadFont'", "'$HANDWRITTENFONT'")] // Font names are case insensitve
    [MutagenModTheoryData("'HandwrittenFont'", "'$HandwrittenFont'")] // The $ is required
    public void InvalidFont(string bad, string good, IsolatedRecordTestFixture<FontAnalyzer, Book, IBookGetter> fixture)
    {
        fixture.Run(
            prepForError: rec =>
            {
                rec.BookText = $"<font face={bad}>";
            },
            prepForFix: rec =>
            {
                rec.BookText = rec.BookText.String!.Replace(bad, good);
            },
            FontAnalyzer.InvalidFont);
    }
}
