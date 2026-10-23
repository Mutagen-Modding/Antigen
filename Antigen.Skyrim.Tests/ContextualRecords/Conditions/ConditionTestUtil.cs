using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Tests.ContextualRecords.Conditions;

internal static class ConditionTestUtil
{
    public static void AddCondition(
        Package rec,
        ConditionData data,
        float comparisonValue,
        bool and = false,
        CompareOperator op = CompareOperator.EqualTo)
    {
        rec.Conditions.Add(new ConditionFloat
        {
            Flags = and ? default : Condition.Flag.OR,
            Data = data,
            ComparisonValue = comparisonValue,
            CompareOperator = op,
        });
    }

    public static void AddGlobalCondition(
        Package rec,
        ConditionData data,
        IGlobalGetter global,
        bool and = false,
        CompareOperator op = CompareOperator.EqualTo)
    {
        rec.Conditions.Add(new ConditionGlobal
        {
            Flags = and ? default : Condition.Flag.OR,
            Data = data,
            ComparisonValue = new FormLink<IGlobalGetter>(global.FormKey),
            CompareOperator = op,
        });
    }
}
