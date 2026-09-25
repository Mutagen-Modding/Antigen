using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Branch;

public class MissingFieldsAnalyzer : IIsolatedRecordAnalyzer<IDialogBranchGetter>
{
    public static readonly TopicDefinition NoQuest = MutagenTopicBuilder.FromDiscussion(
            264,
            "No Quest",
            Severity.Error)
        .WithoutFormatting("Branch has no quest, it will not be available in game");

    public static readonly TopicDefinition NoStartingTopic = MutagenTopicBuilder.FromDiscussion(
            498,
            "No Starting Topic",
            Severity.Warning)
        .WithoutFormatting("Branch has no starting topic, nothing will not be available in game");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoQuest, NoStartingTopic];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogBranchGetter> param)
    {
        var dialogTopic = param.Record;

        if (dialogTopic.Quest.IsNull)
        {
            param.AddTopic(
                NoQuest.Format());
        }

        if (dialogTopic.StartingTopic.IsNull)
        {
            param.AddTopic(
                NoStartingTopic.Format());
        }
    }

    public IEnumerable<Func<IDialogBranchGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Quest;
        yield return x => x.StartingTopic;
    }
}
