using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Static;

public class LODAnalyzer : IIsolatedRecordAnalyzer<IStaticGetter>
{
    public static readonly TopicDefinition<int> FullModelInLod = MutagenTopicBuilder.FromDiscussion(
            595,
            "Full Model used as LOD",
            Severity.Suggestion)
        .WithFormatting<int>("Static has full model used as LOD{0}, consider creating a simplified LOD model for better performance");

    public IEnumerable<TopicDefinition> Topics { get; } = [FullModelInLod];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IStaticGetter> param)
    {
        var @static = param.Record;
        if (@static.Lod is null) return;
        if (@static.Model?.File is not {} file) return;

        if (@static.Lod.Level0.Equals(file))
        {
            param.AddTopic(
                FullModelInLod.Format(0));
        }

        if (@static.Lod.Level1.Equals(file))
        {
            param.AddTopic(
                FullModelInLod.Format(1));
        }

        if (@static.Lod.Level2.Equals(file))
        {
            param.AddTopic(
                FullModelInLod.Format(2));
        }

        if (@static.Lod.Level3.Equals(file))
        {
            param.AddTopic(
                FullModelInLod.Format(3));
        }
    }

    public IEnumerable<Func<IStaticGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Lod;
    }
}
