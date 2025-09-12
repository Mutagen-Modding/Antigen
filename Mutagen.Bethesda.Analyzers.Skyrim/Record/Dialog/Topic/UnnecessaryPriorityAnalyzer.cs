using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Topic;

public class UnnecessaryPriorityAnalyzer : IContextualRecordAnalyzer<IDialogTopicGetter>
{
    private const int DefaultDialogTopicPriority = 50;

    public static readonly TopicDefinition<float> UnnecessaryPriority = MutagenTopicBuilder.FromDiscussion(
            278,
            "Unnecessary Priority",
            Severity.Suggestion)
        .WithFormatting<float>("Topic has a custom priority of {0} but is not a starting topic, this does not have any effect on the dialog order");

    public IEnumerable<TopicDefinition> Topics { get; } = [UnnecessaryPriority];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogTopicGetter> param)
    {
        var dialogTopic = param.Record;

        // Only check custom topics
        if (dialogTopic.Subtype != DialogTopic.SubtypeEnum.Custom) return;

        // Skip if this topic is a starting topic
        var branch = dialogTopic.Branch.TryResolve(param.LinkCache);
        if (branch is not null && branch.StartingTopic.FormKey == dialogTopic.FormKey) return;

        if ((dialogTopic.Priority - DefaultDialogTopicPriority).EqualsWithin(0))
        {
            return;
        }

        param.AddTopic(UnnecessaryPriority.Format(dialogTopic.Priority));
    }

    public IEnumerable<Func<IDialogTopicGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Subtype;
        yield return x => x.Branch;
        yield return x => x.Priority;
    }
}
