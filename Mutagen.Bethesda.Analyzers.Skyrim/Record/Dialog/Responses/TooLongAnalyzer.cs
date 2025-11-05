using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class TooLongAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    private const int DialogPromptLengthLimit = 80;
    private const int DialogResponseLengthLimit = 149;

    public static readonly TopicDefinition<string, Language, int> PromptTooLong = MutagenTopicBuilder.FromDiscussion(
            274,
            "Prompt Too Long",
            Severity.Suggestion)
        .WithFormatting<string, Language, int>("Prompt '{0}' in {1} is {2} longer than the recommended limit " + DialogPromptLengthLimit);

    public static readonly TopicDefinition<string, Language, int> ResponseTooLong = MutagenTopicBuilder.FromDiscussion(
            341,
            "Response Too Long",
            Severity.Suggestion)
        .WithFormatting<string, Language, int>("Response '{0}' in {1} is {2} longer than the recommended limit " + DialogResponseLengthLimit);

    public IEnumerable<TopicDefinition> Topics { get; } = [PromptTooLong, ResponseTooLong];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        // Check prompt
        if (dialogResponses.Prompt is {} prompt)
        {
            foreach (var (language, promptStr) in prompt) {
                if (promptStr.Length > DialogPromptLengthLimit)
                {
                    param.AddTopic(
                        PromptTooLong.Format(promptStr, language, promptStr.Length - DialogPromptLengthLimit));
                }
            }
        }

        // Check responses
        foreach (var response in dialogResponses.Responses)
        {
            foreach (var (language, text) in response.Text)
            {
                if (text.Length > DialogResponseLengthLimit)
                {
                    param.AddTopic(
                        ResponseTooLong.Format(text, language, text.Length - DialogResponseLengthLimit));
                }
            }
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Prompt;
        yield return x => x.Responses;
    }
}
