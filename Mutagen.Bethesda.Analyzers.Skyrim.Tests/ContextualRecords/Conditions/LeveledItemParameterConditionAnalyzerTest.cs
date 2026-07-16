using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using static Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions.ConditionTestUtil;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions;

using Fixture = ContextualRecordTestFixture<ConditionAnalyzer, Package, ISkyrimMajorRecordGetter>;

public class LeveledItemParameterConditionAnalyzerTest
{
    [Theory, ConditionAnalyzerAutoData]
    public void GetEquippedLeveledItem(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var lvli = fixture.Create<LeveledItem>();
                mod.LeveledItems.Add(lvli);

                var data = new GetEquippedConditionData();
                data.ItemOrList.Link.SetTo(lvli);
                AddCondition(rec, data, 0);
            },
            prepForFix: (rec, mod) =>
            {
                var data = (rec.Conditions[0].Data as IGetEquippedConditionData);
                data!.ItemOrList.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Armor.ArmorIronCuirass);
            },
            LeveledItemParameterConditionAnalyzer.LeveledItemParameter);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void GetCountLeveledItem(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var lvli = fixture.Create<LeveledItem>();
                mod.LeveledItems.Add(lvli);

                var data = new GetItemCountConditionData();
                data.ItemOrList.Link.SetTo(lvli);
                AddCondition(rec, data, 0);
            },
            prepForFix: (rec, mod) =>
            {
                var data = (rec.Conditions[0].Data as IGetItemCountConditionData);
                data!.ItemOrList.Link.SetTo(FormKeys.SkyrimSE.Skyrim.Armor.ArmorIronCuirass);
            },
            LeveledItemParameterConditionAnalyzer.LeveledItemParameter);
    }
}
