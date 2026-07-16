using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using static Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions.ConditionTestUtil;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions;

using Fixture = ContextualRecordTestFixture<ConditionAnalyzer, Package, ISkyrimMajorRecordGetter>;

public class GetDeadConditionAnalyzerTest
{
    [Theory, ConditionAnalyzerAutoData]
    public void GetDeadOnUnique(Fixture fixture)
    {
        var npc = fixture.Create<Npc>();
        npc.Configuration.Flags |= NpcConfiguration.Flag.Unique;

        fixture.Run(
            prepForError: (rec, mod) =>
            {
                mod.Npcs.Add(npc);
                var cell = fixture.Create<Cell>();
                cell.Flags |= Cell.Flag.IsInteriorCell;
                mod.Cells.AddInteriorCell(cell);

                var placed = fixture.Create<PlacedNpc>();
                placed.Base.SetTo(npc);
                cell.Temporary.Add(placed);

                var data = new GetDeadConditionData();
                data.Reference.SetTo(placed);
                data.RunOnType = Condition.RunOnType.Reference;
                AddCondition(rec, data, 1);
            },
            prepForFix: (rec, mod) =>
            {
                var data = new GetDeadCountConditionData();
                data.Npc.Link.SetTo(npc);
                rec.Conditions[0].Data = data;
            },
            GetDeadConditionAnalyzer.GetDeadCondition);
    }
}
