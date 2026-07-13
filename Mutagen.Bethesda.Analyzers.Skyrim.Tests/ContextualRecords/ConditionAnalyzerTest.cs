using Mutagen.Bethesda.Analyzers.Skyrim.Record;
using Mutagen.Bethesda.Analyzers.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Tests.ContextualRecords;

using Fixture = ContextualRecordTestFixture<ConditionAnalyzer, Package, ISkyrimMajorRecordGetter>;

public class ConditionAnalyzerTest
{
    static void AddCondition(
        Package rec,
        ConditionData data,
        float comparisonValue,
        bool and = false,
        CompareOperator op = CompareOperator.EqualTo)
    {
        rec.Conditions.Add(new ConditionFloat()
        {
            Flags = and ? 0 : Condition.Flag.OR,
            Data = data,
            ComparisonValue = comparisonValue,
            CompareOperator = op,
        });
    }

    // Condition.Reference should not be null if RunOnType == Reference
    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.InvalidConditionReference);
    }

    // GetStage should compare to an existing stage
    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.InvalidStageCondition);
    }

    // GetStage may compare to stage zero, even if it does not exist
    // This does not apply to GetStageDone
    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.InvalidStageCondition);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.InvalidStageCondition);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.GetDeadCondition);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.GetCurrentTimeConditionWithOrOnDayBreak);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.GetCurrentTimeConditionWithAndOnDayBreak);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.GetCrimeGoldRunOnPlayer);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.GetCrimeGoldRunOnPlayer);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.GetCrimeGoldRunOnPlayer);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.LeveledItemParameter);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.LeveledItemParameter);
    }

    [Theory, MutagenModAutoData]
    public void AliasWithoutOwningQuest(Fixture fixture, Quest quest)
    {
        fixture.Run(prepForError: (rec, mod) =>
        {
            rec.Conditions.Add(new ConditionFloat()
            {
                Data = new GetDeadConditionData() { RunOnType = Condition.RunOnType.QuestAlias }
            });
        },
        prepForFix: (rec, mod) =>
        {
            rec.OwnerQuest.SetTo(quest);
            quest.Aliases.Add(new() { ID = 1 });
            rec.Conditions[0].Data.RunOnTypeIndex = (int)quest.Aliases[0].ID;
        },
        ConditionAnalyzer.AliasWithoutQuest);
    }

    [Theory, MutagenModAutoData]
    public void AliasWithoutOwningQuestGetIsAliasRef(Fixture fixture, Quest quest)
    {
        fixture.Run(prepForError: (rec, mod) =>
        {
            rec.Conditions.Add(new ConditionFloat()
            {
                Data = new GetIsAliasRefConditionData() { RunOnType = Condition.RunOnType.Subject, ReferenceAliasIndex = 1 }
            });
        },
        prepForFix: (rec, mod) =>
        {
            rec.OwnerQuest.SetTo(quest);
            quest.Aliases.Add(new() { ID = 1 });
        },
        ConditionAnalyzer.AliasWithoutQuest);
    }

    [Theory, MutagenModAutoData]
    public void InvalidAliasIndex(Fixture fixture, Quest quest)
    {
        fixture.Run(prepForError: (rec, mod) =>
        {
            rec.OwnerQuest.SetTo(quest);
            quest.Aliases.Add(new() { ID = 1 });

            rec.Conditions.Add(new ConditionFloat()
            {
                Data = new GetDeadConditionData() { RunOnType = Condition.RunOnType.QuestAlias, RunOnTypeIndex = 12345 }
            });
        },
        prepForFix: (rec, mod) =>
        {
            rec.Conditions[0].Data.RunOnTypeIndex = (int)quest.Aliases[0].ID;
        },
        ConditionAnalyzer.InvalidAliasIndex);
    }

    [Theory, MutagenModAutoData]
    public void InvalidAliasIndexGetIsAliasRef(Fixture fixture, Quest quest)
    {
        fixture.Run(prepForError: (rec, mod) =>
        {
            rec.OwnerQuest.SetTo(quest);
            quest.Aliases.Add(new() { ID = 1 });

            rec.Conditions.Add(new ConditionFloat()
            {
                Data = new GetIsAliasRefConditionData() { RunOnType = Condition.RunOnType.Subject, ReferenceAliasIndex = 12345 }
            });
        },
        prepForFix: (rec, mod) =>
        {
            (rec.Conditions[0].Data as IGetIsAliasRefConditionData)!.ReferenceAliasIndex = (int)quest.Aliases[0].ID;
        },
        ConditionAnalyzer.InvalidAliasIndex);
    }

    // Invalid alias may coexist with other topics on the same condition
    [Theory, MutagenModAutoData]
    public void InvalidAliasAndOtherError(Fixture fixture, Quest quest)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                rec.OwnerQuest.SetTo(quest);

                rec.Conditions.Add(new ConditionFloat()
                {
                    Data = new GetIsAliasRefConditionData() { ReferenceAliasIndex = 1, RunOnType = Condition.RunOnType.Reference },
                });
            },
            prepForFix: (rec, mod) =>
            {
                var data = rec.Conditions[0].Data as GetIsAliasRefConditionData;
                data!.Reference.SetTo(FormKeys.SkyrimSE.Skyrim.PlacedNpc.AlvorREF);
                quest.Aliases.Add(new() { ID = (ushort)data.ReferenceAliasIndex });
            },
            ConditionAnalyzer.InvalidConditionReference,
            ConditionAnalyzer.InvalidAliasIndex);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.NoVampireRace);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.NoVampireRace);
    }

    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.NoVampireRace);
    }

    // Vampire conditions should compare against the same value
    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.NoVampireRace);
    }

    // GetRace == 0 vampire conditions should be ANDed together
    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.NoVampireRace);
    }

    // GetRace == 1 vampire conditions should be part of the same OR block
    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.NoVampireRace);
    }

    // Vampire conditions should be part of the same field
    [Theory, MutagenModAutoData]
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
            ConditionAnalyzer.NoVampireRace);
    }
}
