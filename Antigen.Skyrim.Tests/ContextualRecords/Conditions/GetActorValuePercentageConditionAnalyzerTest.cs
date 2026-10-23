using Antigen.Skyrim.Record.Conditions;
using Antigen.Skyrim.Record.Conditions.Analyzers;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using static Antigen.Skyrim.Tests.ContextualRecords.Conditions.ConditionTestUtil;

namespace Antigen.Skyrim.Tests.ContextualRecords.Conditions;

using Fixture = ContextualRecordTestFixture<ConditionAnalyzer, Package, ISkyrimMajorRecordGetter>;

public sealed class GetActorValuePercentageConditionAnalyzerTest
{
    [Theory, ConditionAnalyzerAutoData]
    public void FloatComparisonAboveRange(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                AddCondition(rec, new GetActorValuePercentConditionData(), 1.5f);
            },
            prepForFix: (rec, mod) =>
            {
                ((ConditionFloat)rec.Conditions[0]).ComparisonValue = 0.5f;
            },
            GetActorValuePercentageConditionAnalyzer.GetActorValuePercentageRunOnPlayer);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void FloatComparisonBelowRange(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                AddCondition(rec, new GetActorValuePercentConditionData(), -0.5f);
            },
            prepForFix: (rec, mod) =>
            {
                ((ConditionFloat)rec.Conditions[0]).ComparisonValue = 0.5f;
            },
            GetActorValuePercentageConditionAnalyzer.GetActorValuePercentageRunOnPlayer);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void FloatComparisonInRange(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            AddCondition(rec, new GetActorValuePercentConditionData(), 0.5f);
        });
    }

    [Theory, ConditionAnalyzerAutoData]
    public void GlobalInitialValueAboveRange(Fixture fixture)
    {
        GlobalFloat? global = null;
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                global = new GlobalFloat(mod);
                global.Data = 2f;
                mod.Globals.Add(global);
                AddGlobalCondition(rec, new GetActorValuePercentConditionData(), global);
            },
            prepForFix: (rec, mod) =>
            {
                global!.Data = 0.5f;
            },
            GetActorValuePercentageConditionAnalyzer.GetActorValuePercentageRunOnPlayerGlobal);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void GlobalInitialValueInRange(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            var global = new GlobalFloat(mod);
            global.Data = 0.5f;
            mod.Globals.Add(global);
            AddGlobalCondition(rec, new GetActorValuePercentConditionData(), global);
        });
    }
}
