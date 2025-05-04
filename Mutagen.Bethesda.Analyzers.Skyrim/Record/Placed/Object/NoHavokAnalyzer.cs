using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class NoHavokAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    private const string DisableHavokScript = "defaultDisableHavokOnLoad";

    public static readonly TopicDefinition<string> NonHavokObjectHasFlag = MutagenTopicBuilder.DevelopmentTopic(
            "Redundant No Havok Flag on static object",
            Severity.Warning)
        .WithFormatting<string>("{0} placement is static and doesn't need the No Havok flag");

    public static readonly TopicDefinition<string> NonHavokObjectHasScript = MutagenTopicBuilder.DevelopmentTopic(
            "Redundant disable havok script on static object",
            Severity.Warning)
        .WithFormatting<string>("{0} placement is static and doesn't need the defaultDisableHavokOnLoad script");

    public IEnumerable<TopicDefinition> Topics { get; } = [NonHavokObjectHasFlag, NonHavokObjectHasScript];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        if (placedObject.IsDeleted) return;

        var baseObject = placedObject.Base.TryResolve(param.LinkCache);
        if (baseObject is null) return;

        var canMove = baseObject switch
        {
            IAmmunitionGetter => true,
            IArmorGetter => true,
            IBookGetter => true,
            IIngestibleGetter => true,
            IIngredientGetter => true,
            IKeyGetter => true,
            IMiscItemGetter => true,
            IMoveableStaticGetter => true,
            IScrollGetter => true,
            ISoulGemGetter => true,
            IWeaponGetter => true,
            _ => false
        };

        var hasFlag = ((PlacedObject.DefaultMajorFlag)placedObject.SkyrimMajorRecordFlags & PlacedObject.DefaultMajorFlag.DontHavokSettle) != 0;
        var hasScript = placedObject.HasScript(DisableHavokScript);

        if (!canMove)
        {
            if (hasFlag)
            {
                param.AddTopic(
                    NonHavokObjectHasFlag.Format(baseObject.Registration.Name));
            }

            if (hasScript)
            {
                param.AddTopic(
                    NonHavokObjectHasScript.Format(baseObject.Registration.Name));
            }
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.SkyrimMajorRecordFlags;
        yield return x => x.Base;
    }
}
