using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class NotTrimmedAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<string, Language> PromptNotTrimmed = MutagenTopicBuilder.FromDiscussion(
            270,
            "Prompt Not Trimmed",
            Severity.Suggestion)
        .WithFormatting<string, Language>("Prompt '{0}' in {1} is not trimmed");

    public static readonly TopicDefinition<string, Language> ResponseNotTrimmed = MutagenTopicBuilder.FromDiscussion(
            339,
            "Response Not Trimmed",
            Severity.Suggestion)
        .WithFormatting<string, Language>("Response '{0}' in {1} is not trimmed");

    public IEnumerable<TopicDefinition> Topics { get; } = [PromptNotTrimmed, ResponseNotTrimmed];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        // Check prompt
        if (dialogResponses.Prompt is {} prompt)
        {
            foreach (var (language, promptStr) in prompt) {
                if (NotTrimmed(promptStr))
                {
                    param.AddTopic(
                        PromptNotTrimmed.Format(promptStr, language));
                }
            }
        }

        // Check responses
        foreach (var response in dialogResponses.Responses)
        {
            foreach (var (language, text) in response.Text)
            {
                if (NotTrimmed(text))
                {
                    param.AddTopic(
                        ResponseNotTrimmed.Format(text, language));
                }
            }
        }

        static bool NotTrimmed(string text) => text.StartsWith(' ') || text.EndsWith(' ');
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Prompt;
        yield return x => x.Responses;
    }
}
