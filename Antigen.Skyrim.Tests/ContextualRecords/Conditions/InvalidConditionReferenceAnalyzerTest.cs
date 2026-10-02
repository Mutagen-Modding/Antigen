using Antigen.Skyrim.Record.Conditions;
using Antigen.Skyrim.Record.Conditions.Analyzers;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using static Antigen.Skyrim.Tests.ContextualRecords.Conditions.ConditionTestUtil;

namespace Antigen.Skyrim.Tests.ContextualRecords.Conditions;

using Fixture = ContextualRecordTestFixture<ConditionAnalyzer, Package, ISkyrimMajorRecordGetter>;

public class InvalidConditionReferenceAnalyzerTest
{
    // Condition.Reference should not be null if RunOnType == Reference
    [Theory, ConditionAnalyzerAutoData]
    public void RunOnNull(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                AddCondition(rec, new GetActorValueConditionData()
                {
                    RunOnType = Condition.RunOnType.Reference,
                    //Reference = null
                }, 0);
            },
            prepForFix: (rec, mod) =>
            {
                rec.Conditions[0].Data.Reference.SetTo(FormKeys.SkyrimSE.Skyrim.PlacedNpc.DelphineREF);
            },
            InvalidConditionReferenceAnalyzer.InvalidConditionReference);
    }
}
