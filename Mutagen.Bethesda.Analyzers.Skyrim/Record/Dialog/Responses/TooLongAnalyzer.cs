using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class TooLongAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    private const int DialogPromptLengthLimit = 80;
    private const int DialogResponseLengthLimit = 149;

    public static readonly TopicDefinition<string, int> PromptTooLong = MutagenTopicBuilder.FromDiscussion(
            274,
            "Prompt Too Long",
            Severity.Suggestion)
        .WithFormatting<string, int>("Prompt '{0}' is {1} longer than the recommended limit " + DialogPromptLengthLimit);

    public static readonly TopicDefinition<string, int> ResponseTooLong = MutagenTopicBuilder.FromDiscussion(
            341,
            "Response Too Long",
            Severity.Suggestion)
        .WithFormatting<string, int>("Response '{0}' is {1} longer than the recommended limit " + DialogResponseLengthLimit);

    public IEnumerable<TopicDefinition> Topics { get; } = [PromptTooLong, ResponseTooLong];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        // Check prompt
        if (dialogResponses.Prompt?.String is { Length: > DialogPromptLengthLimit })
        {
            param.AddTopic(
                PromptTooLong.Format(dialogResponses.Prompt.String, dialogResponses.Prompt.String.Length - DialogPromptLengthLimit));
        }

        // Check responses
        foreach (var response in dialogResponses.Responses
                     .Select(x => x.Text.String)
                     .WhereNotNull()
                     .Where(text => text is { Length: > DialogResponseLengthLimit }))
        {
            param.AddTopic(
                ResponseTooLong.Format(response, response.Length - DialogResponseLengthLimit));
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Prompt;
        yield return x => x.Responses;
    }
}
