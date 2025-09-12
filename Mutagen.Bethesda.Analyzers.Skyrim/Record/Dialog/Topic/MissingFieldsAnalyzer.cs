using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Topic;

public class MissingFieldsAnalyzer : IIsolatedRecordAnalyzer<IDialogTopicGetter>
{
    public static readonly TopicDefinition NoBranch = MutagenTopicBuilder.FromDiscussion(
            276,
            "No Branch",
            Severity.Error)
        .WithoutFormatting("Topic has no branch, it will not be available in game");

    public static readonly TopicDefinition NoQuest = MutagenTopicBuilder.FromDiscussion(
            342,
            "No Quest",
            Severity.Error)
        .WithoutFormatting("Topic has no quest, it will not be available in game");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoBranch, NoQuest];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogTopicGetter> param)
    {
        var dialogTopic = param.Record;

        if (dialogTopic.Subtype is DialogTopic.SubtypeEnum.Rumors or DialogTopic.SubtypeEnum.ForceGreet or DialogTopic.SubtypeEnum.Custom
            && dialogTopic.Branch.IsNull)
        {
            param.AddTopic(
                NoBranch.Format());
        }


        if (dialogTopic.Quest.IsNull)
        {
            param.AddTopic(
                NoQuest.Format());
        }
    }
    public IEnumerable<Func<IDialogTopicGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Subtype;
        yield return x => x.Branch;
        yield return x => x.Quest;
    }
}
