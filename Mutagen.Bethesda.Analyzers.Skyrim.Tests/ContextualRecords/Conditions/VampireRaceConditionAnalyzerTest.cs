using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions;

using Fixture = ContextualRecordTestFixture<ConditionAnalyzer, Package, ISkyrimMajorRecordGetter>;

public class VampireRaceConditionAnalyzerTest
{
    [Theory, ConditionAnalyzerAutoData]
    public void VampireConditionGetRace(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var data = new GetIsRaceConditionData();
                data.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRace);
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data,
                    Flags = Condition.Flag.OR,
                    ComparisonValue = 1,
                });
            },
            prepForFix: (rec, mod) =>
            {
                var data = new GetIsRaceConditionData();
                data.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRaceVampire);
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data,
                    ComparisonValue = 1,
                });
            },
            VampireRaceConditionAnalyzer.NoVampireRace);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void VampireConditionGetPCRace(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var data = new GetPCIsRaceConditionData();
                data.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRace);
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data,
                    Flags = Condition.Flag.OR,
                    ComparisonValue = 1,
                });
            },
            prepForFix: (rec, mod) =>
            {
                var data = new GetPCIsRaceConditionData();
                data.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRaceVampire);
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data,
                    ComparisonValue = 1
                });
            },
            VampireRaceConditionAnalyzer.NoVampireRace);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void VampireConditionDifferentTarget(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var data = new GetIsRaceConditionData();
                data.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRace);
                data.RunOnType = Condition.RunOnType.Subject;
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data,
                    Flags = Condition.Flag.OR,
                    ComparisonValue = 1,
                });
                var vampData = new GetIsRaceConditionData();
                vampData.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRaceVampire);
                vampData.RunOnType = Condition.RunOnType.Target;
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = vampData,
                    ComparisonValue = 1,
                });
            },
            prepForFix: (rec, mod) =>
            {
                rec.Conditions[1].Data.RunOnType = Condition.RunOnType.Subject;
            },
            VampireRaceConditionAnalyzer.NoVampireRace);
    }

    // Vampire conditions should compare against the same value
    [Theory, ConditionAnalyzerAutoData]
    public void VampireConditionDifferentValue(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var data = new GetIsRaceConditionData();
                data.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRace);
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data,
                    ComparisonValue = 0,
                    Flags = Condition.Flag.OR,
                });
                var data2 = new GetIsRaceConditionData();
                data2.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRaceVampire);
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data2,
                    ComparisonValue = 1
                });
            },
            prepForFix: (rec, mod) =>
            {
                (rec.Conditions[0] as ConditionFloat)!.ComparisonValue = 1;
            },
            VampireRaceConditionAnalyzer.NoVampireRace);
    }

    // GetRace == 0 vampire conditions should be ANDed together
    [Theory, ConditionAnalyzerAutoData]
    public void VampireConditionNegativeCombineAnd(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var data = new GetIsRaceConditionData();
                data.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRace);
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data,
                    ComparisonValue = 0,
                    Flags = Condition.Flag.OR,
                });
                var data2 = new GetIsRaceConditionData();
                data2.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRaceVampire);
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data2,
                    ComparisonValue = 0,
                });
            },
            prepForFix: (rec, mod) =>
            {
                rec.Conditions[0].Flags &= ~Condition.Flag.OR;
            },
            VampireRaceConditionAnalyzer.NoVampireRace);
    }

    // GetRace == 1 vampire conditions should be part of the same OR block
    [Theory, ConditionAnalyzerAutoData]
    public void VampireConditionPositiveCombineOr(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var data = new GetIsRaceConditionData();
                data.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRace);
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data,
                    ComparisonValue = 1,
                });
                var data2 = new GetIsRaceConditionData();
                data2.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRaceVampire);
                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = data2,
                    ComparisonValue = 1,
                });
            },
            prepForFix: (rec, mod) =>
            {
                rec.Conditions[0].Flags |= Condition.Flag.OR;
            },
            VampireRaceConditionAnalyzer.NoVampireRace);
    }

    // Vampire conditions should be part of the same field
    [Theory, ConditionAnalyzerAutoData]
    public void VampireConditionDifferentField(ContextualRecordTestFixture<ConditionAnalyzer, Quest, ISkyrimMajorRecordGetter> fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var data = new GetIsRaceConditionData();
                data.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRace);
                rec.DialogConditions.Add(new ConditionFloat()
                {
                    Data = data,
                    Flags = Condition.Flag.OR,
                    ComparisonValue = 1,
                });
                var data2 = new GetIsRaceConditionData();
                data2.Race.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Race.NordRaceVampire);
                rec.EventConditions.Add(new ConditionFloat()
                {
                    Data = data2,
                    ComparisonValue = 1,
                });
            },
            prepForFix: (rec, mod) =>
            {
                rec.DialogConditions.Add(rec.EventConditions[0]);
                rec.EventConditions.Clear();
            },
            VampireRaceConditionAnalyzer.NoVampireRace);
    }
}
