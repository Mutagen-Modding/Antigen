using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Responses;

public class InvisibleContinueAnalyzer : IIsolatedRecordAnalyzer<IDialogResponsesGetter>
{
    public static readonly TopicDefinition InvisibleContinueWithoutLinkTo = MutagenTopicBuilder.FromDiscussion(
        565,
        "Invisible Continue Without Link To",
        Severity.Warning)
    .WithoutFormatting("Dialogue has invisible continue flag without link to topic");

    public IEnumerable<TopicDefinition> Topics => [InvisibleContinueWithoutLinkTo];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogResponsesGetter> param)
    {
        bool invisibleContinue = param.Record.Flags?.Flags.HasFlag(DialogResponses.Flag.InvisibleContinue) ?? false;
        if (invisibleContinue && param.Record.LinkTo.Count == 0)
        {
            param.AddTopic(InvisibleContinueWithoutLinkTo.Format());
        }
    }

    public IEnumerable<Func<IDialogResponsesGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.LinkTo;
    }
}
