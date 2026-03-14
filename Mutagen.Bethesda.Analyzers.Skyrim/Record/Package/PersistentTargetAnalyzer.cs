using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Package;

public class PersistentTargetAnalyzer : IContextualRecordAnalyzer<IPackageGetter>
{
    public static readonly TopicDefinition<string> NonPersistentTarget = MutagenTopicBuilder.FromDiscussion(
            534,
            "Package Target Should Be Persistent",
            Severity.Error)
        .WithFormatting<string>("Package data '{0}' targets a placed reference that should be marked as persistent");

    public IEnumerable<TopicDefinition> Topics { get; } = [NonPersistentTarget];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPackageGetter> param)
    {
        var package = param.Record;

        foreach (var (key, data) in package.Data)
        {
            IFormLinkGetter? targetLink = data switch
            {
                IPackageDataLocationGetter packageDataLocation => packageDataLocation.Location.Target switch
                {
                    ILocationTargetGetter locationTarget => locationTarget.Link,
                    _ => null
                },
                IPackageDataTargetGetter dataTarget => dataTarget.Target switch
                {
                    IPackageTargetSpecificReferenceGetter packageTargetSpecificReference => packageTargetSpecificReference.Reference,
                    _ => null
                },
                _ => null
            };

            if (targetLink is null || targetLink.IsNull) continue;

            // Try to resolve as a placed reference
            if (!param.LinkCache.TryResolve<IPlacedGetter>(targetLink.FormKey, out var placed)) continue;

            // Check if it's persistent
            if (placed.SkyrimMajorRecordFlags.HasFlag((SkyrimMajorRecord.SkyrimMajorRecordFlag)PlacedObject.DefaultMajorFlag.Persistent)) continue;

            param.AddTopic(
                NonPersistentTarget.Format(package.GetPackageDataName(key, param.LinkCache) ?? key.ToString()));
        }
    }

    public IEnumerable<Func<IPackageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Data;
    }
}
