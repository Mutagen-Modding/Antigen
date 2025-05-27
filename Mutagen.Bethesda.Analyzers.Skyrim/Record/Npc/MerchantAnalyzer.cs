using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class MerchantAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition MerchantWithoutSpecialization = MutagenTopicBuilder.FromDiscussion(
            200,
            "Merchant without specialization",
            Severity.Warning)
        .WithoutFormatting("Merchant npc does not have a specialized merchant faction");

    public IEnumerable<TopicDefinition> Topics { get; } = [MerchantWithoutSpecialization];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        var isMerchant = npc.HasFaction(param.LinkCache, editorId =>
            editorId is not null && editorId.Contains("JobMerchant", StringComparison.OrdinalIgnoreCase));
        if (!isMerchant) return;

        var hasSpecialization = npc.HasFaction(param.LinkCache, editorId =>
            editorId is not null
            && !editorId.Contains("JobMerchant", StringComparison.OrdinalIgnoreCase)
            && !editorId.Contains("JobTrainer", StringComparison.OrdinalIgnoreCase)
            && editorId.Contains("Job", StringComparison.OrdinalIgnoreCase));

        if (hasSpecialization) return;

        param.AddTopic(
            MerchantWithoutSpecialization.Format());
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Factions;
    }
}
