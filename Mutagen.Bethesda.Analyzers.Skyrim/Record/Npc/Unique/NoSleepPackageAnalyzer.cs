using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public class NoSleepPackageAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition NoSleepPackage = MutagenTopicBuilder.FromDiscussion(
            283,
            "Unique Npc Has No Sleep Package",
            Severity.Suggestion)
        .WithoutFormatting("Unique Npc has no sleep package");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoSleepPackage];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.IsUniqueActorType(param.LinkCache)) return;

        // Skip NPCs using templates for AI packages
        if (npc.Configuration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.AIPackages)) return;

        // Skip innkeepers which are usually active 24/7
        if (npc.HasFaction(param.LinkCache, editorId => editorId is not null && editorId.Contains("JobInnkeeper", StringComparison.Ordinal))) return;

        var hasSleepPackage = npc.Packages
            .Select(p => p.TryResolve(param.LinkCache))
            .WhereNotNull()
            .Any(HasSleepProcedure);

        if (!hasSleepPackage)
        {
            param.AddTopic(
                NoSleepPackage.Format());
        }

        bool HasSleepProcedure(IPackageGetter package)
        {
            if (package.PackageTemplate.IsNull)
            {
                return package.ProcedureTree.Any(x => x.ProcedureType == "Sleep");
            }

            var template = package.PackageTemplate.TryResolve(param.LinkCache);
            if (template is null) return false;

            return HasSleepProcedure(template);
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Configuration.Flags;
        yield return x => x.Configuration.TemplateFlags;
        yield return x => x.Keywords;
        yield return x => x.Factions;
        yield return x => x.Packages;
    }
}
