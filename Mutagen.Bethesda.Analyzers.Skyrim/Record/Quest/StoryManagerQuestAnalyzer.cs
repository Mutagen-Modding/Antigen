using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;

public class StoryManagerQuestAnalyzer : IContextualRecordAnalyzer<IQuestGetter>
{
    public static readonly TopicDefinition StoryManagerQuestNotAssigned = MutagenTopicBuilder.FromDiscussion(
            254,
            "Story Manager Quest not assigned",
            Severity.Error)
        .WithoutFormatting("Quest with Story Manager Event not assigned to any Story Manager Quest Node");

    public IEnumerable<TopicDefinition> Topics { get; } = [StoryManagerQuestNotAssigned];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IQuestGetter> param)
    {
        var quest = param.Record;
        if (!quest.Event.HasValue) return;

        // TODO: potentially replace with reference cache

        if (param.LinkCache.PriorityOrder.WinningOverrides<IStoryManagerQuestNodeGetter>()
            .SelectMany(questNode => questNode.Quests.Select(n => n.Quest.FormKey))
            .All(questFormKey => questFormKey != quest.FormKey))
        {
            param.AddTopic(
                StoryManagerQuestNotAssigned.Format());
        }
    }

    public IEnumerable<Func<IQuestGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Event;
    }
}
