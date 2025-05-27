using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class InconsistentCharactersAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<string, Language> PromptInconsistentCharacters = MutagenTopicBuilder.FromDiscussion(
            275,
            "Prompt Has Inconsistent Characters",
            Severity.Suggestion)
        .WithFormatting<string, Language>("Prompt {0} in {1} contains characters which are not usually used in dialog");

    public static readonly TopicDefinition<string, Language> ResponseInconsistentCharacters = MutagenTopicBuilder.FromDiscussion(
            337,
            "Response Has Inconsistent Characters",
            Severity.Suggestion)
        .WithFormatting<string, Language>("Response {0} in {1} contains characters which are not usually used in dialog");

    public IEnumerable<TopicDefinition> Topics { get; } = [PromptInconsistentCharacters, ResponseInconsistentCharacters];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        // Check prompt
        if (dialogResponses.Prompt is {} prompt)
        {
            foreach (var (language, promptStr) in prompt) {
                InvalidCharactersAnalyzerUtil.CheckInconsistentCharacters(param, promptStr, language, PromptInconsistentCharacters);
            }
        }

        // Check responses
        foreach (var response in dialogResponses.Responses)
        {
            foreach (var (language, text) in response.Text)
            {
                InvalidCharactersAnalyzerUtil.CheckInconsistentCharacters(param, text, language, ResponseInconsistentCharacters);
            }
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Prompt;
        yield return x => x.Responses;
    }
}
