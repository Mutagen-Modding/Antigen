using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Placed.Npc;

public class ScaleAnalyzer : IContextualRecordAnalyzer<IPlacedNpcGetter>
{
    public static readonly TopicDefinition<IFormLinkNullableGetter<INpcGetter>, float?> ScaleNotOne = MutagenTopicBuilder.FromDiscussion(
            486,
            "Placed NPC Scale Not One",
            Severity.Warning)
        .WithFormatting<IFormLinkNullableGetter<INpcGetter>, float?>("Npc placement {0} at  with scale {1} is not one");

    public IEnumerable<TopicDefinition> Topics { get; } = [ScaleNotOne];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPlacedNpcGetter> param)
    {
        var placedNpc = param.Record;

        if (placedNpc.IsDeleted) return;

        var scaleNullable = placedNpc.Scale;
        if (scaleNullable is null) return;

        var scale = scaleNullable.Value;

        if (scale.EqualsWithin(1))
        {
            param.AddTopic(
                ScaleNotOne.Format(placedNpc.Base, placedNpc.Scale));
        }
    }

    public IEnumerable<Func<IPlacedNpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Scale;
        yield return x => x.Base;
    }
}
