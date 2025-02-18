using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class CarryPackageAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition CarryPackageWithoutScript = MutagenTopicBuilder.DevelopmentTopic(
            "Carry package without script",
            Severity.Warning)
        .WithoutFormatting("Npc uses carry package, but doesn't have carry script attached");

    public static readonly TopicDefinition NoStopCarryingEventProperty = MutagenTopicBuilder.DevelopmentTopic(
            "Carry package without StopCarryingEvent property",
            Severity.Warning)
        .WithoutFormatting("Npc uses carry package, but doesn't have StopCarryingEvent property in carry script filled");

    public static readonly TopicDefinition<IFormLinkGetter<ISkyrimMajorRecordGetter>> StopCarryingEventPropertyNotIdleAnimation = MutagenTopicBuilder.DevelopmentTopic(
            "Carry package with wrong StopCarryingEvent property",
            Severity.Warning)
        .WithFormatting< IFormLinkGetter<ISkyrimMajorRecordGetter>>("Npc uses carry package, but StopCarryingEvent property in carry script is not set to OffsetStop but {0}");

    public static readonly TopicDefinition<IFormLinkGetter<ISkyrimMajorRecordGetter>> NoCarryItemProperty = MutagenTopicBuilder.DevelopmentTopic(
            "Carry package without CarryItem property",
            Severity.Warning)
        .WithFormatting< IFormLinkGetter<ISkyrimMajorRecordGetter>>("Npc uses carry package, but has no CarryItem property filled");

    public IEnumerable<TopicDefinition> Topics { get; } = [CarryPackageWithoutScript, NoStopCarryingEventProperty, NoCarryItemProperty];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        if (!HasCarryPackage(param)) return;

        var npc = param.Record;
        var scriptEntry = npc.GetScript("CarryActorScript");
        if (scriptEntry is null)
        {
            param.AddTopic(
                CarryPackageWithoutScript.Format());
            return;
        }

        var stopCarryingEventProperty = scriptEntry.GetProperty<IScriptObjectPropertyGetter>("StopCarryingEvent");
        if (stopCarryingEventProperty is null)
        {
            param.AddTopic(
                NoStopCarryingEventProperty.Format());
            return;
        }

        if (stopCarryingEventProperty.Object.FormKey != FormKeys.SkyrimSE.Skyrim.IdleAnimation.OffsetStop.FormKey)
        {
            param.AddTopic(
                StopCarryingEventPropertyNotIdleAnimation.Format(stopCarryingEventProperty.Object));
            return;
        }

        var carryItemMiscProperty = scriptEntry.GetProperty<IScriptObjectPropertyGetter>("CarryItemMisc");
        if (carryItemMiscProperty is not null) return;

        var carryItemPotionProperty = scriptEntry.GetProperty<IScriptObjectPropertyGetter>("CarryItemPotion");
        if (carryItemPotionProperty is not null) return;

        var carryItemIngredientProperty = scriptEntry.GetProperty<IScriptObjectPropertyGetter>("CarryItemIngredient");
        if (carryItemIngredientProperty is not null) return;

        param.AddTopic(
            NoCarryItemProperty.Format());
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VirtualMachineAdapter?.Scripts;
    }

    private static readonly HashSet<IFormLinkGetter<IPackageGetter>> CarryPackageTemplates =
    [
        FormKeys.SkyrimSE.Skyrim.Package.CarryAndDropItem,
        FormKeys.SkyrimSE.Skyrim.Package.CarryAndKeepItem
    ];

    private static bool HasCarryPackage(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        foreach (var packageLink in param.Record.Packages)
        {
            if (!param.LinkCache.TryResolve<IPackageGetter>(packageLink.FormKey, out var package)) continue;

            if (CarryPackageTemplates.Contains(package.PackageTemplate)) return true;
        }

        return false;
    }
}
