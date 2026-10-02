using Antigen.Skyrim.Record.Conditions;
using Antigen.Skyrim.Record.Conditions.Analyzers;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using static Antigen.Skyrim.Tests.ContextualRecords.Conditions.ConditionTestUtil;

namespace Antigen.Skyrim.Tests.ContextualRecords.Conditions;

using Fixture = ContextualRecordTestFixture<ConditionAnalyzer, Package, ISkyrimMajorRecordGetter>;

public class CrimeGoldConditionAnalyzerTest
{
    [Theory, ConditionAnalyzerAutoData]
    public void CrimeGoldOnPlayer(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var data = new GetCrimeGoldConditionData();
                data.Reference.SetTo(FormKeys.SkyrimSE.Skyrim.PlayerRef);
                data.RunOnType = Condition.RunOnType.Reference;
                AddCondition(rec, data, 0);
            },
            prepForFix: (rec, mod) =>
            {
                var data = rec.Conditions[0].Data as IGetCrimeGoldConditionData;
                data!.Faction.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Faction.CrimeFactionWhiterun);
            },
            CrimeGoldConditionAnalyzer.GetCrimeGoldRunOnPlayer);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void CrimeGoldOnPlayerViolent(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var data = new GetCrimeGoldViolentConditionData();
                data.Reference.SetTo(FormKeys.SkyrimSE.Skyrim.PlayerRef);
                data.RunOnType = Condition.RunOnType.Reference;
                AddCondition(rec, data, 0);
            },
            prepForFix: (rec, mod) =>
            {
                var data = rec.Conditions[0].Data as IGetCrimeGoldViolentConditionData;
                data!.Faction.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Faction.CrimeFactionWhiterun);
            },
            CrimeGoldConditionAnalyzer.GetCrimeGoldRunOnPlayer);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void CrimeGoldOnPlayerNonViolent(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var data = new GetCrimeGoldNonviolentConditionData();
                data.Reference.SetTo(FormKeys.SkyrimSE.Skyrim.PlayerRef);
                data.RunOnType = Condition.RunOnType.Reference;
                AddCondition(rec, data, 0);
            },
            prepForFix: (rec, mod) =>
            {
                var data = rec.Conditions[0].Data as IGetCrimeGoldNonviolentConditionData;
                data!.Faction.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Faction.CrimeFactionWhiterun);
            },
            CrimeGoldConditionAnalyzer.GetCrimeGoldRunOnPlayer);
    }
}
