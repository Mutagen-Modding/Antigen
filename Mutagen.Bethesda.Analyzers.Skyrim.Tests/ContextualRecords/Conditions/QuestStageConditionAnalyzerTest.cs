using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions;
using Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions.Analyzers;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Xunit;
using static Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions.ConditionTestUtil;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords.Conditions;

using Fixture = ContextualRecordTestFixture<ConditionAnalyzer, Package, ISkyrimMajorRecordGetter>;

public class QuestStageConditionAnalyzerTest
{
    // GetStage should compare to an existing stage
    [Theory, ConditionAnalyzerAutoData]
    public void InvalidQuestStageGetStage(Fixture fixture)
    {

        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var quest = fixture.Create<Quest>();
                mod.Quests.Add(quest);
                quest.Stages.Add(new QuestStage() { Index = 10 });

                var data = new GetStageConditionData();
                data.Quest.Link.SetTo(quest);

                AddCondition(rec, data, 20);
            },
            prepForFix: (rec, mod) =>
            {
                (rec.Conditions[0] as IConditionFloat)!.ComparisonValue = 10;
            },
            QuestStageConditionAnalyzer.InvalidStageCondition);
    }

    // GetStage may compare to stage zero, even if it does not exist
    // This does not apply to GetStageDone
    [Theory, ConditionAnalyzerAutoData]
    public void GetStageZero(Fixture fixture)
    {
        var quest = fixture.Create<Quest>();

        fixture.Run(
            prepForError: (rec, mod) => {
                mod.Quests.Add(quest);
                // No stage 0
                //quest.Stages.Add(new QuestStage() { Index = 0 });

                var data = new GetStageDoneConditionData();
                data.Quest.Link.SetTo(quest);
                data.Stage = 0;

                AddCondition(rec, data, 0);
            },
            prepForFix: (rec, mod) =>
            {
                var data = new GetStageConditionData();
                data.Quest.Link.SetTo(quest);
                var condition = rec.Conditions[0] as IConditionFloat;
                condition!.Data = data;
                condition.ComparisonValue = 0;
            },
            QuestStageConditionAnalyzer.InvalidStageCondition);
    }

    [Theory, ConditionAnalyzerAutoData]
    public void InvalidQuestStageGetStageDone(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                var quest = fixture.Create<Quest>();
                mod.Quests.Add(quest);
                quest.Stages.Add(new QuestStage() { Index = 10 });

                var data = new GetStageDoneConditionData();
                data.Quest.Link.SetTo(quest);
                data.Stage = 20;
                AddCondition(rec, data, 1);
            },
            prepForFix: (rec, mod) =>
            {
                (rec.Conditions[0].Data as IGetStageDoneConditionData)!.Stage = 10;
            },
            QuestStageConditionAnalyzer.InvalidStageCondition);
    }
}
