using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Armor;

public class FootstepAnalyzer : IContextualRecordAnalyzer<IArmorGetter>
{
    public static readonly TopicDefinition<ArmorType> UnknownArmorType = MutagenTopicBuilder.DevelopmentTopic(
            "Unknown Armor Type",
            Severity.Suggestion)
        .WithFormatting<ArmorType>("Armor type is set to unkown value {0}");

    public static readonly TopicDefinition<ArmorType, IFormLinkGetter<IFootstepSetGetter>> ArmorMatchingFootstepArmorType = MutagenTopicBuilder.DevelopmentTopic(
            "Footsteps on armor don't match their equipped armor type",
            Severity.Suggestion)
        .WithFormatting<ArmorType, IFormLinkGetter<IFootstepSetGetter>>("Armor has armor type {0} but armor addon doesn't have footstep {1}");

    public static readonly TopicDefinition ArmorMissingFootstep = MutagenTopicBuilder.DevelopmentTopic(
            "Armor has no footstep sound",
            Severity.Warning)
        .WithoutFormatting("Armor has no armor addon that adds footstep sounds");

    public static readonly TopicDefinition<IFormLinkGetter<IRaceGetter>> ArmorDuplicateFootstep = MutagenTopicBuilder.DevelopmentTopic(
            "Armor has more than one armor addon that adds footstep sound",
            Severity.Suggestion)
        .WithFormatting<IFormLinkGetter<IRaceGetter>>("Armor has multiple armor addons that have footstep sounds which are enabled for the same race {0}");

    public IEnumerable<TopicDefinition> Topics => [ArmorMatchingFootstepArmorType, ArmorMissingFootstep, ArmorDuplicateFootstep];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IArmorGetter> param)
    {
        var armor = param.Record;

        // Armor with template armor inherit all relevant data from the template armor and should not be checked themselves
        if (!armor.TemplateArmor.IsNull) return;

        // Only armor with feet slots are relevant for footsteps
        if (armor.BodyTemplate is null || !armor.BodyTemplate.FirstPersonFlags.HasFlag(BipedObjectFlag.Feet)) return;

        var armorAddons = armor.Armature
            .Select(armorAddonLink => armorAddonLink.TryResolve(param.LinkCache))
            .WhereNotNull()
            .ToList();

        // Check duplicate footsteps
        var armorAddonRaces = armorAddons
            .Where(x => !x.FootstepSound.IsNull && !x.Race.IsNull)
            .SelectMany<IArmorAddonGetter, (IArmorAddonGetter ArmorAddon, IFormLinkGetter<IRaceGetter> Race)>(x =>
                [(ArmorAddon: x, Race: new FormLink<IRaceGetter>(x.Race.FormKey)), ..x.AdditionalRaces.Select(race => (ArmorAddon: x, Race: race))])
            .GroupBy(x => x.Race)
            .ToDictionary(x => x.Key, x => x.Select(x => x.ArmorAddon).ToList());

        foreach (var (race, addons) in armorAddonRaces) {
            if (addons.Count <= 1) continue;

            param.AddTopic(
                ArmorDuplicateFootstep.Format(race),
                ("Addons", addons));
        }

        // Check if the footstep sound is correct
        IFormLinkGetter<IFootstepSetGetter> correctFootstepSound;
        switch (armor.BodyTemplate.ArmorType)
        {
            case ArmorType.LightArmor:
                correctFootstepSound = FormKeys.SkyrimSE.Skyrim.FootstepSet.FSTArmorLightFootstepSet;
                break;
            case ArmorType.HeavyArmor:
                correctFootstepSound = FormKeys.SkyrimSE.Skyrim.FootstepSet.FSTArmorHeavyFootstepSet;
                break;
            case ArmorType.Clothing:
                if (armor.EditorID is not null && armor.EditorID.Contains("Naked"))
                {
                    correctFootstepSound = FormKeys.SkyrimSE.Skyrim.FootstepSet.FSTBarefootFootstepSet;
                }
                else
                {
                    correctFootstepSound = FormKeys.SkyrimSE.Skyrim.FootstepSet.DefaultFootstepSet;
                }
                break;
            default:
                param.AddTopic(
                    UnknownArmorType.Format(armor.BodyTemplate.ArmorType));

                return;
        }

        foreach (var armorAddon in armorAddons)
        {
            if (armorAddon.FootstepSound.FormKey == correctFootstepSound.FormKey) continue;

            param.AddTopic(
                ArmorMatchingFootstepArmorType.Format(armor.BodyTemplate.ArmorType, correctFootstepSound));
        }

        // Check if there are any footstep sounds
        if (armorAddons.Count == 0 || armorAddons.TrueForAll(x => x.FootstepSound.IsNull))
        {
            param.AddTopic(
                ArmorMissingFootstep.Format());
        }
    }

    public IEnumerable<Func<IArmorGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.TemplateArmor;
        yield return x => x.BodyTemplate;
        yield return x => x.Armature;
    }
}
