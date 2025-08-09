using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell;

public class EncounterZoneAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IFormLinkGetter<ILocationGetter>, IFormLinkNullableGetter<ILocationGetter>> EncounterZoneLocationMismatch = MutagenTopicBuilder.FromDiscussion(
            457,
            "Encounter Zone / Location Mismatch",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter<ILocationGetter>, IFormLinkNullableGetter<ILocationGetter>>("Encounter Zone assigned to cell has location {0} which is different from the cell's directly assigned location {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [EncounterZoneLocationMismatch];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;
        if (cell.IsDeleted) return;

        if (cell.EncounterZone is not {} encounterZoneLink) return;

        var encounterZone = encounterZoneLink.TryResolve(param.LinkCache);
        if (encounterZone is null) return;
        if (encounterZone.Location.IsNull) return;

        if (!cell.Location.IsNull && cell.Location.FormKey != encounterZone.Location.FormKey)
        {
            param.AddTopic(
                EncounterZoneLocationMismatch.Format(encounterZone.Location, cell.Location));
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.EncounterZone;
        yield return x => x.Location;
    }
}
