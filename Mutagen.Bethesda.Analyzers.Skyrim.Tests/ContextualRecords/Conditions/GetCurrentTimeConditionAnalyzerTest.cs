using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using static Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions.ConditionTestUtil;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions;

using Fixture = ContextualRecordTestFixture<ConditionAnalyzer, Package, ISkyrimMajorRecordGetter>;

public class GetCurrentTimeConditionAnalyzerTest
{
    [Theory, ConditionAnalyzerAutoData]
    public void GetCurrentTimeAlwaysTrue(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                // Time > 10 || Time < 20
                AddCondition(rec, new GetCurrentTimeConditionData(), 10, and: false, CompareOperator.GreaterThan);
                AddCondition(rec, new GetCurrentTimeConditionData(), 20, and: false, CompareOperator.LessThan);
            },
            prepForFix: (rec, mod) =>
            {
                // Time > 10 && Time < 20
                rec.Conditions[0].Flags &= ~Condition.Flag.OR;
            },
            GetCurrentTimeConditionAnalyzer.GetCurrentTimeConditionWithOrOnDayBreak);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void GetCurrentTimeAlwaysFalse(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                // Time < 10 && Time > 20
                AddCondition(rec, new GetCurrentTimeConditionData(), 10, and: true, CompareOperator.LessThan);
                AddCondition(rec, new GetCurrentTimeConditionData(), 20, and: true, CompareOperator.GreaterThan);
            },
            prepForFix: (rec, mod) =>
            {
                // Time < 10 || Time > 20
                rec.Conditions[0].Flags |= Condition.Flag.OR;
            },
            GetCurrentTimeConditionAnalyzer.GetCurrentTimeConditionWithAndOnDayBreak);
    }
}
