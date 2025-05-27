using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class InvalidSayOnceAnalyzer : IContextualRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<IQuestGetter> InvalidSayOnce = MutagenTopicBuilder.FromDiscussion(
            269,
            "Invalid SayOnce",
            Severity.Error)
        .WithFormatting<IQuestGetter>("Dialog is say once although its quest {0} is not Start Game Enabled");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidSayOnce];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        if (dialogResponses.Flags is null || !dialogResponses.Flags.Flags.HasFlag(DialogResponses.Flag.SayOnce)) return;

        var context = param.LinkCache.ResolveSimpleContext<IDialogResponsesGetter>(dialogResponses.FormKey);
        if (context.Parent?.Record is not IDialogTopicGetter topic) return;

        var quest = topic.Quest.TryResolve(param.LinkCache);
        if (quest is null || quest.Flags.HasFlag(Bethesda.Skyrim.Quest.Flag.StartGameEnabled))
        {
            return;
        }

        param.AddTopic(
            InvalidSayOnce.Format(quest));
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
    }
}
