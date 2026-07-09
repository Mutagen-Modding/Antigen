using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class AmbushAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition AmbushMissingScript = MutagenTopicBuilder.FromDiscussion(
            239,
            "Ambush requires script",
            Severity.Warning)
        .WithoutFormatting("Npc is called ambush npc but does not have an ambush script");

    public static readonly TopicDefinition AmbushNotInEditorId = MutagenTopicBuilder.FromDiscussion(
            313,
            "Ambush not in EditorId",
            Severity.None)
        .WithoutFormatting("Npc has ambush script but is not called 'Ambush' in the EditorId");

    public static readonly TopicDefinition<Aggression> AmbushAggressive = MutagenTopicBuilder.FromDiscussion(
            176,
            "Ambush npc aggressive",
            Severity.Error)
        .WithFormatting<Aggression>("NPC with ambush script is {0} not Unaggressive");

    public static readonly TopicDefinition AmbushPackages = MutagenTopicBuilder.FromDiscussion(
            314,
            "Ambush npc without ambush packages",
            Severity.Warning)
        .WithoutFormatting("Npc with ambush script is not using ambush packages");

    public static readonly TopicDefinition<IPlacedNpcGetter> AmbushParentActivator = MutagenTopicBuilder.FromDiscussion(
            540,
            "Ambush npc without parent activator",
            Severity.Warning)
        .WithFormatting<IPlacedNpcGetter>("Placed npc {0} with ambush script doesn't have a parent activator to trigger the ambush");

    public IEnumerable<TopicDefinition> Topics { get; } = [AmbushMissingScript, AmbushNotInEditorId, AmbushAggressive, AmbushParentActivator];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        bool editorIdContainsAmbush
            = npc.EditorID?.Contains("Ambush", StringComparison.OrdinalIgnoreCase) == true;

        bool hasScript
            = npc.VirtualMachineAdapter?.Scripts
                .Any(s => s.Name.Contains("Ambush", StringComparison.OrdinalIgnoreCase)) == true;

        if (editorIdContainsAmbush && !hasScript)
        {
            param.AddTopic(AmbushMissingScript.Format());
        }
        if (!editorIdContainsAmbush && hasScript)
        {
            param.AddTopic(AmbushNotInEditorId.Format());
        }

        if (!hasScript) return;

        var aggression = npc.AIData.Aggression;
        if (aggression != Aggression.Unaggressive)
        {
            param.AddTopic(AmbushAggressive.Format(aggression));
        }

        var hasAmbushPackages = npc.Packages
            .Select(p => p.TryResolve(param.LinkCache))
            .WhereNotNull()
            .Any(p => p.EditorID is not null && p.EditorID.Contains("ambush", StringComparison.OrdinalIgnoreCase));

        var hasTemplatePackages = npc.Configuration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.AIPackages);

        // Assume when using template packages that the template doesn't use ambush packages
        // Maybe revisit to analyze the template(s) as well in the future
        if (!hasAmbushPackages || hasTemplatePackages)
        {
            param.AddTopic(AmbushPackages.Format());
        }

        var linkUsageCache = param.ResolveCache<ILinkUsageCache>();
        foreach (var placedNpcLink in linkUsageCache.GetUsagesOf<IPlacedNpcGetter>(npc).UsageLinks)
        {
            if (!param.LinkCache.TryResolve(placedNpcLink, out var placedNpc)) continue;

            // If they have activate parents, they're set up properly
            if (placedNpc.ActivateParents is not null && placedNpc.ActivateParents.Parents.Count != 0) continue;

            // If they are dead from the start, it doesn't matter if they are activated
            if (placedNpc.MajorFlags.HasFlag(PlacedNpc.MajorFlag.StartsDead)) continue;

            // Also check if there is any other placed referencing the ambush npc - then it might be triggered in another way
            if (!linkUsageCache.GetUsagesOf<IPlacedGetter>(placedNpc).UsageLinks.Any())
            {

                param.AddTopic(AmbushParentActivator.Format(placedNpc));
            }
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.EditorID;
        yield return x => x.AIData;
        yield return x => x.VirtualMachineAdapter!.Scripts;
    }
}
