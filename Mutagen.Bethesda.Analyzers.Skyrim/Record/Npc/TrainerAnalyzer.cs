using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
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

    public IEnumerable<TopicDefinition> Topics { get; } = [TrainerFactionMissingScript, TrainerScriptMissingFaction, TrainerWithoutSpecialization];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.Template.IsNull) return;

        var faction = npc.Factions
            .Select(x => x.Faction.TryResolve(param.LinkCache))
            .WhereNotNull()
            .ToList();

        var hasTrainerGoldScript = npc.VirtualMachineAdapter is not null && npc.HasScript("TrainerGoldScript");
        var trainerFaction = faction.Find(x => x.EditorID?.Contains("JobTrainer", StringComparison.OrdinalIgnoreCase) ?? false);

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
            var hasTrainerSpecialization = faction.Exists(f =>
                f.EditorID is not null
                && !f.EditorID.EndsWith("JobTrainer", StringComparison.OrdinalIgnoreCase)
                && f.EditorID.Contains("JobTrainer", StringComparison.OrdinalIgnoreCase));

            if (!hasTrainerSpecialization)
            {
                param.AddTopic(
                    TrainerWithoutSpecialization.Format());
            }
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Template;
        yield return x => x.Factions;
        yield return x => x.VirtualMachineAdapter;
    }
}
