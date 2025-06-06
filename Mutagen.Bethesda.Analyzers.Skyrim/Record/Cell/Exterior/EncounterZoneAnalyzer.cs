using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Exterior;

public class EncounterZoneAnalyzer : IIsolatedRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IFormLinkNullableGetter<IEncounterZoneGetter>> HasEncounterZone = MutagenTopicBuilder.FromDiscussion(
            393,
            "Exterior Cell Has Encounter Zone",
            Severity.Warning)
        .WithFormatting<IFormLinkNullableGetter<IEncounterZoneGetter>>("Cell is exterior cell with an encounter zone: {0}");

    public IEnumerable<TopicDefinition> Topics => [HasEncounterZone];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;

        if (cell.IsDeleted) return;

        if (!cell.IsExteriorCell()) return;

        if (!cell.EncounterZone.IsNull)
        {
            param.AddTopic(
                HasEncounterZone.Format(cell.EncounterZone));
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.EncounterZone;
    }
}
