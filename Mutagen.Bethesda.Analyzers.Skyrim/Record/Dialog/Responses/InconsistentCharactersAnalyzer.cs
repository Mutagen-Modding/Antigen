using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class InconsistentCharactersAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<string> PromptInconsistentCharacters = MutagenTopicBuilder.FromDiscussion(
            275,
            "Prompt Has Inconsistent Characters",
            Severity.Suggestion)
        .WithFormatting<string>("Prompt {0} contains characters which are not usually used in dialog");

    public static readonly TopicDefinition<string> ResponseInconsistentCharacters = MutagenTopicBuilder.FromDiscussion(
            337,
            "Response Has Inconsistent Characters",
            Severity.Suggestion)
        .WithFormatting<string>("Response {0} contains characters which are not usually used in dialog");

    public IEnumerable<TopicDefinition> Topics { get; } = [PromptInconsistentCharacters, ResponseInconsistentCharacters];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        // Check prompt
        if (dialogResponses.Prompt?.String is not null)
        {
            InvalidCharactersAnalyzerUtil.CheckInconsistentCharacters(param, dialogResponses.Prompt.String, PromptInconsistentCharacters);
        }

        // Check responses
        foreach (var response in dialogResponses.Responses
                     .Select(x => x.Text.String)
                     .WhereNotNull())
        {
            InvalidCharactersAnalyzerUtil.CheckInconsistentCharacters(param, response, ResponseInconsistentCharacters);
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Prompt;
        yield return x => x.Responses;
    }
}
