using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Location;

public class RefTypeDungeonAnalyzer : IContextualRecordAnalyzer<ILocationGetter>
{
    public static readonly TopicDefinition NoBossRefType = MutagenTopicBuilder.FromDiscussion(
            234,
            "No Boss",
            Severity.Suggestion)
        .WithoutFormatting("Dungeon location has no Boss Ref Type - not set up for radiant quests");

    public static readonly TopicDefinition NoBossContainerRefType = MutagenTopicBuilder.FromDiscussion(
            312,
            "No Boss Container",
            Severity.Suggestion)
        .WithoutFormatting("Dungeon location has no Boss Container Ref Type - not set up for radiant quests");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoBossRefType, NoBossContainerRefType];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ILocationGetter> param)
    {
        var location = param.Record;

        if (location.Keywords is null || location.Keywords.All(k => k.FormKey != FormKeys.SkyrimSE.Skyrim.Keyword.LocTypeDungeon.FormKey)) return;

        var referenceTypes = location.LocationRefTypeReference().ToList();

        if (!referenceTypes.Exists(staticRef => staticRef.LocationRefType.FormKey == FormKeys.SkyrimSE.Skyrim.LocationReferenceType.Boss.FormKey))
        {
            param.AddTopic(
                NoBossRefType.Format());
        }

        if (!referenceTypes.Exists(staticRef => staticRef.LocationRefType.FormKey == FormKeys.SkyrimSE.Skyrim.LocationReferenceType.BossContainer.FormKey))
        {
            param.AddTopic(
                NoBossContainerRefType.Format());
        }
    }

    public IEnumerable<Func<ILocationGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Keywords;
        yield return x => x.LocationRefTypeReferencesStatic;
        yield return x => x.LocationRefTypeReferencesAdded;
        yield return x => x.LocationRefTypeReferencesRemoved;
    }
}
