using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Npc;

public class UniquePlacedNpcAnalyzer : IContextualRecordAnalyzer<IPlacedNpcGetter>
{
    public static readonly TopicDefinition UniqueNpcNotInPersistenceLocation = MutagenTopicBuilder.FromDiscussion(
            285,
            "Unique Npc not in Persistence Location",
            Severity.Error)
        .WithoutFormatting("Placed Npcs should be placed in persistence location, otherwise they might not be loaded");

    public static readonly TopicDefinition UniqueNpcWithoutPersistenceLocation = MutagenTopicBuilder.FromDiscussion(
            345,
            "Unique Npc without Persistence Location",
            Severity.Error)
        .WithoutFormatting("Placed Npcs should have a persistence location if the Npc is unique, excludes always persistent npcs or initially disabled npcs");

    public IEnumerable<TopicDefinition> Topics { get; } = [];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedNpcGetter> param)
    {
        var placedNpc = param.Record;

        if (placedNpc.IsDeleted) return;
        if (placedNpc.MajorFlags.HasFlag(PlacedNpc.MajorFlag.InitiallyDisabled)) return;
        if (placedNpc.MajorFlags.HasFlag(PlacedNpc.MajorFlag.StartsDead)) return;

        if (!placedNpc.Base.TryResolveSimpleContext(param.LinkCache, out var context)) return;

        var npc = context.Record;
        if (!npc.IsUnique()) return;

        if (context.Parent?.Record is ICellGetter cell)
        {
            var persistLocation = placedNpc.PersistentLocation.TryResolve(param.LinkCache);
            if (persistLocation is not null
                && cell.GetAllLocations(param.LinkCache)
                    .Select(location => location.FormKey)
                    .Contains(persistLocation.FormKey))
            {
                param.AddTopic(
                    UniqueNpcNotInPersistenceLocation.Format());
            }
        }

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
