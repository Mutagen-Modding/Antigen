using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Weapon;

public class TemperAnalyzerWeapon : IContextualRecordAnalyzer<IWeaponGetter>
{
    public static readonly TopicDefinition NoTemper = MutagenTopicBuilder.FromDiscussion(
            574,
            "No weapon temper recipe",
            Severity.Suggestion)
        .WithoutFormatting("Weapon has no tempering recipe");

    public static readonly TopicDefinition MultipleTemper = MutagenTopicBuilder.FromDiscussion(
            575,
            "Multiple weapon temper recipes",
            Severity.Warning)
        .WithoutFormatting("Weapon has multiple tempering recipes");

    public IEnumerable<TopicDefinition> Topics => [NoTemper, MultipleTemper];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IWeaponGetter> param)
    {
        var weapon = param.Record;
        // Items with templates inherit their template's temper recipe
        if (!weapon.Template.IsNull)
            return;
        if (weapon.Data?.Flags.HasFlag(WeaponData.Flag.NonPlayable) ?? false)
            return;
        if (weapon.Data?.AnimationType is WeaponAnimationType.Staff or WeaponAnimationType.HandToHand)
            return;
        if (weapon.HasKeyword(FormKeys.SkyrimSE.Skyrim.Keyword.Dummy))
            return;

        var recipes = weapon.GetTemperRecipes(
            FormKeys.SkyrimSE.Skyrim.Keyword.CraftingSmithingSharpeningWheel,
            param.ResolveCache<ILinkUsageCache>(),
            param.LinkCache)
            .ToArray();

        switch (recipes.Length)
        {
            case 0:
                param.AddTopic(NoTemper.Format());
                break;
            case > 1:
                param.AddTopic(MultipleTemper.Format());
                break;
        }
    }

    public IEnumerable<Func<IWeaponGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Template;
        yield return x => x.Data;
        yield return x => x.Keywords;
    }
}
