using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions;

internal static class ConditionTestUtil
{
    public static void AddCondition(
        Package rec,
        ConditionData data,
        float comparisonValue,
        bool and = false,
        CompareOperator op = CompareOperator.EqualTo)
    {
        rec.Conditions.Add(new ConditionFloat()
        {
            Flags = and ? default : Condition.Flag.OR,
            Data = data,
            ComparisonValue = comparisonValue,
            CompareOperator = op,
        });
    }
}
