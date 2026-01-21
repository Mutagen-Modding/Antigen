using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class CarryPackageAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition CarryPackageWithoutScript = MutagenTopicBuilder.FromDiscussion(
            240,
            "Carry package without script",
            Severity.Warning)
        .WithoutFormatting("Npc uses carry package, but doesn't have carry script attached");

    public static readonly TopicDefinition NoStopCarryingEventProperty = MutagenTopicBuilder.FromDiscussion(
            315,
            "Carry package without StopCarryingEvent property",
            Severity.Warning)
        .WithoutFormatting("Npc uses carry package, but doesn't have StopCarryingEvent property in carry script filled");

    public static readonly TopicDefinition<IFormLinkGetter<ISkyrimMajorRecordGetter>> StopCarryingEventPropertyNotIdleAnimation = MutagenTopicBuilder.FromDiscussion(
            316,
            "Carry package with wrong StopCarryingEvent property",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter<ISkyrimMajorRecordGetter>>("Npc uses carry package, but StopCarryingEvent property in carry script is not set to OffsetStop but {0}");

    public static readonly TopicDefinition<IFormLinkGetter<ISkyrimMajorRecordGetter>> NoCarryItemProperty = MutagenTopicBuilder.FromDiscussion(
            317,
            "Carry package without CarryItem property",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter<ISkyrimMajorRecordGetter>>("Npc uses carry package, but has no CarryItem property filled");

    public static readonly TopicDefinition<IPlacedNpcGetter> NoLinkCarryStart = MutagenTopicBuilder.FromDiscussion(
            490,
            "Carry package without LinkCarryStart",
            Severity.Warning)
        .WithFormatting<IPlacedNpcGetter>("Npc placement {0} does not have a linked reference with LinkCarryStart keyword");

    public static readonly TopicDefinition<IPlacedNpcGetter> NoLinkCarryEnd = MutagenTopicBuilder.FromDiscussion(
            491,
            "Carry package without LinkCarryEnd",
            Severity.Warning)
        .WithFormatting<IPlacedNpcGetter>("Npc placement {0} does not have a linked reference with LinkCarryEnd keyword");

    public static readonly TopicDefinition<IPlacedNpcGetter, ICellGetter, ICellGetter> LinkCarryStartEndWithLoadScreenInBetween = MutagenTopicBuilder.FromDiscussion(
            492,
            "Carry package with LinkCarryStart and LinkCarryEnd have Load Screen in between",
            Severity.Warning)
        .WithFormatting<IPlacedNpcGetter, ICellGetter, ICellGetter>("Npc placement {0} has LinkCarryStart in cell {1} and LinkCarryEnd in cell {2} with a load door between them, resetting the carry animation");

    public IEnumerable<TopicDefinition> Topics { get; } = [CarryPackageWithoutScript, NoStopCarryingEventProperty, NoCarryItemProperty, StopCarryingEventPropertyNotIdleAnimation, NoLinkCarryStart, NoLinkCarryEnd, LinkCarryStartEndWithLoadScreenInBetween];

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
        if (carryItemMiscProperty is null)
        {
            var carryItemPotionProperty = scriptEntry.GetProperty<IScriptObjectPropertyGetter>("CarryItemPotion");
            if (carryItemPotionProperty is null)
            {
                var carryItemIngredientProperty = scriptEntry.GetProperty<IScriptObjectPropertyGetter>("CarryItemIngredient");
                if (carryItemIngredientProperty is null)
                {
                    param.AddTopic(
                        NoCarryItemProperty.Format());
                }
            }
        }

        var placedNpcs = param.ResolveCache<ILinkUsageCache>()
            .GetUsagesOf<IPlacedNpcGetter>(npc).UsageLinks
            .Select(x => x.TryResolve(param.LinkCache))
            .WhereNotNull();

        foreach (var placedNpc in placedNpcs)
        {

            IPlacedGetter? carryLinkStart = null;
            IPlacedGetter? carryLinkEnd = null;
            foreach (var linkedReference in placedNpc.LinkedReferences)
            {
                var keyword = linkedReference.KeywordOrReference.TryResolve<IKeywordGetter>(param.LinkCache);

                if (keyword is null) continue;

                if (keyword.FormKey == FormKeys.SkyrimSE.Skyrim.Keyword.LinkCarryStart.FormKey)
                {
                    carryLinkStart = linkedReference.Reference.TryResolve(param.LinkCache);
                }
                else if (keyword.FormKey == FormKeys.SkyrimSE.Skyrim.Keyword.LinkCarryEnd.FormKey)
                {
                    carryLinkEnd = linkedReference.Reference.TryResolve(param.LinkCache);
                }
            }

            if (carryLinkStart is null)
            {
                param.AddTopic(
                    NoLinkCarryStart.Format(placedNpc));

                if (carryLinkEnd is null)
                {
                    param.AddTopic(
                        NoLinkCarryEnd.Format(placedNpc));
                }
            }
            else
            {
                if (carryLinkEnd is null)
                {
                    param.AddTopic(
                        NoLinkCarryEnd.Format(placedNpc));
                }
                else
                {
                    if (param.LinkCache.TryResolveSimpleContext<IPlacedGetter>(carryLinkEnd.FormKey, out var endContext) &&
                        param.LinkCache.TryResolveSimpleContext<IPlacedGetter>(carryLinkStart.FormKey, out var startContext) &&
                        startContext.Parent?.Record is ICellGetter startCell &&
                        endContext.Parent?.Record is ICellGetter endCell)
                    {
                        if (startCell.IsInteriorCell() && startCell.FormKey != endCell.FormKey
                            || endCell.IsInteriorCell() && endCell.FormKey != startCell.FormKey
                            || (startCell.IsExteriorCell() && endCell.IsExteriorCell() && startCell.GetWorldspace(param.LinkCache)?.FormKey != endCell.GetWorldspace(param.LinkCache)?.FormKey))
                        {
                            param.AddTopic(
                                LinkCarryStartEndWithLoadScreenInBetween.Format(placedNpc, startCell, endCell));
                        }
                    }
                }
            }
        }
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
