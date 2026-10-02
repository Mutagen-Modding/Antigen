using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Quest;

public class EventConditionsAnalyzer : IIsolatedRecordAnalyzer<IQuestGetter>
{
    public static readonly TopicDefinition<Condition.Function> QuestAliasEventCondition = MutagenTopicBuilder.FromDiscussion(
            421,
            "Event Condition runs on Quest Alias",
            Severity.Error)
        .WithFormatting<Condition.Function>("Quest has an event condition with function {0} that runs on a quest alias");

    public IEnumerable<TopicDefinition> Topics { get; } = [QuestAliasEventCondition];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IQuestGetter> param)
    {
        var quest = param.Record;

        foreach (var condition in quest.EventConditions)
        {
            if (condition.Data.RunOnType == Condition.RunOnType.QuestAlias)
            {
                param.AddTopic(
                    QuestAliasEventCondition.Format(condition.Data.Function));
            }
        }
    }

    public IEnumerable<Func<IQuestGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Aliases;
    }
}
