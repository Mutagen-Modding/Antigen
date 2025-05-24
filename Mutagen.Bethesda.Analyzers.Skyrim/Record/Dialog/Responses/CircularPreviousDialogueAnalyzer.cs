using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class CircularPreviousDialogueAnalyzer : IContextualRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition<IDialogResponsesGetter, IDialogResponsesGetter> CircularPreviousDialogue = MutagenTopicBuilder.FromDiscussion(
            266,
            "Circular Previous Dialog",
            Severity.Warning)
        .WithFormatting<IDialogResponsesGetter, IDialogResponsesGetter>("Dialogue has a circular reference in the previous dialog between {0} and {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [CircularPreviousDialogue];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;
        var dialogCache = new HashSet<FormKey>();

        var previousDialog = dialogResponses.PreviousDialog.TryResolve(param.LinkCache);
        while (previousDialog?.PreviousDialog is not null)
        {
            if (!dialogCache.Add(previousDialog.FormKey))
            {
                param.AddTopic(
                    CircularPreviousDialogue.Format(dialogResponses, previousDialog));
                return;
            }

            previousDialog = previousDialog.PreviousDialog.TryResolve(param.LinkCache);
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.PreviousDialog;
    }
}
