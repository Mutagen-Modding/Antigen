using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class DummyAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition<IFormLinkNullableGetter<IPlaceableObjectGetter>> DummyItemWithoutLeveledList = MutagenTopicBuilder.FromDiscussion(
            388,
            "Dummy Item Without Leveled List",
            Severity.Warning)
        .WithFormatting<IFormLinkNullableGetter<IPlaceableObjectGetter>>("Placed Object is a dummy item {0} without a leveled list");

    public static readonly TopicDefinition<IFormLinkNullableGetter<IPlaceableObjectGetter>, IFormLinkNullableGetter<ILeveledItemGetter>> NonDummyItemWithLeveledList = MutagenTopicBuilder.FromDiscussion(
            387,
            "Non-Dummy Item With Leveled List",
            Severity.Warning)
        .WithFormatting<IFormLinkNullableGetter<IPlaceableObjectGetter>, IFormLinkNullableGetter<ILeveledItemGetter>>("Placed Object is not a dummy item {0} but has a leveled list {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [DummyItemWithoutLeveledList, NonDummyItemWithLeveledList];

    private static readonly HashSet<FormKey> DummyItems =
    [
        FormKeys.SkyrimSE.Skyrim.Ammunition.DummyArrow.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.DummyBoots.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.DummyCuirass.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.DummyGauntlets.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.DummyGauntlets.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.DummyHelmet.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.DummyShield.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.CWDummyBootsImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.CWDummyBootsSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.CWDummyCuirassImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.CWDummyCuirassSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.CWDummyGauntletsImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.CWDummyGauntletsSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.CWDummyHelmetImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.CWDummyHelmetSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.CWDummyShieldImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Armor.CWDummyShieldSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Book.DummyBook.FormKey,
        FormKeys.SkyrimSE.Skyrim.Ingestible.DummyPotion.FormKey,
        FormKeys.SkyrimSE.Skyrim.SoulGem.DummySoulGem.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.DummyBattleaxe.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.DummyBow.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.DummyDagger.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.DummyGreatSword.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.DummyMace.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.DummySword.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.DummyWarAxe.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.DummyWarhammer.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.DummyWeapon1H.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.DummyWeapon2H.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyBattleaxeImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyBowImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyDaggerImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyGreatSwordImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyMaceImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummySwordImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyAxeImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyWarhammerImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyWeapon1HImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyWeapon2HImperial.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyBattleaxeSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyBowSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyDaggerSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyGreatSwordSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyMaceSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummySwordSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyAxeSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyWarhammerSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyWeapon1HSons.FormKey,
        FormKeys.SkyrimSE.Skyrim.Weapon.CWDummyWeapon2HSons.FormKey,
    ];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        var hasLeveledList = placedObject.LeveledItemBaseObject.IsNull == false;
        var isDummyItem = DummyItems.Contains(placedObject.Base.FormKey);

        if (isDummyItem && !hasLeveledList)
        {
            param.AddTopic(
                DummyItemWithoutLeveledList.Format(placedObject.Base));
        }
        else if (!isDummyItem && hasLeveledList)
        {
            param.AddTopic(
                NonDummyItemWithLeveledList.Format(placedObject.Base, placedObject.LeveledItemBaseObject));
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Base;
        yield return x => x.LeveledItemBaseObject;
    }
}
