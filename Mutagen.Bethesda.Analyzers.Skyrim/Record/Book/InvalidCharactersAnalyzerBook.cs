using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Book;

public class InvalidCharactersAnalyzerBook : IIsolatedRecordAnalyzer<IBookGetter>
{
    public static readonly TopicDefinition InvalidCharactersBookText = MutagenTopicBuilder.DevelopmentTopic(
            "Book Text Contains Invalid Characters",
            Severity.Error)
        .WithoutFormatting("Book text contains invalid characters");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidCharactersBookText];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IBookGetter> param)
    {
        var book = param.Record;
        if (book.BookText.String is null) return;

        var invalidStrings = InvalidCharactersAnalyzerUtil.InvalidStrings.Where(invalidString => book.BookText.String.Contains(invalidString.Key)).ToList();
        if (invalidStrings.Count == 0) return;

        param.AddTopic(
            InvalidCharactersBookText.Format(),
            ("Invalid Characters", invalidStrings.Select(x => x.Key)));
    }

    IEnumerable<Func<IBookGetter, object?>> IIsolatedRecordAnalyzer<IBookGetter>.FieldsOfInterest()
    {
        yield return x => x.BookText;
    }
}
