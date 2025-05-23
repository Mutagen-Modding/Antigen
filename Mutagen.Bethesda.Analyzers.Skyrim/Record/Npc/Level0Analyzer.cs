using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class Level0Analyzer : IIsolatedRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition Level0 = MutagenTopicBuilder.FromDiscussion(
            201,
            "Level 0",
            Severity.Warning)
        .WithoutFormatting("Npc has level 0");


    public static readonly TopicDefinition MinLevel0 = MutagenTopicBuilder.FromDiscussion(
            201,
            "Minimum Level 0",
            Severity.Warning)
        .WithoutFormatting("Npc has minimum level 0");

    public static readonly TopicDefinition<float> LevelMultTooSmall = MutagenTopicBuilder.FromDiscussion(
            202,
            "Level Mult Too Small",
            Severity.Warning)
        .WithFormatting<float>("Npc has too small level mult {0}");


    public IEnumerable<TopicDefinition> Topics { get; } = [Level0];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        switch (npc.Configuration.Level)
        {
            case INpcLevelGetter npcLevel:
                if (npcLevel.Level == 0)
                {
                    param.AddTopic(Level0.Format());
                }
                break;
            case IPcLevelMultGetter pcLevelMult:
                if (Math.Abs(pcLevelMult.LevelMult) < 0.001)
                {
                    param.AddTopic(LevelMultTooSmall.Format(pcLevelMult.LevelMult));
                }

                if (npc.Configuration.CalcMinLevel == 0)
                {
                    param.AddTopic(MinLevel0.Format());
                }
                break;
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Configuration.Level;
    }
}
