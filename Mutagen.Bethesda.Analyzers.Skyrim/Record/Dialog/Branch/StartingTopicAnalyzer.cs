using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Branch;

public class StartingTopicAnalyzer(GameConstants gameConstants) : IContextualRecordAnalyzer<IDialogBranchGetter>
{
    public static readonly TopicDefinition NoStartingTopic = MutagenTopicBuilder.FromDiscussion(
            265,
            "No Starting Topic",
            Severity.Error)
        .WithoutFormatting("Branch has no starting topic");

    public static readonly TopicDefinition<Language, IDialogResponsesGetter> NoPromptOnStartingTopic = MutagenTopicBuilder.FromDiscussion(
            336,
            "No Prompt On Starting Topic",
            Severity.Error)
        .WithFormatting<Language, IDialogResponsesGetter>("Top level branch has starting topic with no prompt in {0} on topic or response {1}");

    public IEnumerable<TopicDefinition> Topics => [NoStartingTopic, NoPromptOnStartingTopic];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IDialogBranchGetter> param)
    {
        var branch = param.Record;

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

            var name = topic.Name;
            foreach (var language in gameConstants.Languages)
            {
                if (name is not null
                    && name.TryLookup(language, out var nameStr)
                    && !nameStr.IsNullOrEmpty()) continue;

                foreach (var responses in topic.Responses)
                {
                    if ((name is null && responses.Prompt is null)
                        || (responses.Prompt is not null && responses.Prompt.TryLookup(language, out var prompt) && prompt.IsNullOrEmpty()))
                    {
                        param.AddTopic(
                            NoPromptOnStartingTopic.Format(language, responses));
                    }
                }
            }
        }
    }

    public IEnumerable<Func<IDialogBranchGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.StartingTopic;
    }
}
