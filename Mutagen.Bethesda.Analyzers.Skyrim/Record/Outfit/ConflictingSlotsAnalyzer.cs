using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Outfit;

public class ConflictingSlotsAnalyzer : IContextualRecordAnalyzer<IOutfitGetter>
{
    public static readonly TopicDefinition<BipedObjectFlag> ConflictingSlotsTopic = MutagenTopicBuilder.FromDiscussion(
            247,
            "Conflicting Slots",
            Severity.Warning)
        .WithFormatting<BipedObjectFlag>("Outfit entries are conflicting in slot {0}");

    public IEnumerable<TopicDefinition> Topics { get; } = [ConflictingSlotsTopic];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IOutfitGetter> param)
    {
        var outfit = param.Record;

        if (outfit.Items is null) return;

        var armorsEntriesPerSlot = outfit.Items
            .SelectMany<IFormLinkGetter<IOutfitTargetGetter>,(IMajorRecordGetter Entry, BipedObjectFlag Slot, IArmorGetter Armor)>(item => item.TryResolve(param.LinkCache) switch
            {
                IArmorGetter armor => armor.GetSlots().Select(slot => ((IMajorRecordGetter) armor, slot, armor)),
                ILeveledItemGetter leveledItem => leveledItem.GetItems<IArmorGetter>(param.LinkCache).SelectMany(armor => armor.GetSlots().Select(slot => ((IMajorRecordGetter) leveledItem, slot, armor))),
                _ => []
            })
            .GroupBy(x => x.Slot)
            .ToDictionary(x => x.Key, x => x.ToList());

        foreach (var (slot, entries) in armorsEntriesPerSlot)
        {
            var separateEntriesOccupyingSlots = entries.GroupBy(x => x.Entry.FormKey).ToList();
            if (separateEntriesOccupyingSlots.Count <= 1) continue;

            param.AddTopic(
                ConflictingSlotsTopic.Format(slot),
                ("Outfit Entries", separateEntriesOccupyingSlots.ToDictionary(x => x.First().Entry.EditorID ?? x.First().Entry.FormKey.ToString(), x => x.Select(x => x.Armor).ToList())));
        }
    }

    public IEnumerable<Func<IOutfitGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Items;
    }
}
