using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;

public class ReservedPlayerAliasAnalyzer : IIsolatedRecordAnalyzer<IQuestGetter>
{
    public static readonly TopicDefinition<string> ReservedPlayerAlias = MutagenTopicBuilder.FromDiscussion(
            420,
            "Reserved Player Alias",
            Severity.Error)
        .WithFormatting<string>("Quest has alias {0} which is filled by the player that reserves the player reference");

    public IEnumerable<TopicDefinition> Topics { get; } = [ReservedPlayerAlias];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IQuestGetter> param)
    {
        var quest = param.Record;

        foreach (var alias in quest.Aliases)
        {
            if (!alias.Flags.HasValue || !alias.Flags.Value.HasFlag(QuestAlias.Flag.ReservesLocationOrReference)) continue;

            if (alias.ForcedReference.FormKey == FormKeys.SkyrimSE.Skyrim.PlayerRef.FormKey
                || alias.UniqueActor.FormKey == FormKeys.SkyrimSE.Skyrim.Npc.Player.FormKey)
            {
                param.AddTopic(
                    ReservedPlayerAlias.Format(alias.Name ?? alias.ID.ToString()));
            }
        }
    }

    public IEnumerable<Func<IQuestGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Aliases;
    }
}
