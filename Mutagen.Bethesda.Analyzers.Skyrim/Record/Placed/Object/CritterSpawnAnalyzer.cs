using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class CritterSpawnAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    private const string CritterSpawnScriptName = "CritterSpawn";
    private const string CritterSpawnScriptName01 = "CritterSpawn01";
    private const string CritterSpawnScriptName02 = "CritterSpawn02";
    private const string CritterSpawnScriptName03 = "CritterSpawn03";

    private const string CritterSpawnPropertyName = "CritterTypes";
    private const string DistancePropertyName = "fLeashLength";

    private readonly Dictionary<string, string> _critterLandingPropertyPerScriptName = new()
    {
        { "CritterBird", "PerchTypeList" },
        { "CritterMoth", "PlantTypes" },
        { "Firefly", "PlantTypes" },
    };

    public static readonly TopicDefinition<IActivatorGetter, IFormListGetter> CritterSpawnWithoutLandingOpportunity = MutagenTopicBuilder.FromDiscussion(
            999,
            "Critter Spawn without Landing Opportunity",
            Severity.Error)
        .WithFormatting<IActivatorGetter, IFormListGetter>($"Placed Object has a {CritterSpawnScriptName} script but the critter {{0}} that it spawns has no nearby landing opportunities as listed in {{1}}");

    public IEnumerable<TopicDefinition> Topics { get; } =
    [
        CritterSpawnWithoutLandingOpportunity,
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var spawnRef = param.Record;
        if (spawnRef.VirtualMachineAdapter is null) return;

        foreach (var (critterTypesProperty, distanceProperty) in GetCritterProperties())
        {
            var critterTypesList = critterTypesProperty.Object.TryResolve<IFormListGetter>(param.LinkCache);
            if (critterTypesList is null) continue;

            foreach (var critterItem in critterTypesList.Items)
            {
                if (!critterItem.TryResolve<IActivatorGetter>(param.LinkCache, out var critter)) continue;

                foreach (var (critterScriptName, critterPropertyName) in _critterLandingPropertyPerScriptName)
                {
                    var critterScript = critter.GetScript(critterScriptName);
                    var landingProperty = critterScript?.GetProperty<IScriptObjectPropertyGetter>(critterPropertyName);
                    if (landingProperty is null) continue;

                    landingProperty.Object.TryResolve<IFormListGetter>(param.LinkCache, out var landingList);
                    if (landingList is null || !landingList.Items.Any()) continue;

                    var nearbyObjectsOfType = spawnRef.GetNearbyObjects(
                        placeable => landingList.Items.Contains(placeable),
                        distanceProperty.Data,
                        param.LinkCache);

                    if (!nearbyObjectsOfType.Any())
                    {
                        param.AddTopic(
                            CritterSpawnWithoutLandingOpportunity.Format(critter, landingList));
                    }
                }
            }
        }

        IEnumerable<(IScriptObjectPropertyGetter CritterTypes, IScriptFloatPropertyGetter Distance)> GetCritterProperties()
        {
            var (_, critterListProperty) = spawnRef.GetScriptPropertyFromSelfOrBase<IScriptObjectPropertyGetter>(param.LinkCache, CritterSpawnScriptName, CritterSpawnPropertyName);
            if (critterListProperty is not null)
            {
                var (_, distanceProperty) = spawnRef.GetScriptPropertyFromSelfOrBase<IScriptFloatPropertyGetter>(param.LinkCache, CritterSpawnScriptName, DistancePropertyName);
                if (distanceProperty is not null) yield return (critterListProperty, distanceProperty);
            }

            (_, critterListProperty) = spawnRef.GetScriptPropertyFromSelfOrBase<IScriptObjectPropertyGetter>(param.LinkCache, CritterSpawnScriptName01, CritterSpawnPropertyName);
            if (critterListProperty is not null)
            {
                var (_, distanceProperty) = spawnRef.GetScriptPropertyFromSelfOrBase<IScriptFloatPropertyGetter>(param.LinkCache, CritterSpawnScriptName01, DistancePropertyName);
                if (distanceProperty is not null) yield return (critterListProperty, distanceProperty);
            }

            (_, critterListProperty) = spawnRef.GetScriptPropertyFromSelfOrBase<IScriptObjectPropertyGetter>(param.LinkCache, CritterSpawnScriptName02, CritterSpawnPropertyName);
            if (critterListProperty is not null)
            {
                var (_, distanceProperty) = spawnRef.GetScriptPropertyFromSelfOrBase<IScriptFloatPropertyGetter>(param.LinkCache, CritterSpawnScriptName02, DistancePropertyName);
                if (distanceProperty is not null) yield return (critterListProperty, distanceProperty);
            }

            (_, critterListProperty) = spawnRef.GetScriptPropertyFromSelfOrBase<IScriptObjectPropertyGetter>(param.LinkCache, CritterSpawnScriptName03, CritterSpawnPropertyName);
            if (critterListProperty is not null)
            {
                var (_, distanceProperty) = spawnRef.GetScriptPropertyFromSelfOrBase<IScriptFloatPropertyGetter>(param.LinkCache, CritterSpawnScriptName03, DistancePropertyName);
                if (distanceProperty is not null) yield return (critterListProperty, distanceProperty);
            }
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VirtualMachineAdapter;
    }
}
