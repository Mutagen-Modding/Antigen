using AutoFixture.Xunit2;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Book;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Books;

public class FontAnalyzerTest
{
    public class FontAnalyzerDataAttribute(params object[] objects) : CompositeDataAttribute(new InlineDataAttribute(objects), new MutagenModAutoDataAttribute())
    {
    }

    [Theory]
    [FontAnalyzerData("'$BadFont'", "'$HandwrittenFont'")] // Font name must be valid
    [FontAnalyzerData("'$BadFont'", "\"$HandwrittenFont\"")] // Single or double quotes are allowed
    [FontAnalyzerData("'$BadFont'", "'$HandwrittenFont\"")] // Mismatched quote types are ok
    [FontAnalyzerData("$HandwrittenFont", "'$HandwrittenFont'")] // Quotes are required
    [FontAnalyzerData("'$HandwrittenFont", "'$HandwrittenFont'")] // Quotes must be closed
    [FontAnalyzerData("'$BadFont'", "'$HANDWRITTENFONT'")] // Font names are case insensitve
    [FontAnalyzerData("'HandwrittenFont'", "'$HandwrittenFont'")] // The $ is required
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
