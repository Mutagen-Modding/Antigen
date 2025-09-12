using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class LinkInGoodbyeAnalyzer : IContextualRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition LinkInGoodbye = MutagenTopicBuilder.FromDiscussion(
            453,
            "Goodbye with Link",
            Severity.Warning)
        .WithoutFormatting("Dialog responses is marked as Goodbye, but has a link to another dialog response");

    public IEnumerable<TopicDefinition> Topics { get; } = [LinkInGoodbye];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        var dialogResponses = param.Record;

        if (dialogResponses.LinkTo.Count == 0) return;
        if (dialogResponses.Flags is null) return;

        if (dialogResponses.Flags.Flags.HasFlag(DialogResponses.Flag.Goodbye))
        {
            param.AddTopic(
                LinkInGoodbye.Format());
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.LinkTo;
    }
}
