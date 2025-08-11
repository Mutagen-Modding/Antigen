using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class InconsistentEndAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<string, Language> PromptNotEndingWithEndCharacter = MutagenTopicBuilder.FromDiscussion(
            469,
            "Prompt Does Not End With End Character",
            Severity.Suggestion)
        .WithFormatting<string, Language>("Prompt '{0}' in {1} does not end with an end character");

    public static readonly TopicDefinition<string, Language> ResponseNotEndingWithEndCharacter = MutagenTopicBuilder.FromDiscussion(
            470,
            "Response Does Not End With End Character",
            Severity.Suggestion)
        .WithFormatting<string, Language>("Response '{0}' in {1} does not end with an end character");

    private static readonly HashSet<char> PromptEndCharacters = ['.', '!', '?', ')'];
    private static readonly HashSet<char> ResponseEndCharacters = ['.', '!', '?', '-', ')', ':', '"'];

    public IEnumerable<TopicDefinition> Topics { get; } = [PromptNotEndingWithEndCharacter, ResponseNotEndingWithEndCharacter];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        // Check prompt
        if (dialogResponses.Prompt is {} prompt)
        {
            foreach (var (language, promptStr) in prompt)
            {
                if (language is Language.Japanese or Language.Korean or Language.Chinese or Language.ChineseSimplified or Language.Russian) continue;

                if (promptStr.Length == 0) continue;
                if (!PromptEndCharacters.Contains(promptStr[^1]))
                {
                    param.AddTopic(
                        PromptNotEndingWithEndCharacter.Format(promptStr, language));
                }
            }
        }

        // Check responses
        foreach (var response in dialogResponses.Responses)
        {
            foreach (var (language, text) in response.Text)
            {
                if (language is Language.Japanese or Language.Korean or Language.Chinese or Language.ChineseSimplified or Language.Russian) continue;

                if (text.Length == 0) continue;
                if (text == " ") continue;

                if (!ResponseEndCharacters.Contains(text[^1]))
                {
                    param.AddTopic(
                        ResponseNotEndingWithEndCharacter.Format(text, language));
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
