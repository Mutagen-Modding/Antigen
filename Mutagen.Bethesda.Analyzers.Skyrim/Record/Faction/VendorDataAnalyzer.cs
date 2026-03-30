using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Faction;

public class VendorDataAnalyzer : IIsolatedRecordAnalyzer<IFactionGetter>
{
    // Location data -
    public static readonly TopicDefinition LocationDataWithoutVendor = MutagenTopicBuilder.DevelopmentTopic( // TODO: Proper ID
        "Vendor location data without vendor flag",
        Severity.Warning
        ).WithoutFormatting("Faction has vendor location data, but no vendor flag");

    public IEnumerable<TopicDefinition> Topics => [LocationDataWithoutVendor];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IFactionGetter> param)
    {
        var faction = param.Record;
        if (faction.IsVendor())
            return;

        if (faction.VendorLocation != null)
        {
            param.AddTopic(LocationDataWithoutVendor.Format());
        }
    }

    public IEnumerable<Func<IFactionGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.VendorLocation;
    }
}
