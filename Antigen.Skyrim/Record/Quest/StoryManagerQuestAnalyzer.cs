using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Quest;

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

        if (!param.ResolveCache<ILinkUsageCache>().GetUsagesOf<IStoryManagerQuestNodeGetter>(quest).UsageLinks
            .Select(n => n.Resolve(param.LinkCache))
            .Any(n => n.Quests.Any(q => q.Quest.Equals(quest))))
        {
            param.AddTopic(StoryManagerQuestNotAssigned.Format());
        }
    }

    public IEnumerable<Func<IQuestGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Event;
    }
}
