using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class InventoryOwnershipAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<IItemGetter?> InventoryItemWithOwner = MutagenTopicBuilder.FromDiscussion(
            568,
            "Npc is owner of Item in Inventory",
            Severity.Suggestion)
        .WithFormatting<IItemGetter?>("Npc is owner of Item {0} in own Inventory");

    public IEnumerable<TopicDefinition> Topics { get; } = [InventoryItemWithOwner];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (npc.Items is null) return;

        foreach (var entry in npc.Items)
        {
            if (entry.Data is null) return;

            using var enumerator = entry.Data.Owner.EnumerateFormLinks().GetEnumerator();
            do
            {
                if (enumerator.Current.FormKeyNullable == npc.FormKey)
                {
                    param.AddTopic(InventoryItemWithOwner.Format(entry.Item.Item.TryResolve(param.LinkCache)));
                }
            } while (enumerator.MoveNext());
        }
    }
    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Items;
    }
}
