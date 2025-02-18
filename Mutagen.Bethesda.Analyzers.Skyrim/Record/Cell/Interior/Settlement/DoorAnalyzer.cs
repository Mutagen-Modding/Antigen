using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior.Settlement;

public class DoorAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IPlacedObjectGetter> NotLocked = MutagenTopicBuilder.DevelopmentTopic(
            "Door Not Locked",
            Severity.Warning)
        .WithFormatting<IPlacedObjectGetter>("{0} is a door leading to the exterior and should be locked");

    public static readonly TopicDefinition<IPlacedObjectGetter> NoKey = MutagenTopicBuilder.DevelopmentTopic(
            "Door Has No Key",
            Severity.Suggestion)
        .WithFormatting<IPlacedObjectGetter>("{0} is missing a key");

    public static readonly TopicDefinition<IPlacedObjectGetter> NoOwner = MutagenTopicBuilder.DevelopmentTopic(
            "Door Has No Owner",
            Severity.Warning)
        .WithFormatting<IPlacedObjectGetter>("{0} is not owned");

    public static readonly TopicDefinition<IPlacedObjectGetter> ExteriorDoorLocked = MutagenTopicBuilder.DevelopmentTopic(
            "Exterior Door Is Locked",
            Severity.Warning)
        .WithFormatting<IPlacedObjectGetter>("{0} should not be locked, just the interior door");

    public IEnumerable<TopicDefinition> Topics { get; } = [NotLocked, NoKey, NoOwner, ExteriorDoorLocked];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        // Public cells don't need to be locked
        if (cell.IsPublic()) return;

        // Skip non-settlement cells
        if (!cell.IsSettlementCell(param.LinkCache)) return;

        foreach (var placedObject in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedObjectGetter>())
        {
            if (!placedObject.LeadsToExterior(param.LinkCache, out var exteriorDoor)) continue;

            if (placedObject.Lock is null)
            {
                param.AddTopic(
                    NotLocked.Format(placedObject));
            }
            else if (placedObject.Lock.Key.IsNull)
            {
                param.AddTopic(
                    NoKey.Format(placedObject));
            }

            if (placedObject.Owner.IsNull)
            {
                param.AddTopic(
                    NoOwner.Format(placedObject));
            }

            if (exteriorDoor.Lock is not null)
            {
                param.AddTopic(
                    ExteriorDoorLocked.Format(exteriorDoor));
            }
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.Location;
        yield return x => x.Owner;
        yield return x => x.Temporary;
    }
}
