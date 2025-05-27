using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Constructible;

public class MissingEnchantedPerkAnalyzer : IIsolatedRecordAnalyzer<IConstructibleObjectGetter>
{
    public static readonly TopicDefinition MissingCreatedObject = MutagenTopicBuilder.FromDiscussion(
            221,
            "Missing Perk for Enchanted Weapon/Armor",
            Severity.Suggestion)
        .WithoutFormatting("Temper recipes need to have condition EPTemperingItemIsEnchanted = 0 or HasPerk ArcaneBlacksmith");

    public IEnumerable<TopicDefinition> Topics { get; } = [MissingCreatedObject];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IConstructibleObjectGetter> param)
    {
        var constructibleObject = param.Record;

        // Only applicable to temper crafting stations for weapons and armor
        if (constructibleObject.WorkbenchKeyword.FormKey != FormKeys.SkyrimSE.Skyrim.Keyword.CraftingSmithingSharpeningWheel.FormKey
            && constructibleObject.WorkbenchKeyword.FormKey != FormKeys.SkyrimSE.Skyrim.Keyword.CraftingSmithingArmorTable.FormKey) return;

        var perkCondition = false;
        var isEnchanted = false;
        foreach (var condition in constructibleObject.Conditions) {
            switch (condition.Data) {
                case IEPTemperingItemIsEnchantedConditionDataGetter: {
                    isEnchanted = true;
                    if (perkCondition) return;

                    break;
                }
                case IHasPerkConditionDataGetter hasPerk when hasPerk.Perk.Link.FormKey == FormKeys.SkyrimSE.Skyrim.Perk.ArcaneBlacksmith.FormKey: {
                    perkCondition = true;
                    if (isEnchanted) return;

                    break;
                }
            }

            if (!condition.Flags.HasFlag(Condition.Flag.OR)) {
                perkCondition = isEnchanted = false;
            }
        }

        param.AddTopic(MissingCreatedObject.Format());
    }

    public IEnumerable<Func<IConstructibleObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Conditions;
    }
}
