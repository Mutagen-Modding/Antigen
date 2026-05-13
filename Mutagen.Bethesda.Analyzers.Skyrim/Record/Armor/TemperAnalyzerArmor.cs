using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Armor;

public class TemperAnalyzerArmor : IContextualRecordAnalyzer<IArmorGetter>
{
    public static readonly TopicDefinition NoTemper = MutagenTopicBuilder.FromDiscussion(
            572,
            "No armor temper recipe",
            Severity.Suggestion)
        .WithoutFormatting("Armor has no tempering recipe");

    public static readonly TopicDefinition MultipleTemper = MutagenTopicBuilder.FromDiscussion(
            573,
            "Multiple armor temper recipies",
            Severity.Warning)
        .WithoutFormatting("Armor has multiple tempering recipies");

    public IEnumerable<TopicDefinition> Topics => [NoTemper, MultipleTemper];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IArmorGetter> param)
    {
        var armor = param.Record;
        // Items with templates inherit their template's temper recipe
        if (!armor.TemplateArmor.IsNull)
            return;
        // Both flags are used depending on form version
        if (armor.MajorFlags.HasFlag(Bethesda.Skyrim.Armor.MajorFlag.NonPlayable) ||
            (armor.BodyTemplate?.Flags.HasFlag(BodyTemplate.Flag.NonPlayable) ?? false))
            return;
        if (armor.BodyTemplate?.ArmorType == ArmorType.Clothing)
            return;
        if (armor.HasKeyword(FormKeys.SkyrimSE.Skyrim.Keyword.Dummy))
            return;

        var recipes = armor.GetTemperRecipes(
            FormKeys.SkyrimSE.Skyrim.Keyword.CraftingSmithingArmorTable,
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

    public IEnumerable<Func<IArmorGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.TemplateArmor;
        yield return x => x.MajorFlags;
        yield return x => x.Keywords;
    }
}
