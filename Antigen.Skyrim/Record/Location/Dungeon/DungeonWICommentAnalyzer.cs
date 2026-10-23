using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Antigen.Skyrim.Record.Location.Dungeon;

public sealed class DungeonWICommentAnalyzer : IContextualRecordAnalyzer<ILocationGetter>
{
    public static readonly TopicDefinition HasNoWICommentTrigger = MutagenTopicBuilder.FromDiscussion(
            639,
            "Dungeon has no WI Comment Trigger",
            Severity.Suggestion)
        .WithoutFormatting("No dungeon location cell has a WI Comment Trigger activator for a follower to speak a comment in the dungeon");

    public IEnumerable<TopicDefinition> Topics { get; } = [HasNoWICommentTrigger];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ILocationGetter> param)
    {
        var location = param.Record;
        if (!location.IsDungeonLocation()) return;

        var usageCache = param.ResolveCache<ILinkUsageCache>();
        var dungeonCells = usageCache.GetUsagesOf<ICellGetter>(location).UsageLinks
            .Select(cell => cell.TryResolve<ICellGetter>(param.LinkCache))
            .WhereNotNull()
            .Where(cell => cell.Location.FormKey == location.FormKey);

        const string wiCommentTriggerScript = "WICommentTriggerScript";
        var anyCommentTrigger = dungeonCells
            .SelectMany(cell => cell.GetAllPlaced(param.LinkCache))
            .OfType<IPlacedObjectGetter>()
            // Only check trigger boxes
            .Where(placedObject => placedObject.Primitive is not null)
            .Any(placedObject =>
            {
                // Check if the script is on the placed object directly
                if (placedObject.HasScript(wiCommentTriggerScript)) return true;

                // Check if the script is on the base activator
                var activator = placedObject.Base.TryResolve<IActivatorGetter>(param.LinkCache);
                return activator?.HasScript(wiCommentTriggerScript) ?? false;
            });

        if (!anyCommentTrigger)
        {
            param.AddTopic(HasNoWICommentTrigger.Format());
        }
    }

    public IEnumerable<Func<ILocationGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Keywords;
    }
}
