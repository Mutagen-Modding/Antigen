using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Object;

public class ChildOwnershipAnalyzer : IContextualRecordAnalyzer<IPlacedObjectGetter>
{
    public static readonly TopicDefinition<IPlaceableObjectGetter> InvalidChildOwner = MutagenTopicBuilder.FromDiscussion(
            382,
            "Invalid Child Owner",
            Severity.Error)
        .WithFormatting<IPlaceableObjectGetter>("Placed object is owned by a child, but it places a {0} which cannot be used by children");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidChildOwner];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedObjectGetter> param)
    {
        var placedObject = param.Record;

        if (placedObject.Owner.IsNull) return;

        var npcOwner = placedObject.Owner.TryResolve<INpcGetter>(param.LinkCache);
        var race = npcOwner?.Race.TryResolve(param.LinkCache);
        if (race is null) return;

        if (!race.IsChildRace()) return;

        TestChildCanUse<IActivatorGetter>(placedObject);
        TestChildCanUse<IIdleMarkerGetter>(placedObject);
        TestChildCanUse<IFurnitureGetter>(placedObject);

        void TestChildCanUse<TRecord>(IPlacedObjectGetter placed) where TRecord : class, IPlaceableObjectGetter
        {
            var record = placed.Base.TryResolve<TRecord>(param.LinkCache);
            if (record is null) return;

            // Activators, Idle Markers, and Furniture all use the same flag value to determine if children can use them
            if (!((Bethesda.Skyrim.Activator.MajorFlag)record.MajorRecordFlagsRaw).HasFlag(Bethesda.Skyrim.Activator.MajorFlag.ChildCanUse))
            {
                param.AddTopic(
                    InvalidChildOwner.Format(record));
            }
        }
    }

    public IEnumerable<Func<IPlacedObjectGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Base;
        yield return x => x.Owner;
    }
}
