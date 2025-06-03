using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class UnnecessaryOwnershipAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition<IPlaceableObjectGetter> UnnecessaryOwnership = MutagenTopicBuilder.FromDiscussion(
            381,
            "Unnecessary  Ownership",
            Severity.Suggestion)
        .WithFormatting<IPlaceableObjectGetter>("Placed record has an owner but it has no effect on {0}");

    public IEnumerable<TopicDefinition> Topics { get; } = [UnnecessaryOwnership];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        if (placedObject.Owner.IsNull) return;

        var placeableObject = placedObject.Base.TryResolve(param.LinkCache);
        if (placeableObject is null) return;

        switch (placeableObject)
        {
            case IAcousticSpaceGetter:
            case IAddonNodeGetter:
            case IAlchemicalApparatusGetter:
            case IArtObjectGetter:
            case ILightGetter:
            case IMoveableStaticGetter:
            case ISoundMarkerGetter:
            case ISpellGetter:
            case IStaticGetter:
            case ITalkingActivatorGetter:
            case ITextureSetGetter:
                param.AddTopic(
                    UnnecessaryOwnership.Format(placeableObject));
                break;
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Base;
        yield return x => x.Owner;
    }
}
