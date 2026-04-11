using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;

public class DialogueAliasAnalyzer : IContextualRecordAnalyzer<IQuestGetter>
{
    public static readonly TopicDefinition<string?> InvalidDialogueAlias = MutagenTopicBuilder.FromDiscussion(
            252,
            "Invalid Dialogue Alias",
            Severity.Warning)
        .WithFormatting<string?>("Alias {0} that is forced to none, has no additional dialogue formlist/npc and has dialogue conditioned to it - no lines available for VA export");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidDialogueAlias];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IQuestGetter> param)
    {
        var quest = param.Record;
        var relevantAliases = quest.Aliases
            .Where(alias => alias.IsForcedNone(quest))
            .ToList();

        if (relevantAliases.Count == 0) return;

        var referencedDialogue = new Dictionary<IQuestAliasGetter, List<IDialogResponsesGetter>>();

        // TODO: potentially replace with reference cache
        foreach (var topic in param.LinkCache.PriorityOrder.WinningOverrides<IDialogTopicGetter>())
        {
            if (quest.FormKey != topic.Quest.FormKey) continue;

            foreach (var response in topic.Responses)
            {
                if (response.Speaker.IsNull) continue;

                foreach (var condition in response.Conditions)
                {
                    if (condition.Data is not IGetIsAliasRefConditionDataGetter { ReferenceAliasIndex: var aliasIndex }) continue;
                    if (condition is IConditionFloatGetter { ComparisonValue: 0 }) continue;

                    var alias = relevantAliases.Find(x => x.ID == aliasIndex);
                    if (alias is null) continue;

                    referencedDialogue.GetOrAdd(alias).Add(response);
                }
            }
        }

        foreach (var (alias, dialogue) in referencedDialogue)
        {
            if (dialogue.Count == 0) continue;

            param.AddTopic(
                InvalidDialogueAlias.Format(alias.Name),
                ("Dialogue", dialogue));
        }
    }

    public IEnumerable<Func<IQuestGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Aliases;
    }
}
