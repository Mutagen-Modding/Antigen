using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;

public class MissingTextReplacementReferenceAnalyzer : IContextualRecordAnalyzer<IQuestGetter>
{
    public static readonly TopicDefinition<ushort, Language, string> LogEntryMissingGlobalVariable = MutagenTopicBuilder.FromDiscussion(
            423,
            "Log Entry references missing Global Variable",
            Severity.Error)
        .WithFormatting<ushort, Language, string>("Log entry in stage {0} in {1} references a global variable with EditorID '{2}' that does not exist");

    public static readonly TopicDefinition<ushort, Language, string> LogEntryMissingGlobalVariableInQuest = MutagenTopicBuilder.FromDiscussion(
            424,
            "Log Entry references Global Variable not defined in quest",
            Severity.Error)
        .WithFormatting<ushort, Language, string>("Log entry in stage {0} in {1} references a global variable with EditorID '{2}' that is not defined in the quest");

    public static readonly TopicDefinition<ushort, Language, string> LogEntryMissingAlias = MutagenTopicBuilder.FromDiscussion(
            425,
            "Log Entry references missing Alias",
            Severity.Error)
        .WithFormatting<ushort, Language, string>("Log entry in stage {0} in {1} references an alias with name '{2}' that does not exist");

    public static readonly TopicDefinition<ushort, Language, string> LogEntryAliasWithoutFlag = MutagenTopicBuilder.FromDiscussion(
            426,
            "Log Entry references Alias without Stores Text",
            Severity.Error)
        .WithFormatting<ushort, Language, string>("Log entry in stage {0} in {1} references an alias with name '{2}' that does not have the 'Stores Text' flag set");

    public static readonly TopicDefinition<ushort, Language, string> ObjectiveMissingGlobalVariable = MutagenTopicBuilder.FromDiscussion(
            427,
            "Objective references missing Global Variable",
            Severity.Error)
        .WithFormatting<ushort, Language, string>("Objective with index {0} in {1} references a global variable with EditorID '{2}' that does not exist");

    public static readonly TopicDefinition<ushort, Language, string> ObjectiveMissingGlobalVariableInQuest = MutagenTopicBuilder.FromDiscussion(
            428,
            "Objective references Global Variable not defined in quest",
            Severity.Error)
        .WithFormatting<ushort, Language, string>("Objective with index {0} in {1} references a global variable with EditorID '{2}' that is not defined in the quest");

    public static readonly TopicDefinition<ushort, Language, string> ObjectiveMissingAlias = MutagenTopicBuilder.FromDiscussion(
            429,
            "Objective references missing Alias",
            Severity.Error)
        .WithFormatting<ushort, Language, string>("Objective with index {0} in {1} references an alias with name '{2}' that does not exist");

    public static readonly TopicDefinition<ushort, Language, string> ObjectiveAliasWithoutFlag = MutagenTopicBuilder.FromDiscussion(
            430,
            "Objective references Alias without Stores Text",
            Severity.Error)
        .WithFormatting<ushort, Language, string>("Objective with index {0} in {1} references an alias with name '{2}' that does not have the 'Stores Text' flag set");

    public static readonly TopicDefinition<IMessageGetter, Language, string> MessageTitleMissingGlobalVariable = MutagenTopicBuilder.FromDiscussion(
            431,
            "Message title references missing Global Variable",
            Severity.Error)
        .WithFormatting<IMessageGetter, Language, string>("Message {0} used as display name in alias '{1}' references a global variable with EditorID '{2}' that does not exist");

    public static readonly TopicDefinition<IMessageGetter, Language, string> MessageTitleMissingGlobalVariableInQuest = MutagenTopicBuilder.FromDiscussion(
            432,
            "Message title references Global Variable not defined in quest",
            Severity.Error)
        .WithFormatting<IMessageGetter, Language, string>("Message {0} used as display name in alias '{1}' references a global variable with EditorID '{2}' that is not defined in the quest");

    public static readonly TopicDefinition<IMessageGetter, Language, string> MessageTitleMissingAlias = MutagenTopicBuilder.FromDiscussion(
            433,
            "Message title references missing Alias",
            Severity.Error)
        .WithFormatting<IMessageGetter, Language, string>("Message {0} used as display name in alias '{1}' references an alias with name '{2}' that does not exist");

    public static readonly TopicDefinition<IMessageGetter, Language, string> MessageTitleAliasWithoutFlag = MutagenTopicBuilder.FromDiscussion(
            434,
            "Message title references Alias without Stores Text",
            Severity.Error)
        .WithFormatting<IMessageGetter, Language, string>("Message {0} used as display name in alias '{1}' references an alias with name '{2}' that does not have the 'Stores Text' flag set");

