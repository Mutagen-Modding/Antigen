using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;

public class ActorDialogueQuestAnalyzer : IIsolatedRecordAnalyzer<IQuestGetter>
{
    public static readonly TopicDefinition NoAliases = MutagenTopicBuilder.FromDiscussion(
            251,
            "No Aliases",
            Severity.Error)
        .WithoutFormatting("Quest has no aliases");

    public static readonly TopicDefinition OddNumberOfAliases = MutagenTopicBuilder.FromDiscussion(
            323,
            "Odd Number Of Aliases",
            Severity.Warning)
        .WithoutFormatting("Quest has an odd number of aliases");

    public static readonly TopicDefinition<string?> AliasWithoutFindMatchingRefFromEvent = MutagenTopicBuilder.FromDiscussion(
            324,
            "Alias without Find Matching Reference From Event",
            Severity.Error)
        .WithFormatting<string?>("Alias {0} is not filled with Find Matching Reference From Event fill type");

    public static readonly TopicDefinition<string?, int, int> AliasWithoutSameNumberOfConditionsAsNpcs = MutagenTopicBuilder.FromDiscussion(
            325,
            "Alias without same number of conditions as npcs",
            Severity.Error)
        .WithFormatting<string?, int, int>("Alias {0} has {1} conditions, which doesn't match the {2} npcs in the scene");

    public static readonly TopicDefinition<string?> AliasWithoutGetIsIDCondition = MutagenTopicBuilder.FromDiscussion(
            326,
            "Alias without GetIsID condition",
            Severity.Error)
        .WithFormatting<string?>("Alias {0} uses conditions which are not GetIsID");

    public static readonly TopicDefinition<string?, int, int> AliasWithoutSameNumberOfGetIsIDConditionsAsNpcs = MutagenTopicBuilder.FromDiscussion(
            327,
            "Alias without same number of GetIsID conditions as npcs",
            Severity.Error)
        .WithFormatting<string?, int, int>("Alias {0} has {1} GetIsID conditions, which doesn't match the {2} npcs in the scene");

    public static readonly TopicDefinition<string?, int> AliasWithoutGetDistanceCondition = MutagenTopicBuilder.FromDiscussion(
            328,
            "Alias without GetDistance condition",
            Severity.Error)
        .WithFormatting<string?, int>("Alias {0} has {1} GetDistance conditions, not 1");

    public static readonly TopicDefinition<string?> AliasWithoutUniqueActor = MutagenTopicBuilder.FromDiscussion(
            329,
            "Alias without Unique Actor",
            Severity.Error)
        .WithFormatting<string?>("Alias {0} is not filled with Unique Actor fill type");

    public static readonly TopicDefinition<string?> AliasWithoutAllowReuseInQuestFlag = MutagenTopicBuilder.FromDiscussion(
            330,
            "Alias without Allow Reuse In Quest flag",
            Severity.Error)
        .WithFormatting<string?>("Alias {0} doesn't have Allow Reuse In Quest flag");

    public IEnumerable<TopicDefinition> Topics { get; } =
    [
        NoAliases,
        OddNumberOfAliases,
        AliasWithoutFindMatchingRefFromEvent,
        AliasWithoutSameNumberOfConditionsAsNpcs,
        AliasWithoutGetIsIDCondition,
        AliasWithoutSameNumberOfGetIsIDConditionsAsNpcs,
        AliasWithoutGetDistanceCondition,
        AliasWithoutUniqueActor,
        AliasWithoutAllowReuseInQuestFlag
    ];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IQuestGetter> param)
    {
        var quest = param.Record;
        if (!quest.Event.HasValue || quest.Event.Value != "ADIA") return;

        if (quest.Aliases.Count == 0)
        {
            param.AddTopic(
                NoAliases.Format());
            return;
        }

        if (quest.Aliases.Count % 2 != 0)
        {
            param.AddTopic(
                OddNumberOfAliases.Format());

            return;
        }

        var firstAliasHalf = quest.Aliases.Take(quest.Aliases.Count / 2).ToList();
        var secondAliasHalf = quest.Aliases.Skip(quest.Aliases.Count / 2).ToList();

        var startsWithEventAlias = quest.Aliases[0].FindMatchingRefFromEvent is not null;
        var eventAliases = startsWithEventAlias ? firstAliasHalf : secondAliasHalf;
        for (var i = 0; i < eventAliases.Count; i++)
        {
            var eventAlias = eventAliases[i];
            if (i < 2)
            {
                if (eventAlias.FindMatchingRefFromEvent is null)
                {
                    param.AddTopic(
                        AliasWithoutFindMatchingRefFromEvent.Format(eventAlias.Name));
                }

                if (eventAlias.Conditions.Count != eventAliases.Count)
                {
                    param.AddTopic(
                        AliasWithoutSameNumberOfConditionsAsNpcs.Format(eventAlias.Name, eventAlias.Conditions.Count, eventAliases.Count));
                }

                var conditions = eventAlias.Conditions.Where(condition => condition.Data is not IGetIsIDConditionDataGetter).ToList();
                if (conditions.Count > 0)
                {
                    param.AddTopic(
                        AliasWithoutGetIsIDCondition.Format(eventAlias.Name),
                        ("Functions", conditions.Select(x => x.Data.Function).Distinct()));
                }
            }
            else
            {
                if (eventAlias.Conditions.Count != eventAliases.Count + 1)
                {
                    param.AddTopic(
                        AliasWithoutSameNumberOfConditionsAsNpcs.Format(eventAlias.Name, eventAlias.Conditions.Count, eventAliases.Count + 1));
                }

                if (eventAlias.Conditions.Count(condition => condition.Data is IGetIsIDConditionDataGetter) != eventAliases.Count)
                {
                    param.AddTopic(
                        AliasWithoutSameNumberOfGetIsIDConditionsAsNpcs.Format(eventAlias.Name, eventAlias.Conditions.Count, eventAliases.Count));
                }

                var count = eventAlias.Conditions.Count(condition => condition.Data is IGetDistanceConditionDataGetter);
                if (count != 1)
                {
                    param.AddTopic(
                        AliasWithoutGetDistanceCondition.Format(eventAlias.Name, count));
                }
            }
        }

        var npcAliases = startsWithEventAlias ? secondAliasHalf : firstAliasHalf;
        foreach (var npcAlias in npcAliases)
        {
            if (npcAlias.UniqueActor.IsNull)
            {
                param.AddTopic(
                    AliasWithoutUniqueActor.Format(npcAlias.Name));
            }
        }

        foreach (var alias in secondAliasHalf)
        {
            if (alias.Flags is null || !alias.Flags.Value.HasFlag(QuestAlias.Flag.AllowReuseInQuest))
            {
                param.AddTopic(
                    AliasWithoutAllowReuseInQuestFlag.Format(alias.Name));
            }
        }
    }
    public IEnumerable<Func<IQuestGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Aliases;
    }
}
