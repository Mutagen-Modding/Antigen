using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class ChildLowConfidenceAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition ChildLowConfidence = MutagenTopicBuilder.FromDiscussion(
            241,
            "Child low confidence",
            Severity.Suggestion)
        .WithoutFormatting("Child  doesn't have low confidence");

    public IEnumerable<TopicDefinition> Topics { get; } = [ChildLowConfidence];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.Race.TryResolve(param.LinkCache, out var race)) return;
        if (!race.IsChildRace()) return;

        if (npc.AIData.Confidence is Confidence.Brave or Confidence.Foolhardy)
        {
            param.AddTopic(
                ChildLowConfidence.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.MajorFlags;
        yield return x => x.AIData.Confidence;
        yield return x => x.Race;
    }
}
