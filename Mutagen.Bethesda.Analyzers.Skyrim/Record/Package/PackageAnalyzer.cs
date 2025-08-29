using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Package;

public class PackageAnalyzer : IContextualRecordAnalyzer<IPackageGetter>
{
    public static readonly TopicDefinition<string> PackageWithoutOwningQuestReferencingQuestAlias = MutagenTopicBuilder.FromDiscussion(
            488,
            "Package Without Owning Quest Referencing Quest Alias",
            Severity.Error)
        .WithFormatting<string>("Package without owning quest data {0} references quest alias");

    public static readonly TopicDefinition<string, IQuestGetter> PackageReferencingMissingQuestAlias = MutagenTopicBuilder.FromDiscussion(
            489,
            "Package Referencing Missing Quest Alias",
            Severity.Error)
        .WithFormatting<string, IQuestGetter>("Package data {0} references quest alias missing in quest {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [PackageWithoutOwningQuestReferencingQuestAlias, PackageReferencingMissingQuestAlias];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPackageGetter> param)
    {
        var package = param.Record;
        if (package.IsDeleted) return;

        foreach (var (key, data) in package.Data)
        {
            if (data is not IPackageDataTargetGetter dataTarget) continue;
            if (dataTarget.Target is not IPackageTargetAliasGetter targetAlias) continue;

            if (package.OwnerQuest is null)
            {
                param.AddTopic(
                    PackageWithoutOwningQuestReferencingQuestAlias.Format(package.GetPackageDataName(key, param.LinkCache) ?? key.ToString()));
            }
            else
            {
                var quest = package.OwnerQuest.TryResolve(param.LinkCache);
                if (quest is null) continue;
                if (quest.HasAlias((uint)targetAlias.Alias)) continue;

                param.AddTopic(
                    PackageReferencingMissingQuestAlias.Format(package.GetPackageDataName(key, param.LinkCache) ?? key.ToString(), quest));
            }
        }
    }

    public IEnumerable<Func<IPackageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.OwnerQuest;
        yield return x => x.Data;
    }
}
