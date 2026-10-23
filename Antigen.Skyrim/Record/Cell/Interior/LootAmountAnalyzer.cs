using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Cell.Interior;

public sealed class LootAmountAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public const int LootValueThreshold = 10000;

    public static readonly TopicDefinition<long> TooMuchLootTopic = MutagenTopicBuilder.FromDiscussion(
            661,
            "Cell Has Too Much Loot",
            Severity.Suggestion)
        .WithFormatting<long>($"The average total loot value of this cell is {{0}}, which is above the recommended threshold of {LootValueThreshold}. Consider reducing the amount of loot placed in this cell to maintain game balance.");

    public IEnumerable<TopicDefinition> Topics { get; } = [TooMuchLootTopic];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        if (cell.IsExteriorCell()) return;

        const int levelToSampleAt = 10;

        var results = cell.GetAllPlaced(param.LinkCache)
            .Select<IPlacedGetter, (string? Item, long Value)>(placed =>
            {
                switch (placed) {
                    case IPlacedObjectGetter placedObject:
                        if (placedObject.LeveledItemBaseObject.IsNull)
                        {
                            if (placedObject.Base.TryResolve(param.LinkCache, out var baseObject))
                            {
                                switch (baseObject) {
                                    case IWeightValueGetter weightValue:
                                        // A non-leveled item is placed, use the gold value of the base object
                                        return (baseObject.EditorID, weightValue.Value);
                                    case IContainerGetter container:
                                        var averageGoldAmount = container.GetAverageGoldValue(param.LinkCache, levelToSampleAt);

                                        return (container.EditorID, averageGoldAmount);
                                }
                            }
                        }
                        else if (placedObject.LeveledItemBaseObject.TryResolve(param.LinkCache, out var leveledItem))
                        {
                            // A leveled list is used to determine which item is placed, use the gold value of the leveled list
                            var averageGoldAmount = leveledItem.GetAverageGoldValue(param.LinkCache, levelToSampleAt);

                            return (leveledItem.EditorID, averageGoldAmount);
                        }
                        break;
                    case IPlacedNpcGetter placedNpc:
                        if (placedNpc.Base.TryResolve(param.LinkCache, out var npc))
                        {
                            var averageGoldAmount = npc.GetAverageGoldValue(param.LinkCache, levelToSampleAt);

                            return (npc.EditorID, averageGoldAmount);
                        }
                        break;
                }

                return (null, 0);
            })
            .Where(x => x.Item is not null || x.Value > 0)
            .ToArray();

        var goldSum = results.Sum(x => x.Value);

        if (goldSum > LootValueThreshold)
        {
            param.AddTopic(TooMuchLootTopic.Format(goldSum),
            ("Most valuable items", results
                .OrderByDescending(x => x.Value)
                .Take(20)
                .ToArray()));
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.Temporary;
        yield return x => x.Persistent;
    }
}
