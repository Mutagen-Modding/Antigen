using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class InvalidCharactersAnalyzerDialogResponses : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<string, string> InvalidCharactersDialogResponses = MutagenTopicBuilder.DevelopmentTopic(
            "Dialog Responses Contains Invalid Characters",
            Severity.Error)
        .WithFormatting<string, string>("Dialog response '{0}' contain invalid characters: {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidCharactersDialogResponses];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        foreach (var response in dialogResponses.Responses)
        {
            if (response.Text.String is null) continue;

            var invalidStrings = InvalidCharactersAnalyzerUtil.InvalidStrings.Where(invalidString => response.Text.String.Contains(invalidString.Key)).ToList();
            if (invalidStrings.Count == 0) continue;

            param.AddTopic(
                InvalidCharactersDialogResponses.Format(response.Text.String, string.Join(" ", invalidStrings.Select(x => x.Key))));
        }
    }

    IEnumerable<Func<IDialogResponsesGetter, object?>> IIsolatedRecordAnalyzer<IDialogResponsesGetter>.FieldsOfInterest()
    {
        yield return x => x.Responses;
    }
}
