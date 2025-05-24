using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Location;

public class NoParentLocationAnalyzer : IContextualRecordAnalyzer<ILocationGetter>
{
    public static readonly TopicDefinition NoParentLocation = MutagenTopicBuilder.FromDiscussion(
            233,
            "No Parent Location",
            Severity.Suggestion)
        .WithoutFormatting("Location has no parent location - this is likely a mistake - only top level locations should have no parent location");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoParentLocation];

    private static readonly HashSet<IFormLinkGetter<ILocationGetter>> ValidTopLevelLocations =
    [
        FormKeys.SkyrimSE.Skyrim.Location.PersistAll,
        FormKeys.SkyrimSE.Skyrim.Location.HoldingCell,
        FormKeys.SkyrimSE.Skyrim.Location.VirtualLocation,
        FormKeys.SkyrimSE.Skyrim.Location.TamrielLocation,
        FormKeys.SkyrimSE.Skyrim.Location.SovngardeLocation,
        FormKeys.SkyrimSE.Dragonborn.Location.DLC2SolstheimLocation,
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ILocationGetter> param)
    {
        var location = param.Record;

        // Ignore some well known top level locations
        if (ValidTopLevelLocations.Contains(location))
        {
            return;
        }

        // Ignore locations that are assigned on a worldspace level
        if (param.LinkCache.PriorityOrder.WinningOverrides<IWorldspaceGetter>()
            .Select(x => x.Location)
            .Any(x => x.FormKey == location.FormKey))
        {
            return;
        }

        if (location.ParentLocation.IsNull)
        {
            param.AddTopic(NoParentLocation.Format());
        }
    }

    public IEnumerable<Func<ILocationGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.ParentLocation;
    }
}
