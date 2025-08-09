using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Relationship;

public class RelationshipTypeAnalyzer : IContextualRecordAnalyzer<IRelationshipGetter>
{
    public static readonly TopicDefinition<Bethesda.Skyrim.Relationship.RankType> SpouseRelationshipType = MutagenTopicBuilder.FromDiscussion(
            451,
            "Spouse with low relationship rank",
            Severity.Suggestion)
        .WithFormatting<Bethesda.Skyrim.Relationship.RankType>("Relationship type is Spouse, but their rank {0} is lower than Ally");

    public static readonly TopicDefinition<Bethesda.Skyrim.Relationship.RankType> CourtingRelationshipType = MutagenTopicBuilder.FromDiscussion(
            452,
            "Courting with low relationship rank",
            Severity.Suggestion)
        .WithFormatting<Bethesda.Skyrim.Relationship.RankType>("Relationship type is Courting, but their rank {0} is lower than Acquaintance");

    public IEnumerable<TopicDefinition> Topics { get; } = [CourtingRelationshipType];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IRelationshipGetter> param)
    {
        var relationship = param.Record;
        if (relationship.IsDeleted) return;

        if (relationship.AssociationType.FormKey == FormKeys.SkyrimSE.Skyrim.AssociationType.Spouse.FormKey
            && relationship.Rank > Bethesda.Skyrim.Relationship.RankType.Ally)
        {
            param.AddTopic(
                SpouseRelationshipType.Format(relationship.Rank));
        }

        if (relationship.AssociationType.FormKey == FormKeys.SkyrimSE.Skyrim.AssociationType.Courting.FormKey
            && relationship.Rank >= Bethesda.Skyrim.Relationship.RankType.Acquaintance)
        {
            param.AddTopic(
                CourtingRelationshipType.Format(relationship.Rank));
        }
    }

    public IEnumerable<Func<IRelationshipGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.AssociationType;
        yield return x => x.Rank;
    }
}
