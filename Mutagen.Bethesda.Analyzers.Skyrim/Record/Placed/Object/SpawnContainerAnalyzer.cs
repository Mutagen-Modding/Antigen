using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class SpawnContainerAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition<IPlacedObjectGetter, IPlaceableObjectGetter> InvalidSpawnContainer = MutagenTopicBuilder.FromDiscussion(
            500,
            "Invalid Spawn Container",
            Severity.Error)
        .WithFormatting<IPlacedObjectGetter, IPlaceableObjectGetter>("Has spawn container {0} which is a {1} which is not a container");

    public static readonly TopicDefinition<IPlaceableObjectGetter> SpawnContainerOnNonItem = MutagenTopicBuilder.FromDiscussion(
            501,
            "Spawn Container On Non-Item",
            Severity.Error)
        .WithFormatting<IPlaceableObjectGetter>("Placed has a spawn container set but it is placing {0} which is not an item that can be placed in a container");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidSpawnContainer, SpawnContainerOnNonItem];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;
        if (placedObject.IsDeleted) return;

        if (placedObject.SpawnContainer.IsNull) return;

        var placeableObject = placedObject.Base.TryResolve(param.LinkCache);
        if (placeableObject is null) return;

        if (placeableObject is not IItemGetter) {
            param.AddTopic(
                SpawnContainerOnNonItem.Format(placeableObject));
            return;
        }

        var spawnContainerRef = placedObject.SpawnContainer.TryResolve(param.LinkCache);
        if (spawnContainerRef is null) return;

        var spawnContainer = spawnContainerRef.Base.TryResolve(param.LinkCache);

        if (spawnContainer is not IContainerGetter)
        {
            param.AddTopic(
                InvalidSpawnContainer.Format());
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Base;
        yield return x => x.SpawnContainer;
    }
}