    public static readonly TopicDefinition<IMessageGetter, Language, string> MessageDescriptionMissingGlobalVariable = MutagenTopicBuilder.FromDiscussion(
            435,
            "Message description references missing Global Variable",
            Severity.Error)
        .WithFormatting<IMessageGetter, Language, string>("Message {0} used as display name in alias '{1}' references a global variable with EditorID '{2}' that does not exist");

    public static readonly TopicDefinition<IMessageGetter, Language, string> MessageDescriptionMissingGlobalVariableInQuest = MutagenTopicBuilder.FromDiscussion(
            436,
            "Message description references Global Variable not defined in quest",
            Severity.Error)
        .WithFormatting<IMessageGetter, Language, string>("Message {0} used as display name in alias '{1}' references a global variable with EditorID '{2}' that is not defined in the quest");

    public static readonly TopicDefinition<IMessageGetter, Language, string> MessageDescriptionMissingAlias = MutagenTopicBuilder.FromDiscussion(
            437,
            "Message description references missing Alias",
            Severity.Error)
        .WithFormatting<IMessageGetter, Language, string>("Message {0} used as display name in alias '{1}' references an alias with name '{2}' that does not exist");

    public static readonly TopicDefinition<IMessageGetter, Language, string> MessageDescriptionAliasWithoutFlag = MutagenTopicBuilder.FromDiscussion(
            438,
            "Message description references Alias without Stores Text",
            Severity.Error)
        .WithFormatting<IMessageGetter, Language, string>("Message {0} used as display name in alias '{1}' references an alias with name '{2}' that does not have the 'Stores Text' flag set");

    public static readonly TopicDefinition<IBookGetter, Language, string> BookTextMissingGlobalVariable = MutagenTopicBuilder.FromDiscussion(
            447,
            "Book Text references missing Global Variable",
            Severity.Error)
        .WithFormatting<IBookGetter, Language, string>("Text of book {0} in alias '{1}' references a global variable with EditorID '{2}' that does not exist");

    public static readonly TopicDefinition<IBookGetter, Language, string> BookTextMissingGlobalVariableInQuest = MutagenTopicBuilder.FromDiscussion(
            448,
            "Book Text references Global Variable not defined in quest",
            Severity.Error)
        .WithFormatting<IBookGetter, Language, string>("Text of book {0} in alias '{1}' references a global variable with EditorID '{2}' that is not defined in the quest");

    public static readonly TopicDefinition<IBookGetter, Language, string> BookTextMissingAlias = MutagenTopicBuilder.FromDiscussion(
            449,
            "Book Text references missing Alias",
            Severity.Error)
        .WithFormatting<IBookGetter, Language, string>("Text of book {0} in alias '{1}' references an alias with name '{2}' that does not exist");

    public static readonly TopicDefinition<IBookGetter, Language, string> BookTextAliasWithoutFlag = MutagenTopicBuilder.FromDiscussion(
            450,
            "Book Text references Alias without Stores Text",
            Severity.Error)
        .WithFormatting<IBookGetter, Language, string>("Text of book {0} in alias '{1}' references an alias with name '{2}' that does not have the 'Stores Text' flag set");

