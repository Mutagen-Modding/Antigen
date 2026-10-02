using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Antigen.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Antigen.Skyrim.Record.Dialog.Topic;

public class InconsistentCharactersAnalyzer : IIsolatedRecordAnalyzer<IDialogTopicGetter>
{
    public static readonly TopicDefinition<string, Language> PromptInconsistentCharacters = MutagenTopicBuilder.FromDiscussion(
            267,
            "Prompt Has Inconsistent Characters",
            Severity.Suggestion)
        .WithFormatting<string, Language>("Response {0} in {1} contains characters which are not usually used in dialog");

    public IEnumerable<TopicDefinition> Topics { get; } = [PromptInconsistentCharacters];


    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogTopicGetter> param)
    {
        var dialogResponses = param.Record;

        // Check prompt
        if (dialogResponses.Name is null) return;
        foreach (var (language, name) in dialogResponses.Name) {
            InvalidCharactersAnalyzerUtil.CheckInconsistentCharacters(param, name, language, PromptInconsistentCharacters);
        }
    }

    public IEnumerable<Func<IDialogTopicGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
    }
}
