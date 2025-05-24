using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Fonts;
using Mutagen.Bethesda.Fonts.DI;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class InvalidCharactersAnalyzerDialogResponses(IFontProviderFactory fontProviderFactory, Language language) : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    private readonly IFontProvider _fontProvider = fontProviderFactory.Create(language);

    public static readonly TopicDefinition<string> InvalidCharactersDialogResponses = MutagenTopicBuilder.FromDiscussion(
            268,
            "Dialog Responses Contains Invalid Characters",
            Severity.Error)
        .WithFormatting<string>("Dialog response '{0}' contain invalid characters");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidCharactersDialogResponses];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        foreach (var response in dialogResponses.Responses)
        {
            if (!response.Text.TryLookup(language, out var str)) continue;

            var invalidChars = str
                .ToCharArray()
                .Distinct()
                .Where(c => c != '"')
                .Where(c => !_fontProvider.ValidNameChars.Contains(c))
                .ToArray();

            if (invalidChars.Length == 0) continue;

            param.AddTopic(
                InvalidCharactersDialogResponses.Format(),
                    ("Invalid Characters", invalidChars));
        }
    }

    IEnumerable<Func<IDialogResponsesGetter, object?>> IIsolatedRecordAnalyzer<IDialogResponsesGetter>.FieldsOfInterest()
    {
        yield return x => x.Responses;
    }
}
