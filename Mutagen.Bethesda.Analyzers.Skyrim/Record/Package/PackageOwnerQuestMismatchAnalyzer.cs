using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Package;

public class PackageOwnerQuestMismatchAnalyzer : IContextualRecordAnalyzer<IPackageGetter>
{
    public static readonly TopicDefinition<IFormLinkGetter<INpcGetter>> PackageWithOwnerQuestUsedInNpc = MutagenTopicBuilder.FromDiscussion(
            535,
            "Package with Owner Quest used directly in NPC",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter<INpcGetter>>("Package has owner quest {0} but is used directly in an Npc");

    public static readonly TopicDefinition<IFormLinkGetter<IQuestGetter>, IQuestAliasGetter, IFormLinkGetter<IQuestGetter>> PackageWithOwnerQuestUsedInWrongQuest = MutagenTopicBuilder.FromDiscussion(
            536,
            "Package with Owner Quest used in wrong Quest Alias",
            Severity.Warning)
        .WithFormatting<IFormLinkGetter<IQuestGetter>, IQuestAliasGetter, IFormLinkGetter<IQuestGetter>>("Package has owner quest {0} but is used in quest alias {1} of quest {2}");

    public IEnumerable<TopicDefinition> Topics { get; } = [PackageWithOwnerQuestUsedInNpc, PackageWithOwnerQuestUsedInWrongQuest];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<IPackageGetter> param)
    {
        var package = param.Record;
        if (package.OwnerQuest.IsNull) return;

        var questFormKey = package.OwnerQuest.FormKey;

        var linkUsageCache = param.ResolveCache<ILinkUsageCache>();
        foreach (var otherQuest in linkUsageCache.GetUsagesOf<IQuestGetter>(package).UsageLinks)
        {
            if (questFormKey == otherQuest.FormKey) continue;

            var quest = otherQuest.TryResolve(param.LinkCache);
            if (quest is null) continue;

            foreach (var alias in quest.Aliases)
            {
                var aliasPackage = alias.PackageData.FirstOrDefault(p => p.FormKey == package.FormKey);
                if (aliasPackage is null) continue;

                param.AddTopic(
                    PackageWithOwnerQuestUsedInWrongQuest.Format(package.OwnerQuest, alias, otherQuest));
            }
        }

        foreach (var npcLink in linkUsageCache.GetUsagesOf<INpcGetter>(package).UsageLinks)
        {
            param.AddTopic(
                PackageWithOwnerQuestUsedInNpc.Format(npcLink));
        }
    }

    public IEnumerable<Func<IPackageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.OwnerQuest;
    }
}
