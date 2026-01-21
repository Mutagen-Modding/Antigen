using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Location;

class CircularLocationAnalyzer : IContextualRecordAnalyzer<ILocationGetter>
{
    public static readonly TopicDefinition CircularLocation = MutagenTopicBuilder.FromDiscussion(
            175,
            "Circular Location",
            Severity.CTD)
        .WithoutFormatting("Location is its own parent location");

    IEnumerable<TopicDefinition> IAnalyzer.Topics { get; } = [CircularLocation];

    private static IEnumerable<ILocationGetter> GetParentLocations(ILocationGetter currentLocation, ILinkCache linkCache)
    {
        while (currentLocation is { ParentLocation.IsNull: false })
        {
            if (linkCache.TryResolve<ILocationGetter>(currentLocation.ParentLocation.FormKey, out var parentLocation))
            {
                yield return parentLocation;
                currentLocation = parentLocation;
            }
            else
            {
                yield break;
            }
        }
    }

    void IContextualRecordAnalyzer<ILocationGetter>.AnalyzeRecord(ContextualRecordAnalyzerParams<ILocationGetter> param)
    {
        var link = param.Record.ToLink();
        HashSet<IFormLinkGetter> sequence = [link];
        bool circular = GetParentLocations(param.Record, param.LinkCache)
            .Any(parentLocation => !sequence.Add(parentLocation.ToLink()));

        if (circular)
        {
            var parentsList = sequence.Except([link]).ToList();
            parentsList.Add(link);
            param.AddTopic(CircularLocation.Format(), ("Parent Locations", parentsList));
        }
    }
     
    IEnumerable<Func<ILocationGetter, object?>> IContextualRecordAnalyzer<ILocationGetter>.FieldsOfInterest()
    {
        yield return x => x.ParentLocation;
    }
}

