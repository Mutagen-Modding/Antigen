using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using TextReplacementUtil = Mutagen.Bethesda.Analyzers.Skyrim.Util.TextReplacementUtil;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class MissingTextReplacementReferenceAnalyzer : IContextualRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<Language, string> DialogPromptMissingGlobalVariable = MutagenTopicBuilder.FromDiscussion(
        443,
            "Dialog responses prompt references missing Global Variable",
            Severity.Error)
        .WithFormatting<Language, string>("Prompt in {0} references a global variable with EditorID '{1}' that does not exist");

    public static readonly TopicDefinition<Language, string> DialogPromptMissingGlobalVariableInQuest = MutagenTopicBuilder.FromDiscussion(
        444,
            "Dialog responses prompt references Global Variable not defined in quest",
            Severity.Error)
        .WithFormatting<Language, string>("Prompt in {0} references a global variable with EditorID '{1}' that is not defined in the quest");

    public static readonly TopicDefinition<Language, string> DialogPromptMissingAlias = MutagenTopicBuilder.FromDiscussion(
        445,
            "Dialog responses prompt references missing Alias",
            Severity.Error)
        .WithFormatting<Language, string>("Prompt in {0} references an alias with name '{1}' that does not exist");

    public static readonly TopicDefinition<Language, string> DialogPromptAliasWithoutFlag = MutagenTopicBuilder.FromDiscussion(
        446,
            "Dialog responses prompt references Alias without Stores Text",
            Severity.Error)
        .WithFormatting<Language, string>("Prompt in {0} references an alias with name '{1}' that does not have the 'Stores Text' flag set");

    public IEnumerable<TopicDefinition> Topics { get; } =
    [
        DialogPromptMissingGlobalVariable,
        DialogPromptMissingGlobalVariableInQuest,
        DialogPromptMissingAlias,
        DialogPromptAliasWithoutFlag,
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        if (dialogResponses.Prompt is null) return;
        if (!param.LinkCache.TryResolveSimpleContext(dialogResponses, out var context)) return;
        if (context.Parent?.Record is not IDialogTopicGetter topic) return;

        var quest = topic.Quest.TryResolve(param.LinkCache);
        if (quest is null) return;

        TextReplacementUtil.CheckString(
            quest,
            param.LinkCache,
            dialogResponses.Prompt, // Expecting dialog to always have a quest
            (language, text) => param.AddTopic(
                DialogPromptMissingGlobalVariableInQuest.Format(language, text)),
            (language, text) => param.AddTopic(
                DialogPromptMissingGlobalVariable.Format(language, text)),
            (language, text) => param.AddTopic(
                DialogPromptMissingAlias.Format(language, text)),
            (language, text) => param.AddTopic(
                DialogPromptAliasWithoutFlag.Format(language, text)));
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Prompt;
    }
}
