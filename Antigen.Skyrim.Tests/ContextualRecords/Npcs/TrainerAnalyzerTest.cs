using Antigen.Skyrim.Record.Npc;
using Antigen.Testing.Frameworks;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Antigen.Skyrim.Tests.ContextualRecords.Npcs;

using Fixture = ContextualRecordTestFixture<TrainerAnalyzer, Npc, INpcGetter>;

public sealed class TrainerAnalyzerTest
{
    private const string TrainerScriptName = "TrainerGoldScript";

    static void AddTrainerFaction(Fixture fixture, Npc rec, ISkyrimMod mod, string editorId)
    {
        var faction = fixture.Create<Faction>();
        faction.EditorID = editorId;
        mod.Factions.Add(faction);

        var rank = new RankPlacement();
        rank.Faction.SetTo(faction);
        rec.Factions.Add(rank);
    }

    static void AddTrainerClass(Fixture fixture, Npc rec, ISkyrimMod mod, Skill skill)
    {
        var trainerClass = fixture.Create<Class>();
        trainerClass.Teaches = skill;
        trainerClass.MaxTrainingLevel = 50;
        trainerClass.SkillWeights[skill] = 3;
        mod.Classes.Add(trainerClass);
        rec.Class.SetTo(trainerClass);
    }

    static void AddTrainerScript(Npc rec)
    {
        rec.VirtualMachineAdapter = new VirtualMachineAdapter
        {
            Scripts = [new ScriptEntry { Name = TrainerScriptName }]
        };
    }

    // Sets the npc up as a non auto calc trainer with an explicit skill level
    static void SetExplicitSkillLevel(Npc rec, Skill skill, int level)
    {
        rec.Configuration.Flags &= ~NpcConfiguration.Flag.AutoCalcStats;
        rec.Configuration.Level = new NpcLevel { Level = 1 };
        rec.PlayerSkills = new PlayerSkills();
        rec.PlayerSkills.SkillValues[skill] = (byte)level;
    }

    // Sets the npc up as an auto calc trainer, whose skill level is derived from race and class at the given minimum level
    static void SetAutoCalcStats(Fixture fixture, Npc rec, ISkyrimMod mod, short minimumLevel)
    {
        var race = fixture.Create<Race>();
        mod.Races.Add(race);
        rec.Race.SetTo(race);

        rec.Configuration.Flags |= NpcConfiguration.Flag.AutoCalcStats;
        rec.Configuration.Level = new NpcLevel { Level = minimumLevel };
    }

