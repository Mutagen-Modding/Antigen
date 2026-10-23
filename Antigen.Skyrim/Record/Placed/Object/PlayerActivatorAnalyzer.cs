using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Placed.Object;

public sealed class PlayerActivatorAnalyzer: IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public const uint L_NONCOLLIDABLE = 15; // L_NONCOLLIDABLE, the player activator collision layer

    public static readonly TopicDefinition InvalidRotation = MutagenTopicBuilder.FromDiscussion(
            652,
            "Player Activator with Invalid Rotation",
            Severity.Error)
        .WithoutFormatting("Player activator is a box without a rotation of 0 in all axes and will not be visible in the world");

    public static readonly TopicDefinition MissingName = MutagenTopicBuilder.FromDiscussion(
            653,
            "Player Activator with Missing Name",
            Severity.Error)
        .WithoutFormatting("Player activator has no name set and will not be visible in the world");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidRotation];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        if (!placedObject.CollisionLayer.HasValue) return;

        if (placedObject.CollisionLayer.Value != L_NONCOLLIDABLE) return;
        if (placedObject.Primitive is null) return;
        if (placedObject.Primitive.Type == PlacedPrimitive.TypeEnum.None) return;

        if (!placedObject.Base.TryResolve<IActivatorGetter>(param.LinkCache, out var activator)) return;

        if (string.IsNullOrEmpty(activator.Name?.String))
        {
            param.AddTopic(
                MissingName.Format());
        }

        if (placedObject.Primitive.Type == PlacedPrimitive.TypeEnum.Box
         && placedObject.Placement is { Rotation: { X: 0, Y: 0, Z: 0 } })
        {
            param.AddTopic(
                InvalidRotation.Format());
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Base;
        yield return x => x.CollisionLayer;
        yield return x => x.Primitive;
    }
}
