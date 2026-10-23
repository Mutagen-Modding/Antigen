using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Antigen.Skyrim.Record.Npc;

public sealed class TrainerAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public const string JobTrainerEditorIDPart = "JobTrainer";

    public static readonly TopicDefinition TrainerMissingScript = MutagenTopicBuilder.FromDiscussion(
            196,
            "Trainer requires TrainerGoldScript",
            Severity.Warning)
        .WithoutFormatting("Trainer npc does not have a TrainerGoldScript");

    public static readonly TopicDefinition TrainerMissingFaction = MutagenTopicBuilder.FromDiscussion(
            197,
            "Trainer requires trainer faction",
            Severity.Warning)
        .WithoutFormatting("Trainer npc is not in a trainer faction");

    public static readonly TopicDefinition<IFormLinkGetter<IClassGetter>> TrainerMissingClass = MutagenTopicBuilder.FromDiscussion(
            665,
            "Trainer requires trainer class",
            Severity.Error)
        .WithFormatting<IFormLinkGetter<IClassGetter>>("Trainer npc has {0} which is not a valid trainer class");

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

    public static readonly TopicDefinition<string, int, IFormLinkGetter<IClassGetter>, int> LowSkillLevelAutoCalc = MutagenTopicBuilder.FromDiscussion(
            199,
            "Trainer npc auto calculated skill level too low",
            Severity.Warning)
        .WithFormatting<string, int, IFormLinkGetter<IClassGetter>, int>("Npc is {0} trainer but only reaches skill level {1} with class {2}, at their minimum npc level {3}");

    public IEnumerable<TopicDefinition> Topics { get; } = [TrainerMissingScript, TrainerMissingFaction, TrainerWithoutSpecialization, LowSkillLevel, LowSkillLevelAutoCalc, TrainerMissingClass];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.Template.IsNull) return;

        var factions = npc.Factions
            .Select(x => x.Faction.TryResolve(param.LinkCache))
            .WhereNotNull()
            .ToList();

        var hasTrainerGoldScript = npc.VirtualMachineAdapter is not null && npc.HasScript("TrainerGoldScript");
        var trainerFaction = factions.Find(x => x.EditorID?.Contains(JobTrainerEditorIDPart, StringComparison.OrdinalIgnoreCase) ?? false);
        var hasTrainerFaction = trainerFaction is not null;
        var trainerClass = npc.Class.TryResolve(param.LinkCache);

        // Some classes are randomly set as trainer class without actually being a trainer class
        // We check for the EditorID containing "Trainer" to filter out those cases
        var hasTrainerClass = trainerClass is { MaxTrainingLevel: > 0, EditorID: {} classEditorID } && classEditorID.Contains("Trainer");

        if ((hasTrainerGoldScript || hasTrainerClass) && trainerFaction is null)
        {
            param.AddTopic(
                TrainerMissingFaction.Format());
        }

        if ((hasTrainerFaction || hasTrainerClass) && !hasTrainerGoldScript)
        {
            param.AddTopic(
                TrainerMissingScript.Format());
        }

        if (hasTrainerGoldScript || hasTrainerFaction || hasTrainerClass)
        {
            var factionSpecialization = factions
                .Select<IFactionGetter, Skill?>(f =>
                {
                    if (f.EditorID is null) return null;

                    var index = f.EditorID.IndexOf(JobTrainerEditorIDPart, StringComparison.OrdinalIgnoreCase);
                    if (index < 0) return null;

                    // If the EditorID ends with "JobTrainer", it is not a specialization
                    if (index + JobTrainerEditorIDPart.Length >= f.EditorID.Length) return null;

                    // Extract the specialization part
                    var specialization = f.EditorID[(index + JobTrainerEditorIDPart.Length)..];
                    specialization = specialization.TrimStringFromEnd("Faction");
                    if (string.Equals(specialization, "Marksman", StringComparison.OrdinalIgnoreCase)) return Skill.Archery; // Handle marksman synonym for archery
                    if (Enum.TryParse<Skill>(specialization, out var actorValue)) return actorValue;

                    return null;
                })
                .WhereNotNull()
                .FirstOrDefault();

            if (factionSpecialization == 0)
            {
                param.AddTopic(
                    TrainerWithoutSpecialization.Format());
            }

            var trainerType = npc.GetTrainerType(param.LinkCache);
            if (trainerType is null)
            {
                param.AddTopic(
                    TrainerMissingClass.Format(npc.Class));
            }
            else
            {
                var minimumLevel = npc.GetMinimumLevel();
                var minimumSkillLevel = npc.GetSkillLevel(trainerType.Value, minimumLevel, param.LinkCache);
                if (minimumSkillLevel >= 25) return;

                if (npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.AutoCalcStats))
                {
                    param.AddTopic(
                        LowSkillLevelAutoCalc.Format(
                            trainerType.Value.ToString(),
                            minimumSkillLevel,
                            npc.Class,
                            minimumLevel));
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
