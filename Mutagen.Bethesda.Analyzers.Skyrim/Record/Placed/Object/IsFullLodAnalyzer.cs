using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class IsFullLodAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition FullLodWithoutPersistence = MutagenTopicBuilder.FromDiscussion(
            475,
            "Full LOD Without Persistence",
            Severity.Error)
        .WithoutFormatting("Placed object has flag 'Is full LOD' but is not persistent");

    public IEnumerable<TopicDefinition> Topics { get; } = [FullLodWithoutPersistence];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        var flags = (PlacedObject.StaticMajorFlag)placedObject.SkyrimMajorRecordFlags;
        if (!flags.HasFlag(PlacedObject.StaticMajorFlag.IsFullLod)) return;

        var baseObject = placedObject.Base.TryResolve(param.LinkCache);
        if (baseObject is not (IActivatorGetter or IStaticGetter or ITreeGetter or IFloraGetter)) return;

        if (!flags.HasFlag(PlacedObject.StaticMajorFlag.Persistent))
        {
            param.AddTopic(
                FullLodWithoutPersistence.Format());
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.SkyrimMajorRecordFlags;
    }
}
