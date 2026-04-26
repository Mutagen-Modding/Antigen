using Mutagen.Bethesda.Analyzers.Skyrim.Record.Book;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.IsolatedRecords.Books;

public class InvalidCharactersAnalyzerBookTest
{
    [Theory, MutagenModAutoData]
    public void DetectsInvalidCharactersInBooks(
        IsolatedRecordTestFixture<InvalidCharactersAnalyzerBook, Book, IBookGetter> fixture)
    {
        fixture.Run(
            prepForError: rec => rec.BookText = new TranslatedString(Language.English,
                    new KeyValuePair<Language, string>(Language.English, "E’x’ample text"),
                    new KeyValuePair<Language, string>(Language.French, "Example te`xt"),
                    new KeyValuePair<Language, string>(Language.Danish, "Exa”mple text"),
                    new KeyValuePair<Language, string>(Language.Arabic, "Example—text"),
                    new KeyValuePair<Language, string>(Language.Chinese, "Example… text"))
            ,
            prepForFix: rec => rec.BookText = new TranslatedString(Language.English,
                new KeyValuePair<Language, string>(Language.English, "Example text"),
                new KeyValuePair<Language, string>(Language.French, "Example \r text"),
                new KeyValuePair<Language, string>(Language.Danish, "Example \n text"),
                new KeyValuePair<Language, string>(Language.Arabic, "Example \t text"),
                new KeyValuePair<Language, string>(Language.Chinese, "Example text...")),
            new []
            {
                InvalidCharactersAnalyzerBook.InvalidCharactersBookText,
                InvalidCharactersAnalyzerBook.InvalidCharactersBookText,
                InvalidCharactersAnalyzerBook.InvalidCharactersBookText,
                InvalidCharactersAnalyzerBook.InvalidCharactersBookText,
                InvalidCharactersAnalyzerBook.InvalidCharactersBookText
            });
    }
}
