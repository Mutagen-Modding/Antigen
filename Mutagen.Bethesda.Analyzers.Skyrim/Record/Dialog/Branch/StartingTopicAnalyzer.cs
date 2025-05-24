using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Branch;

public class StartingTopicAnalyzer : IContextualRecordAnalyzer<IDialogBranchGetter>
{
    public static readonly TopicDefinition NoStartingTopic = MutagenTopicBuilder.FromDiscussion(
            265,
            "No Starting Topic",
            Severity.Error)
        .WithoutFormatting("Branch has no starting topic");

    public static readonly TopicDefinition<IDialogResponsesGetter> NoPromptOnStartingTopic = MutagenTopicBuilder.FromDiscussion(
            336,
            "No Prompt On Starting Topic",
            Severity.Error)
        .WithFormatting<IDialogResponsesGetter>("Top level branch has starting topic with no prompt on topic or response {0}");

    public IEnumerable<TopicDefinition> Topics => [NoStartingTopic, NoPromptOnStartingTopic];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogBranchGetter> param)
    {
        var branch = param.Record;
        if (branch.IsDeleted) return;

        var startingTopic = branch.StartingTopic;
        if (startingTopic.IsNull)
        {
            param.AddTopic(
                NoStartingTopic.Format());
        }

        if (branch.Flags is not null && branch.Flags.Value.HasFlag(DialogBranch.Flag.TopLevel))
        {
            var topic = startingTopic.TryResolve(param.LinkCache);
            if (topic is null) return;

            // Rumors are not required to have a prompt - it will default to a prompt set by a game setting
            if (topic.SubtypeName == "RUMO") return;

            if (topic.Name is not null && !topic.Name.String.IsNullOrEmpty()) return;

            foreach (var responses in topic.Responses) {
                if (responses.Prompt?.String is null)
                {
                    param.AddTopic(
                        NoPromptOnStartingTopic.Format(responses));
                }
            }
        }
    }

    public IEnumerable<Func<IDialogBranchGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.StartingTopic;
    }
}
