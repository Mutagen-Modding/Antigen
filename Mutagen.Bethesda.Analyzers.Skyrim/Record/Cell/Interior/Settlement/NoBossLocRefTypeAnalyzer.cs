using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior.Settlement;

public class NoBossLocRefTypeAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IPlacedNpcGetter> NoBossLocRefType = MutagenTopicBuilder.DevelopmentTopic(
            "No Boss Location Reference Type",
            Severity.Suggestion)
        .WithFormatting<IPlacedNpcGetter>("{0} is a unique NPC and should be marked as a boss");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoBossLocRefType];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        // Skip non-settlement cells
        if (!cell.IsSettlementCell(param.LinkCache)) return;

        foreach (var placedNpc in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedNpcGetter>())
        {
            // Skip NPCs that are deleted or initially disabled
            if (placedNpc.IsDeleted) continue;
            if (placedNpc.MajorFlags.HasFlag(PlacedNpc.MajorFlag.InitiallyDisabled)) continue;

            // Skip NPCs that are already marked as bosses
            if (placedNpc.HasLocationRefType(FormKeys.SkyrimSE.Skyrim.LocationReferenceType.Boss)) continue;

            // Only check unique NPCs
            var npc = placedNpc.Base.TryResolve(param.LinkCache);
            if (npc is null || !npc.IsUnique()) continue;

            // Skip children
            if (npc.Race.TryResolve(param.LinkCache, out var race) && race.IsChildRace()) continue;

            param.AddTopic(
                NoBossLocRefType.Format(placedNpc));
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.Location;
        yield return x => x.Temporary;
        yield return x => x.Persistent;
    }
}
