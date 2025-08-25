using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class OwnershipAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition<IFactionGetter> InvalidOwner = MutagenTopicBuilder.FromDiscussion(
            487,
            "Invalid Faction Owner",
            Severity.Warning)
        .WithFormatting<IFactionGetter>("Placed record is owned by faction {0} that doesn't have the 'Can Be Owner' flag set");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidOwner];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        if (placedObject.Owner.IsNull) return;

        var owner = placedObject.Owner.TryResolve(param.LinkCache);
        if (owner is null) return;

        switch (owner) {
            case IFactionGetter faction:
            {
                if (!faction.Flags.HasFlag(Bethesda.Skyrim.Faction.FactionFlag.CanBeOwner))
                {
                    param.AddTopic(
                        InvalidOwner.Format(faction));
                }
                break;
            }
            case INpcGetter:
                break;
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Owner;
    }
}
