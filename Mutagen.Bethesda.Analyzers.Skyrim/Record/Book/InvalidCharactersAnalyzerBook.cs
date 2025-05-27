using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Fonts;
using Mutagen.Bethesda.Fonts.DI;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Book;

public class InvalidCharactersAnalyzerBook(IFontProviderFactory fontProviderFactory, GameConstants gameConstants) : IIsolatedRecordAnalyzer<IBookGetter>
{
    private readonly Dictionary<Language, IFontProvider> _fontProviders = gameConstants.Languages.Append(Language.Japanese)
        .ToDictionary(
            l => l,
            fontProviderFactory.Create);

    public static readonly TopicDefinition<Language> InvalidCharactersBookText = MutagenTopicBuilder.FromDiscussion(
            220,
            "Book Text Contains Invalid Characters",
            Severity.Error)
        .WithFormatting<Language>("Book text contains invalid characters in {0}");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidCharactersBookText];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IBookGetter> param)
    {
        var book = param.Record;

        foreach (var (language, text) in book.BookText)
        {
            var invalidChars = text
                .ToCharArray()
                .Distinct()
                .Where(c => c != '"' && c != '\r' && c != '\n' && c != '\t')
                .Where(c => !_fontProviders[language].ValidNameChars.Contains(c))
                .ToArray();

            if (invalidChars.Length == 0) return;

            param.AddTopic(
                InvalidCharactersBookText.Format(),
                ("Invalid Characters", invalidChars));

        }
    }

    IEnumerable<Func<IBookGetter, object?>> IIsolatedRecordAnalyzer<IBookGetter>.FieldsOfInterest()
    {
        yield return x => x.BookText;
    }
}
