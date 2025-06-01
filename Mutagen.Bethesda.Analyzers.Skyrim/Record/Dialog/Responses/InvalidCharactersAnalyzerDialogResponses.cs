using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Fonts;
using Mutagen.Bethesda.Fonts.DI;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class InvalidCharactersAnalyzerDialogResponses : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<string, Language> InvalidCharactersDialogResponses = MutagenTopicBuilder.FromDiscussion(
            268,
            "Dialog Responses Contains Invalid Characters",
            Severity.Error)
        .WithFormatting<string, Language>("Dialog response '{0}' in {1} contain invalid characters");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidCharactersDialogResponses];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        foreach (var response in dialogResponses.Responses)
        {
            foreach (var (language, text) in response.Text)
            {
                var invalidChars = text
                    .ToCharArray()
                    .Distinct()
                    .Where(c => c != '"')
                    .Where(c => InvalidCharactersAnalyzerUtil.InvalidStrings.ContainsKey(c))
                    .ToArray();

                if (invalidChars.Length == 0) continue;

                param.AddTopic(
                    InvalidCharactersDialogResponses.Format(text, language),
                    ("Invalid Characters", invalidChars));

            }
        }
    }

    IEnumerable<Func<IDialogResponsesGetter, object?>> IIsolatedRecordAnalyzer<IDialogResponsesGetter>.FieldsOfInterest()
    {
        yield return x => x.Responses;
    }
}
