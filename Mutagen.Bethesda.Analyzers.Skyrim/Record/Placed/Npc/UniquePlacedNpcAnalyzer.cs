using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Npc;

public class UniquePlacedNpcAnalyzer : IContextualRecordAnalyzer<IPlacedNpcGetter>
{
    public static readonly TopicDefinition UniqueNpcWithoutPersistenceLocation = MutagenTopicBuilder.FromDiscussion(
            345,
            "Unique Npc without Persistence Location",
            Severity.Error)
        .WithoutFormatting("Placed Npcs should have a persistence location if the Npc is unique, excludes always persistent npcs or initially disabled npcs");

    public IEnumerable<TopicDefinition> Topics { get; } = [UniqueNpcWithoutPersistenceLocation];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedNpcGetter> param)
    {
        var placedNpc = param.Record;
        if (placedNpc.MajorFlags.HasFlag(PlacedNpc.MajorFlag.InitiallyDisabled)) return;
        if (placedNpc.MajorFlags.HasFlag(PlacedNpc.MajorFlag.StartsDead)) return;

        if (!placedNpc.Base.TryResolve(param.LinkCache, out var npc)) return;
        if (!npc.IsUnique()) return;

        if (placedNpc.PersistentLocation.IsNull)
        {
            param.AddTopic(
                UniqueNpcWithoutPersistenceLocation.Format());
        }
    }

    public IEnumerable<Func<IPlacedNpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.MajorFlags;
        yield return x => x.Base;
        yield return x => x.PersistentLocation;
    }
}
