using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Branch;

public class TopLevelBlockingAnalyzer : IContextualRecordAnalyzer<IDialogBranchGetter>
{
    public static readonly TopicDefinition BothTopLevelAndBlocking = MutagenTopicBuilder.FromDiscussion(
            384,
            "Both Top Level and Blocking Branch",
            Severity.Error)
        .WithoutFormatting("Branch is both a top-level branch and a blocking branch");

    public IEnumerable<TopicDefinition> Topics => [BothTopLevelAndBlocking];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogBranchGetter> param)
    {
        var branch = param.Record;
        if (branch.IsDeleted) return;

        if (branch.Flags is null) return;

        if (branch.Flags.Value.HasFlag(DialogBranch.Flag.Blocking) && branch.Flags.Value.HasFlag(DialogBranch.Flag.TopLevel))
        {

            param.AddTopic(
                BothTopLevelAndBlocking.Format());
        }
    }

    public IEnumerable<Func<IDialogBranchGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
    }
}
