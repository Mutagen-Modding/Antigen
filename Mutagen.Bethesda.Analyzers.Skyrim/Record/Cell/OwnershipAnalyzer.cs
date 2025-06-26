using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell;

public class OwnershipAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<IPlacedGetter, IFormLinkNullableGetter<IOwnerGetter>> RedundantOwnership = MutagenTopicBuilder.FromDiscussion(
            366,
            "Redundant Ownership",
            Severity.Suggestion)
        .WithFormatting<IPlacedGetter, IFormLinkNullableGetter<IOwnerGetter>>("Placed {0} has the same owner {1} as the cell");

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;
        if (cell.Owner.IsNull) return;

        foreach (var placed in cell.GetAllPlaced(param.LinkCache))
        {
            var owner = placed switch
            {
                IAPlacedTrapGetter trap => trap.Owner,
                IPlacedNpcGetter npc => npc.Owner,
                IPlacedObjectGetter obj => obj.Owner,
                _ => null
            };

            if (owner is null || owner.IsNull) continue;
            if (owner.FormKey != cell.Owner.FormKey) continue;

            switch (placed) {
                case IPlacedNpcGetter or IPlacedTrapGetter:
                    param.AddTopic(
                        RedundantOwnership.Format(placed, cell.Owner));
                    break;
                case IPlacedObjectGetter obj:
                {
                    var baseObject = obj.Base.TryResolve(param.LinkCache);
                    if (baseObject is null) continue;

                    // These base objects actually have an effect when they set ownership again explicitly
                    switch (baseObject)
                    {
                        case IActivatorGetter:
                        case IContainerGetter:
                        case IDoorGetter:
                        case IFurnitureGetter:
                        case IIdleMarkerGetter:
                            continue;
                    }

                    // The rest should be reported
                    param.AddTopic(
                        RedundantOwnership.Format(placed, cell.Owner));

                    break;
                }
            }
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Temporary;
        yield return x => x.Persistent;
    }

    public IEnumerable<TopicDefinition> Topics => RedundantOwnership.AsEnumerable();
}
