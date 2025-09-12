using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using TextReplacementUtil = Mutagen.Bethesda.Analyzers.Skyrim.Util.TextReplacementUtil;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Topic;

public class MissingTextReplacementReferenceAnalyzer : IContextualRecordAnalyzer<IDialogTopicGetter>
{
    public static readonly TopicDefinition<Language, string> DialogPromptMissingGlobalVariable = MutagenTopicBuilder.FromDiscussion(
        439,
            "Dialog Topic prompt references missing Global Variable",
            Severity.Error)
        .WithFormatting<Language, string>("Prompt in {0} references a global variable with EditorID '{1}' that does not exist");

    public static readonly TopicDefinition<Language, string> DialogPromptMissingGlobalVariableInQuest = MutagenTopicBuilder.FromDiscussion(
        440,
            "Dialog Topic prompt references Global Variable not defined in quest",
            Severity.Error)
        .WithFormatting<Language, string>("Prompt in {0} references a global variable with EditorID '{1}' that is not defined in the quest");

    public static readonly TopicDefinition<Language, string> DialogPromptMissingAlias = MutagenTopicBuilder.FromDiscussion(
        441,
            "Dialog Topic prompt references missing Alias",
            Severity.Error)
        .WithFormatting<Language, string>("Prompt in {0} references an alias with name '{1}' that does not exist");

    public static readonly TopicDefinition<Language, string> DialogPromptAliasWithoutFlag = MutagenTopicBuilder.FromDiscussion(
        442,
            "Dialog Topic prompt references Alias without Stores Text",
            Severity.Error)
        .WithFormatting<Language, string>("Prompt in {0} references an alias with name '{1}' that does not have the 'Stores Text' flag set");

    public IEnumerable<TopicDefinition> Topics { get; } =
    [
        DialogPromptMissingGlobalVariable,
        DialogPromptMissingGlobalVariableInQuest,
        DialogPromptMissingAlias,
        DialogPromptAliasWithoutFlag,
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogTopicGetter> param)
    {
        var dialogTopic = param.Record;

        if (dialogTopic.Name is null) return;

        var quest = dialogTopic.Quest.TryResolve(param.LinkCache);
        if (quest is null) return;

        TextReplacementUtil.CheckString(
            quest,
            param.LinkCache,
            dialogTopic.Name, // Expecting dialog to always have a quest
            (language, text) => param.AddTopic(
                DialogPromptMissingGlobalVariableInQuest.Format(language, text)),
            (language, text) => param.AddTopic(
                DialogPromptMissingGlobalVariable.Format(language, text)),
            (language, text) => param.AddTopic(
                DialogPromptMissingAlias.Format(language, text)),
            (language, text) => param.AddTopic(
                DialogPromptAliasWithoutFlag.Format(language, text)));
    }

    public IEnumerable<Func<IDialogTopicGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
    }
}
