using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Location;

class CircularLocationAnalyzer : IContextualRecordAnalyzer<ILocationGetter>
{
    public static readonly TopicDefinition CircularLocation = MutagenTopicBuilder.DevelopmentTopic(
            "Circular Location",
            Severity.CTD)
        .WithoutFormatting("Location is its own parent location - this leads to CTDs and infinite loading screens");

    IEnumerable<TopicDefinition> IAnalyzer.Topics { get; } = [CircularLocation];

    private static bool FindCicularLocation(ILocationGetter sourceLocation, ILocationGetter? currentLocation, ILinkCache linkCache)
    {
        if (currentLocation == null) return false;

        if (currentLocation.FormKey == sourceLocation.FormKey) return true;

        if (currentLocation.ParentLocation.IsNull) return false;

        if (!linkCache.TryResolve<ILocationGetter>(currentLocation.ParentLocation.FormKey, out var parentLocation)) return false;

        return FindCicularLocation(sourceLocation, parentLocation, linkCache);
    }

    void IContextualRecordAnalyzer<ILocationGetter>.AnalyzeRecord(ContextualRecordAnalyzerParams<ILocationGetter> param)
    {
        var location = param.Record;

        if (location.ParentLocation.IsNull) return;

        if (!param.LinkCache.TryResolve<ILocationGetter>(location.ParentLocation.FormKey, out var parentLocation)) return;

        if (FindCicularLocation(location, parentLocation, param.LinkCache))
        {
            param.AddTopic(CircularLocation.Format());
        }
    }
     
    IEnumerable<Func<ILocationGetter, object?>> IContextualRecordAnalyzer<ILocationGetter>.FieldsOfInterest()
    {
        yield return x => x.ParentLocation;
    }
}

