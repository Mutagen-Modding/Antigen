using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class ScaleAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition<string, float> ScaleTooSmall = MutagenTopicBuilder.DevelopmentTopic(
            "Scale Too Small",
            Severity.Warning)
        .WithFormatting<string, float>("A {0} placement with scale {1} is too small");

    public static readonly TopicDefinition<string, float> ScaleTooLarge = MutagenTopicBuilder.DevelopmentTopic(
            "Scale Too Large",
            Severity.Warning)
        .WithFormatting<string, float>("A {0} placement with scale {1} is too large");

    public IEnumerable<TopicDefinition> Topics { get; } = [ScaleTooSmall, ScaleTooLarge];

    public static readonly HashSet<FormKey> AllowedScaledObjects =
    [
        FormKeys.SkyrimSE.Skyrim.Static.BlackPlane01.FormKey,
        FormKeys.SkyrimSE.Skyrim.Door.AutoLoadDoor01.FormKey,
        FormKeys.SkyrimSE.Skyrim.Door.AutoLoadDoorMinUse01.FormKey,
        FormKeys.SkyrimSE.Skyrim.Door.AutoLoadDoorHiddenMinUse01.FormKey,
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        if (placedObject.IsDeleted) return;

        var scaleNullable = placedObject.Scale;
        if (scaleNullable is null) return;

        var scale = scaleNullable.Value;

        // Scale 1 is always allowed
        if (Math.Abs(1 - scale) < float.Epsilon) return;

        // Allowed objects
        if (AllowedScaledObjects.Contains(placedObject.Base.FormKey)) return;

        var baseObject = placedObject.Base.TryResolve(param.LinkCache);
        if (baseObject is null) return;

        var baseObjectEditorID = baseObject.EditorID;
        if (baseObjectEditorID is null) return;

        // Allowed editor ids
        if (baseObjectEditorID.StartsWith("dwe", StringComparison.OrdinalIgnoreCase)) return;
        if (baseObjectEditorID.Contains("mine", StringComparison.OrdinalIgnoreCase)) return;
        if (baseObjectEditorID.Contains("cave", StringComparison.OrdinalIgnoreCase)) return;
        if (baseObjectEditorID.Contains("mountain", StringComparison.OrdinalIgnoreCase)) return;
        if (baseObjectEditorID.Contains("rock", StringComparison.OrdinalIgnoreCase)) return;
        if (baseObjectEditorID.Contains("water", StringComparison.OrdinalIgnoreCase)) return;
        if (baseObjectEditorID.Contains("fx", StringComparison.OrdinalIgnoreCase)) return;
        if (baseObjectEditorID.Contains("web", StringComparison.OrdinalIgnoreCase)) return;

        // Specific type filter
        switch (baseObject)
        {
            case IMiscItemGetter:
                // Misc items should never be scaled
                // They can be picked up and dropped which resets the scale to 1
                // A value other than 1 would be lost and create inconsistencies
                break;
            case IActivatorGetter:
            case IContainerGetter:
            case IDoorGetter:
            case IFurnitureGetter:
            case IIngestibleGetter:
                if (scale is >= 0.5f and <= 1.5f) return;

                break;
            case IFloraGetter:
                if (scale is >= 0.1f and <= 3f) return;

                break;
            case IMoveableStaticGetter:
                if (scale is >= 0.2f and <= 3f) return;

                break;
            case IStaticGetter:
                if (scale is >= 0.2f and <= 2.5f) return;

                break;
            case ITreeGetter:
                if (scale is >= 0.1f and <= 3f) return;

                break;
            // Ignored types - never report
            case ILightGetter:
            case IIdleMarkerGetter:
            case ITextureSetGetter:
            case ISoundMarkerGetter:
            case ISpellGetter:
                return;
        }

        param.AddTopic(
            (scale < 1 ? ScaleTooSmall : ScaleTooLarge).Format(baseObject.Registration.Name, scale));
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Scale;
        yield return x => x.Base;
    }
}
