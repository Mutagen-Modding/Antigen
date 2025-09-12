using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Package;

public class PackageDataAnalyzer : IContextualRecordAnalyzer<IPackageGetter>
{
    public static readonly TopicDefinition<string> PackageWithoutOwningQuestReferencingQuestAlias = MutagenTopicBuilder.FromDiscussion(
            488,
            "Package Without Owning Quest Referencing Quest Alias",
            Severity.Error)
        .WithFormatting<string>("Package without owning quest data '{0}' references quest alias");

    public static readonly TopicDefinition<string, IQuestGetter> PackageReferencingMissingQuestAlias = MutagenTopicBuilder.FromDiscussion(
            489,
            "Package Referencing Missing Quest Alias",
            Severity.Error)
        .WithFormatting<string, IQuestGetter>("Package data '{0}' references quest alias missing in quest {1}");

    public static readonly TopicDefinition<string> PackageTargetsNoObject = MutagenTopicBuilder.FromDiscussion(
            496,
            "Package Targets No Object",
            Severity.Warning)
        .WithFormatting<string>("Package data '{0}' targets no object");

    public static readonly TopicDefinition<string, string?> PackageDataDoesNotExist = MutagenTopicBuilder.FromDiscussion(
            497,
            "Referenced Package Data Index Missing",
            Severity.Error)
        .WithFormatting<string, string?>("Package data '{0}' referenced by {1} does not exist");

    public IEnumerable<TopicDefinition> Topics { get; } = [PackageWithoutOwningQuestReferencingQuestAlias, PackageReferencingMissingQuestAlias];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPackageGetter> param)
    {
        var package = param.Record;

        if (package.PackageTemplate.IsNull)
        {
            var existingIndices = package.Data.Keys.ToHashSet();
            foreach (var branch in package.ProcedureTree)
            {
                foreach (var index in branch.DataInputIndices)
                {
                    var sIndex = (sbyte)index;
                    if (sIndex != -1 && !existingIndices.Contains(sIndex))
                    {
                        param.AddTopic(
                            PackageDataDoesNotExist.Format(package.GetPackageDataName(sIndex, param.LinkCache) ?? sIndex.ToString(), branch.ProcedureType));
                    }
                }
            }
        }

        foreach (var (key, data) in package.Data)
        {
            switch (data)
            {
                case IPackageDataLocationGetter packageDataLocation:
                {
                    switch (packageDataLocation.Location.Target)
                    {
                        case ILocationCellGetter locationCell:
                            CheckNull(locationCell.Link);
                            break;
                        case ILocationFallbackGetter locationFallback:
                            if (locationFallback.Type is LocationTargetRadius.LocationType.AliasForReference or LocationTargetRadius.LocationType.AliasForLocation)
                            {
                                CheckInvalidAlias(locationFallback.Data);
                            }
                            break;
                        case ILocationObjectIdGetter locationObjectId:
                            CheckNull(locationObjectId.Link);
                            break;
                        case ILocationObjectTypeGetter locationObjectType:
                            CheckNone(locationObjectType.Type);
                            break;
                        case ILocationTargetGetter locationTarget:
                            CheckNull(locationTarget.Link);
                            break;
                    }
                    break;
                }
                case IPackageDataTargetGetter dataTarget:
                {
                    switch (dataTarget.Target)
                    {
                        case IPackageTargetAliasGetter targetAlias:
                            CheckInvalidAlias(targetAlias.Alias);
                            break;
                        case IPackageTargetObjectIDGetter packageTargetObjectID:
                            CheckNull(packageTargetObjectID.Reference);
                            break;
                        case IPackageTargetObjectTypeGetter packageTargetObjectType:
                            CheckNone(packageTargetObjectType.Type);
                            break;
                        case IPackageTargetSpecificReferenceGetter packageTargetSpecificReference:
                            CheckNull(packageTargetSpecificReference.Reference);
                            break;
                    }

                    break;
                }
            }

            void CheckNull(IFormLinkGetter formLink)
            {
                if (package.Type == Bethesda.Skyrim.Package.Types.PackageTemplate) return;

                if (formLink.IsNull)
                {
                    var packageBranchGetters = GetBranchesNotGuardedByNullCondition(key).ToList();
                    if (packageBranchGetters.Count == 0) return;

                    param.AddTopic(
                        PackageTargetsNoObject.Format(package.GetPackageDataName(key, param.LinkCache) ?? key.ToString()));
                }
            }

            void CheckNone(TargetObjectType type)
            {
                if (package.Type == Bethesda.Skyrim.Package.Types.PackageTemplate) return;

                if (type == TargetObjectType.None)
                {
                    var unguardedBranches = GetBranchesNotGuardedByNullCondition(key).ToList();
                    if (unguardedBranches.Count == 0) return;

                    // Ignore data called "EmptyTarget" like it's used in PatrolAndHunt
                    var name = package.GetPackageDataName(key, param.LinkCache);
                    if (string.Equals(name, "EmptyTarget", StringComparison.OrdinalIgnoreCase)) return;

                    param.AddTopic(
                        PackageTargetsNoObject.Format(name ?? key.ToString()));
                }
            }

            void CheckInvalidAlias(int aliasIndex)
            {
                if (package.OwnerQuest.IsNull)
                {
                    param.AddTopic(
                        PackageWithoutOwningQuestReferencingQuestAlias.Format(package.GetPackageDataName(key, param.LinkCache) ?? key.ToString()));
                }
                else
                {
                    var quest = package.OwnerQuest.TryResolve(param.LinkCache);
                    if (quest is null) return;
                    if (quest.HasAlias((uint)aliasIndex)) return;

                    param.AddTopic(
                        PackageReferencingMissingQuestAlias.Format(package.GetPackageDataName(key, param.LinkCache) ?? key.ToString(), quest));
                }
            }
        }

        IEnumerable<IPackageBranchGetter> GetBranchesNotGuardedByNullCondition(sbyte key)
        {
            var template = package.PackageTemplate.TryResolve(param.LinkCache);
            var branches = (template is null ? package.ProcedureTree : template.ProcedureTree);
            var procedureTree = branches.BuildProcedureTree();

            foreach (var procedureUsage in branches.GetDataUsageInProcedure((byte)key))
            {
                var procedureTreeNode = procedureTree.FindNode(procedureUsage);
                var currentParent = procedureTreeNode?.Parent;
                if (currentParent is null) continue;

                var foundCondition = false;
                foreach (var condition in currentParent.Branch.Conditions)
                {
                    if (condition.Data is IIsNullPackageDataConditionDataGetter isNullPackageDataCondition
                        && isNullPackageDataCondition.PackageDataIndex == key)
                    {
                        // We found a condition in a parent that checks for null, so this is probably intentional
                        foundCondition = true;
                        break;
                    }
                }

                if (!foundCondition)
                {
                    yield return procedureUsage;
                }
            }
        }
    }

    public IEnumerable<Func<IPackageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.OwnerQuest;
        yield return x => x.Data;
    }
}