    public IEnumerable<TopicDefinition> Topics { get; } =
    [
        LogEntryMissingGlobalVariable,
        LogEntryMissingGlobalVariableInQuest,
        LogEntryMissingAlias,
        LogEntryAliasWithoutFlag,
        ObjectiveMissingGlobalVariable,
        ObjectiveMissingGlobalVariableInQuest,
        ObjectiveMissingAlias,
        ObjectiveAliasWithoutFlag,
        MessageTitleMissingGlobalVariable,
        MessageTitleMissingGlobalVariableInQuest,
        MessageTitleMissingAlias,
        MessageTitleAliasWithoutFlag,
        MessageDescriptionMissingGlobalVariable,
        MessageDescriptionMissingGlobalVariableInQuest,
        MessageDescriptionMissingAlias,
        MessageDescriptionAliasWithoutFlag,
        BookTextMissingGlobalVariable,
        BookTextMissingGlobalVariableInQuest,
        BookTextMissingAlias,
        BookTextAliasWithoutFlag,
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IQuestGetter> param)
    {
        var quest = param.Record;
        if (quest.IsDeleted) return;

        foreach (var stage in quest.Stages)
        {
            foreach (var logEntry in stage.LogEntries)
            {
                if (logEntry.Entry is null) continue;

                Util.TextReplacementUtil.CheckString(
                    quest,
                    param.LinkCache,
                    logEntry.Entry,
                    (language, text) => param.AddTopic(
                        LogEntryMissingGlobalVariableInQuest.Format(stage.Index, language, text)),
                    (language, text) => param.AddTopic(
                        LogEntryMissingGlobalVariable.Format(stage.Index, language, text)),
                    (language, text) => param.AddTopic(
                        LogEntryMissingAlias.Format(stage.Index, language, text)),
                    (language, text) => param.AddTopic(
                        LogEntryAliasWithoutFlag.Format(stage.Index, language, text)));
            }
        }

        foreach (var objective in quest.Objectives)
        {
            if (objective.DisplayText is null) continue;

            Util.TextReplacementUtil.CheckString(
                quest,
                param.LinkCache,
                objective.DisplayText,
                (language, text) => param.AddTopic(
                    ObjectiveMissingGlobalVariableInQuest.Format(objective.Index, language, text)),
                (language, text) => param.AddTopic(
                    ObjectiveMissingGlobalVariable.Format(objective.Index, language, text)),
                (language, text) => param.AddTopic(
                    ObjectiveMissingAlias.Format(objective.Index, language, text)),
                (language, text) => param.AddTopic(
                    ObjectiveAliasWithoutFlag.Format(objective.Index, language, text)));
        }

        foreach (var alias in quest.Aliases)
        {
            if (alias.DisplayName.IsNull)
            {
                var message = alias.DisplayName.TryResolve(param.LinkCache);
                if (message is not null)
                {
                    if (message.Name is not null)
                    {
                        Util.TextReplacementUtil.CheckString(
                            quest,
                            param.LinkCache,
                            message.Name,
                            (language, text) => param.AddTopic(
                                MessageTitleMissingGlobalVariableInQuest.Format(message, language, text)),
                            (language, text) => param.AddTopic(
                                MessageTitleMissingGlobalVariable.Format(message, language, text)),
                            (language, text) => param.AddTopic(
                                MessageTitleMissingAlias.Format(message, language, text)),
                            (language, text) => param.AddTopic(
                                MessageTitleAliasWithoutFlag.Format(message, language, text)));
                    }

                    Util.TextReplacementUtil.CheckString(
                        quest,
                        param.LinkCache,
                        message.Description,
                        (language, text) => param.AddTopic(
                            MessageDescriptionMissingGlobalVariableInQuest.Format(message, language, text)),
                        (language, text) => param.AddTopic(
                            MessageDescriptionMissingGlobalVariable.Format(message, language, text)),
                        (language, text) => param.AddTopic(
                            MessageDescriptionMissingAlias.Format(message, language, text)),
                        (language, text) => param.AddTopic(
                            MessageDescriptionAliasWithoutFlag.Format(message, language, text)));
                }
            }

            if (!alias.ForcedReference.IsNull)
            {
                var placedObject = alias.ForcedReference.TryResolve<IPlacedObjectGetter>(param.LinkCache);
                var book = placedObject?.Base.TryResolve<IBookGetter>(param.LinkCache);
                if (book is not null)
                {
                    Util.TextReplacementUtil.CheckString(
                        quest,
                        param.LinkCache,
                        book.BookText,
                        (language, text) => param.AddTopic(
                            BookTextMissingGlobalVariableInQuest.Format(book, language, text)),
                        (language, text) => param.AddTopic(
                            BookTextMissingGlobalVariable.Format(book, language, text)),
                        (language, text) => param.AddTopic(
                            BookTextMissingAlias.Format(book, language, text)),
                        (language, text) => param.AddTopic(
                            BookTextAliasWithoutFlag.Format(book, language, text)));
                }
            }

            if (alias.CreateReferenceToObject is not null)
            {
                var book = alias.CreateReferenceToObject.Object.TryResolve<IBookGetter>(param.LinkCache);
                if (book is not null)
                {
                    Util.TextReplacementUtil.CheckString(
                        quest,
                        param.LinkCache,
                        book.BookText,
                        (language, text) => param.AddTopic(
                            BookTextMissingGlobalVariableInQuest.Format(book, language, text)),
                        (language, text) => param.AddTopic(
                            BookTextMissingGlobalVariable.Format(book, language, text)),
                        (language, text) => param.AddTopic(
                            BookTextMissingAlias.Format(book, language, text)),
                        (language, text) => param.AddTopic(
                            BookTextAliasWithoutFlag.Format(book, language, text)));
                }
            }
        }
    }

    public IEnumerable<Func<IQuestGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Aliases;
        yield return x => x.Objectives;
        yield return x => x.Stages;
    }
}
