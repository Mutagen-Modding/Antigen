using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record;

public class LinkAnalyzer : IContextualRecordAnalyzer<ISkyrimMajorRecordGetter>
{
    public static readonly TopicDefinition<IFormLinkGetter, Type> InvalidLink = MutagenTopicBuilder.FromDiscussion(
            582,
            "Invalid link",
            Severity.Error)
        .WithFormatting<IFormLinkGetter, Type>("Link {0} cannot be resolved as {1}");

    public IEnumerable<TopicDefinition> Topics => [InvalidLink];

    private static readonly Dictionary<FormKey, Type> HardcodedLinks = new List<IFormLinkGetter>([
        FormKeys.SkyrimSE.Skyrim.BodyPartData.PlayerBodyPartData,
        FormKeys.SkyrimSE.Skyrim.ImageSpaceAdapter.ImageSpaceConcussion,
        FormKeys.SkyrimSE.Skyrim.ImageSpaceAdapter.ExplosionInFace,
        FormKeys.SkyrimSE.Skyrim.ImpactDataSet.DefaultImpactDataSet,
        FormKeys.SkyrimSE.Skyrim.PlayerRef,
        FormKeys.SkyrimSE.Skyrim.TextureSet.NullTextureSet
    ]).ToDictionary(l => l.FormKey, l => l.Type);

    static bool CanResolveLink(IFormLinkGetter link, ILinkCache linkCache)
    {
        // Some records are generated at runtime, but not present in Skyrim.esm
        // Links to them must still be of the correct type
        if (HardcodedLinks.TryGetValue(link.FormKey, out var targetType))
        {
            return link.Type.IsAssignableFrom(targetType);
        }

        return linkCache.TryResolve(link.FormKey, link.Type, out var _);
    }

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ISkyrimMajorRecordGetter> param)
    {
        var record = param.Record;

        foreach (var link in record.EnumerateFormLinks(iterateNestedRecords: false))
        {
            // Mutagen doesn't provide enough context to know if a null link is valid
            if (link.IsNull)
                continue;

            if (!CanResolveLink(link, param.LinkCache))
            {
                param.AddTopic(InvalidLink.Format(link, link.Type));
            }
        }
    }

    public IEnumerable<Func<ISkyrimMajorRecordGetter, object?>> FieldsOfInterest()
    {
        yield return x => x;
    }
}
