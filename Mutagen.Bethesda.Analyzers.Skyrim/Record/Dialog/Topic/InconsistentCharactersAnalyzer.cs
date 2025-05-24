using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Topic;

public class InconsistentCharactersAnalyzer : IIsolatedRecordAnalyzer<IDialogTopicGetter>
{
    public static readonly TopicDefinition<string> PromptInconsistentCharacters = MutagenTopicBuilder.FromDiscussion(
            267,
            "Prompt Has Inconsistent Characters",
            Severity.Suggestion)
        .WithFormatting<string>("Response {0} contains characters which are not usually used in dialog");

    public IEnumerable<TopicDefinition> Topics { get; } = [PromptInconsistentCharacters];


    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogTopicGetter> param)
    {
        var dialogResponses = param.Record;

        // Check prompt
        if (dialogResponses.Name?.String is not null)
        {
            InvalidCharactersAnalyzerUtil.CheckInconsistentCharacters(param, dialogResponses.Name.String, PromptInconsistentCharacters);
        }
    }

    public IEnumerable<Func<IDialogTopicGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
    }
}
