using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class RackAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    private const string TriggerScriptName = "WeaponRackTriggerSCRIPT";
    private const string TriggerPropertyName = "WRackTrigger";
    private const string ActivatorScriptName = "WeaponRackActivateSCRIPT";
    private const string ActivatorPropertyName = "WRackActivator";

    public static readonly TopicDefinition RackTriggerMissingKeywordProperty = MutagenTopicBuilder.FromDiscussion(
            367,
            "Rack Trigger Missing Keyword Property",
            Severity.Error)
        .WithoutFormatting($"Placed Object has a {TriggerScriptName} but property {ActivatorPropertyName} is not filled");

    public static readonly TopicDefinition<IKeywordGetter> RackActivatorMissing = MutagenTopicBuilder.FromDiscussion(
            368,
            "Rack Activator Missing",
            Severity.Error)
        .WithFormatting<IKeywordGetter>("Placed Object has a " + TriggerScriptName + " but no linked ref with keyword {0} to a rack activator");

    public static readonly TopicDefinition<IPlacedObjectGetter> RackActivatorInvalid = MutagenTopicBuilder.FromDiscussion(
            369,
            "Rack Activator Invalid",
            Severity.Error)
        .WithFormatting<IPlacedObjectGetter>($"Placed Object has a {TriggerScriptName} but the linked ref activator {{0}} does not have the script {ActivatorScriptName}");

    public static readonly TopicDefinition<IPlacedObjectGetter> RackActivatorMissingKeywordProperty = MutagenTopicBuilder.FromDiscussion(
            397,
            "Rack Activator Missing Keyword Property",
            Severity.Error)
        .WithFormatting<IPlacedObjectGetter>($"Placed Object has a {TriggerScriptName} but the linked ref activator {{0}} with script {ActivatorPropertyName} doesn't have property {TriggerPropertyName} filled");

    public static readonly TopicDefinition<IPlacedObjectGetter, IKeywordGetter> RackActivatorNoTrigger = MutagenTopicBuilder.FromDiscussion(
            370,
            "Rack Activator No Trigger",
            Severity.Error)
        .WithFormatting<IPlacedObjectGetter, IKeywordGetter>($"Placed Object has a {TriggerScriptName} but the linked ref activator {{0}} does not have a linked ref with keyword {{1}} back to the rack trigger");

    public static readonly TopicDefinition<IPlacedObjectGetter, IKeywordGetter, IPlacedGetter> RackActivatorLinksToDifferentTrigger = MutagenTopicBuilder.FromDiscussion(
            371,
            "Rack Activator Links to Different Trigger",
            Severity.Error)
        .WithFormatting<IPlacedObjectGetter, IKeywordGetter, IPlacedGetter>($"Placed Object has a {TriggerScriptName} but the linked ref activator {{0}} has a linked ref with keyword {{1}} that links to a different trigger {{2}} and not back to the rack trigger");

    public static readonly TopicDefinition<IPlacedObjectGetter, IPlacedObjectGetter, IPlaceableObjectGetter?> RackActivatorDisplayItemInvalidType = MutagenTopicBuilder.FromDiscussion(
            372,
            "Rack Display Item Invalid Has Type",
            Severity.Error)
        .WithFormatting<IPlacedObjectGetter, IPlacedObjectGetter, IPlaceableObjectGetter?>($"Placed Object has a {TriggerScriptName} but the linked ref activator {{0}} has a linked ref without keyword {{1}} that places {{2}} and not a weapon or armor");

    public IEnumerable<TopicDefinition> Topics { get; } =
    [
        RackTriggerMissingKeywordProperty,
        RackActivatorMissing,
        RackActivatorInvalid,
        RackActivatorNoTrigger,
        RackActivatorLinksToDifferentTrigger,
        RackActivatorDisplayItemInvalidType,
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var triggerRef = param.Record;

        if (triggerRef.IsDeleted) return;

        var (triggerScript, activatorLinkProperty) = triggerRef.GetScriptPropertyFromSelfOrBase<IScriptObjectPropertyGetter>(param.LinkCache, TriggerScriptName, ActivatorPropertyName);
        if (triggerScript is null) return;

        var activatorLinkKeyword = activatorLinkProperty?.Object.TryResolve<IKeywordGetter>(param.LinkCache);
        if (activatorLinkKeyword is null)
        {
            param.AddTopic(
                RackTriggerMissingKeywordProperty.Format());
            return;
        }

        var activator = triggerRef.GetLinkedReference(param.LinkCache, activatorLinkKeyword.ToLink());
        if (activator is not IPlacedObjectGetter activatorRef)
        {
            param.AddTopic(
                RackActivatorMissing.Format(activatorLinkKeyword));
            return;
        }

        var (activatorScript, triggerLinkProperty) = activatorRef.GetScriptPropertyFromSelfOrBase<IScriptObjectPropertyGetter>(param.LinkCache, ActivatorScriptName, TriggerPropertyName);
        if (activatorScript is null)
        {
            param.AddTopic(
                RackActivatorInvalid.Format(activatorRef));
            return;
        }

        var triggerLinkKeyword = triggerLinkProperty?.Object.TryResolve<IKeywordGetter>(param.LinkCache);
        if (triggerLinkKeyword is null)
        {
            param.AddTopic(
                RackActivatorMissingKeywordProperty.Format(activatorRef));
            return;
        }

        var linkedTriggerRef = activatorRef.GetLinkedReference(param.LinkCache, triggerLinkKeyword.ToLink());
        if (linkedTriggerRef is null)
        {
            param.AddTopic(
                RackActivatorNoTrigger.Format(activatorRef, triggerLinkKeyword));
            return;
        }

        if (linkedTriggerRef.FormKey != triggerRef.FormKey)
        {
            param.AddTopic(
                RackActivatorLinksToDifferentTrigger.Format(activatorRef, triggerLinkKeyword, linkedTriggerRef));
            return;
        }

        var displayItem = activatorRef.GetLinkedReference(param.LinkCache);
        if (displayItem is not IPlacedObjectGetter displayItemRef)
        {
            // Nothing displayed - that's okay
            return;
        }

        var displayItemBase = displayItemRef.Base.TryResolve(param.LinkCache);
        if (displayItemBase is not IArmorGetter and not IWeaponGetter)
        {
            param.AddTopic(
                RackActivatorDisplayItemInvalidType.Format(activatorRef, displayItemRef, displayItemBase));
            return;
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VirtualMachineAdapter;
        yield return x => x.Base;
        yield return x => x.LinkedReferences;
    }
}