    // A trainer faction without the TrainerGoldScript is reported
    [Theory, MutagenModAutoData]
    public void TrainerFactionWithoutScript(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                AddTrainerFaction(fixture, rec, mod, $"JobTrainer{Skill.Alchemy}");
                AddTrainerClass(fixture, rec, mod, Skill.Alchemy);
                SetExplicitSkillLevel(rec, Skill.Alchemy, 25);
            },
            prepForFix: (rec, mod) =>
            {
                AddTrainerScript(rec);
            },
            TrainerAnalyzer.TrainerMissingScript);
    }

    // A TrainerGoldScript without a trainer faction is reported.
    // TrainerWithoutSpecialization is also reported, as there is no trainer faction to derive a specialization from
    [Theory, MutagenModAutoData]
    public void TrainerScriptWithoutFaction(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                AddTrainerScript(rec);
                AddTrainerClass(fixture, rec, mod, Skill.Alchemy);
                SetExplicitSkillLevel(rec, Skill.Alchemy, 25);
            },
            prepForFix: (rec, mod) =>
            {
                AddTrainerFaction(fixture, rec, mod, $"JobTrainer{Skill.Alchemy}");
            },
            TrainerAnalyzer.TrainerMissingFaction,
            TrainerAnalyzer.TrainerWithoutSpecialization);
    }

    // A trainer faction without a specialization suffix is reported
    [Theory, MutagenModAutoData]
    public void TrainerWithoutSpecializedFaction(Fixture fixture)
    {
        var faction = fixture.Create<Faction>();
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                faction.EditorID = "JobTrainer";
                mod.Factions.Add(faction);
                var rank = new RankPlacement();
                rank.Faction.SetTo(faction);
                rec.Factions.Add(rank);

                AddTrainerScript(rec);
                AddTrainerClass(fixture, rec, mod, Skill.Alchemy);
                SetExplicitSkillLevel(rec, Skill.Alchemy, 25);
            },
            prepForFix: (rec, mod) =>
            {
                faction.EditorID = $"JobTrainer{Skill.Alchemy}";
            },
            TrainerAnalyzer.TrainerWithoutSpecialization);
    }

    // A trainer npc whose class does not train any skill is reported
    [Theory, MutagenModAutoData]
    public void TrainerWithoutTrainerClass(Fixture fixture)
    {
        var plainClass = fixture.Create<Class>();
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                AddTrainerFaction(fixture, rec, mod, $"JobTrainer{Skill.Alchemy}");
                AddTrainerScript(rec);
                mod.Classes.Add(plainClass);
                rec.Class.SetTo(plainClass);
                SetExplicitSkillLevel(rec, Skill.Alchemy, 25);
            },
            prepForFix: (rec, mod) =>
            {
                AddTrainerClass(fixture, rec, mod, Skill.Alchemy);
            },
            TrainerAnalyzer.TrainerMissingClass);
    }

    // A non auto calc trainer with a too low skill level is reported
    [Theory, MutagenModAutoData]
    public void LowExplicitSkillLevel(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                AddTrainerFaction(fixture, rec, mod, $"JobTrainer{Skill.Alchemy}");
                AddTrainerScript(rec);
                AddTrainerClass(fixture, rec, mod, Skill.Alchemy);
                SetExplicitSkillLevel(rec, Skill.Alchemy, 10);
            },
            prepForFix: (rec, mod) =>
            {
                SetExplicitSkillLevel(rec, Skill.Alchemy, 25);
            },
            TrainerAnalyzer.LowSkillLevel);
    }

    // An auto calc trainer that does not reach a sufficient skill level at their minimum level is reported
    [Theory, MutagenModAutoData]
    public void LowAutoCalcSkillLevel(Fixture fixture)
    {
        fixture.Run(
            prepForError: (rec, mod) =>
            {
                AddTrainerFaction(fixture, rec, mod, $"JobTrainer{Skill.Alchemy}");
                AddTrainerScript(rec);
                AddTrainerClass(fixture, rec, mod, Skill.Alchemy);
                SetAutoCalcStats(fixture, rec, mod, minimumLevel: 1);
            },
            prepForFix: (rec, mod) =>
            {
                rec.Configuration.Level = new NpcLevel { Level = 4 };
            },
            TrainerAnalyzer.LowSkillLevelAutoCalc);
    }

    // A fully set up trainer is not reported
    [Theory, MutagenModAutoData]
    public void ValidTrainer(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            AddTrainerFaction(fixture, rec, mod, $"JobTrainer{Skill.Alchemy}");
            AddTrainerScript(rec);
            AddTrainerClass(fixture, rec, mod, Skill.Alchemy);
            SetExplicitSkillLevel(rec, Skill.Alchemy, 25);
        });
    }

    // The marksman faction specialization is treated as archery
    [Theory, MutagenModAutoData]
    public void MarksmanFactionCountsAsArcherySpecialization(Fixture fixture)
    {
        fixture.RunShouldBeNoError((rec, mod) =>
        {
            AddTrainerFaction(fixture, rec, mod, "JobTrainerMarksman");
            AddTrainerScript(rec);
            AddTrainerClass(fixture, rec, mod, Skill.Archery);
            SetExplicitSkillLevel(rec, Skill.Archery, 25);
        });
    }
}
