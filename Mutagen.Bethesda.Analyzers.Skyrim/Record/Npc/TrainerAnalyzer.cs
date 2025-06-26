using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class TrainerAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition TrainerFactionMissingScript = MutagenTopicBuilder.FromDiscussion(
            196,
            "Trainer requires TrainerGoldScript",
            Severity.Warning)
        .WithoutFormatting("Trainer npc does not have a TrainerGoldScript");

    public static readonly TopicDefinition TrainerScriptMissingFaction = MutagenTopicBuilder.FromDiscussion(
            197,
            "Trainer requires trainer faction",
            Severity.Warning)
        .WithoutFormatting("Trainer npc is not in a trainer faction");

    public static readonly TopicDefinition TrainerWithoutSpecialization = MutagenTopicBuilder.FromDiscussion(
            198,
            "Trainer without specialization",
            Severity.Warning)
        .WithoutFormatting("Trainer npc does not have a specialized trainer faction");

    public static readonly TopicDefinition<Skill, int> LowSkillLevel = MutagenTopicBuilder.FromDiscussion(
            357,
            "Trainer npc has low skill level",
            Severity.Warning)
        .WithFormatting<Skill, int>("Npc is a {0} trainer but only has their {0} skill set to {1}");

    public static readonly TopicDefinition<Skill, int, IFormLinkGetter<IClassGetter>, int> LowSkillLevelAutoCalc = MutagenTopicBuilder.FromDiscussion(
            199,
            "Trainer npc auto calculated skill level too low",
            Severity.Warning)
        .WithFormatting<Skill, int, IFormLinkGetter<IClassGetter>, int>("Npc is {0} trainer but only reaches {0} {1} with class {2}, at their minimum npc level {3}");

    public IEnumerable<TopicDefinition> Topics { get; } = [TrainerFactionMissingScript, TrainerScriptMissingFaction, TrainerWithoutSpecialization, LowSkillLevel, LowSkillLevelAutoCalc];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.Template.IsNull) return;

        var factions = npc.Factions
            .Select(x => x.Faction.TryResolve(param.LinkCache))
            .WhereNotNull()
            .ToList();

        var hasTrainerGoldScript = npc.VirtualMachineAdapter is not null && npc.HasScript("TrainerGoldScript");
        var trainerFaction = factions.Find(x => x.EditorID?.Contains("JobTrainer", StringComparison.OrdinalIgnoreCase) ?? false);

        if (hasTrainerGoldScript && trainerFaction is null)
        {
            param.AddTopic(
                TrainerScriptMissingFaction.Format());
        }

        if (trainerFaction is not null && !hasTrainerGoldScript)
        {
            param.AddTopic(
                TrainerFactionMissingScript.Format());
        }

        if (hasTrainerGoldScript || trainerFaction is not null)
        {
            var trainerType = npc.GetTrainerType(param.LinkCache);

            if (trainerType is null)
            {
                param.AddTopic(
                    TrainerWithoutSpecialization.Format());
            }
            else
            {
                var minimumSkillLevel = npc.GetMinimumSkillLevel(trainerType.Value, param.LinkCache);
                if (minimumSkillLevel < 25)
                {
                    if (npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.AutoCalcStats))
                    {
                        param.AddTopic(
                            LowSkillLevelAutoCalc.Format(
                                trainerType.Value,
                                minimumSkillLevel,
                                npc.Class,
                                npc.Configuration.CalcMinLevel));
                    }
                    else
                    {
                        param.AddTopic(
                            LowSkillLevel.Format(
                                trainerType.Value,
                                minimumSkillLevel));
                    }
                }
            }
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Template;
        yield return x => x.Factions;
        yield return x => x.Class;
        yield return x => x.PlayerSkills;
        yield return x => x.VirtualMachineAdapter;
        yield return x => x.Configuration.Level;
        yield return x => x.Configuration.CalcMinLevel;
        yield return x => x.Configuration.Flags;
    }
}
