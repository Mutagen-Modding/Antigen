using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Fonts;
using Mutagen.Bethesda.Fonts.DI;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Book;

public class InvalidCharactersAnalyzerBook(IFontProviderFactory fontProviderFactory, Language language) : IIsolatedRecordAnalyzer<IBookGetter>
{
    private readonly IFontProvider _fontProvider = fontProviderFactory.Create(language);

    public static readonly TopicDefinition InvalidCharactersBookText = MutagenTopicBuilder.FromDiscussion(
            220,
            "Book Text Contains Invalid Characters",
            Severity.Error)
        .WithoutFormatting("Book text contains invalid characters");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidCharactersBookText];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IBookGetter> param)
    {
        var book = param.Record;
        if (!book.BookText.TryLookup(language, out var str)) return;

        var invalidChars = str
            .ToCharArray()
            .Distinct()
            .Where(c => c != '"' && c != '\r' && c != '\n' && c != '\t')
            .Where(c => !_fontProvider.ValidNameChars.Contains(c))
            .ToArray();

        if (invalidChars.Length == 0) return;

        param.AddTopic(
            InvalidCharactersBookText.Format(),
            ("Invalid Characters", invalidChars));
    }

    IEnumerable<Func<IBookGetter, object?>> IIsolatedRecordAnalyzer<IBookGetter>.FieldsOfInterest()
    {
        yield return x => x.BookText;
    }
}
