using System.Reflection.Metadata.Ecma335;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class GhostKeywordAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition GhostScriptMissingKeyword = MutagenTopicBuilder.FromDiscussion(
            246,
            "Ghost With Script Missing Keyword",
            Severity.Suggestion)
        .WithoutFormatting("Npc has ghost script but no ghost keyword");

    public static readonly TopicDefinition GhostScriptMissingDoesntBleedFlag = MutagenTopicBuilder.FromDiscussion(
            319,
            "Ghost With Script Missing DoesNotBleed Flag",
            Severity.Warning)
        .WithoutFormatting("Npc has ghost script but no DoesNotBleed flag");

    public static readonly TopicDefinition GhostFlagMissingKeyword = MutagenTopicBuilder.FromDiscussion(
            321,
            "Ghost With Flag Missing Keyword",
            Severity.Suggestion)
        .WithoutFormatting("Npc has ghost flag but no ghost keyword");

    public static readonly TopicDefinition GhostFlagForceGreetPackage = MutagenTopicBuilder.FromDiscussion(
            630,
            "Ghost With Script without Flag Assigned Force Greet Package",
            Severity.Warning)
        .WithoutFormatting("Npc has ghost script and Force Greet Package without ghost flag");

    public IEnumerable<TopicDefinition> Topics { get; } = [GhostScriptMissingKeyword, GhostScriptMissingDoesntBleedFlag, GhostFlagMissingKeyword, GhostFlagForceGreetPackage];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        var hasKeyword = npc.HasKeyword(FormKeys.SkyrimSE.Skyrim.Keyword.ActorTypeGhost);
        var hasFlag = npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.IsGhost);
        var hasScript = npc.HasScript("defaultGhostScript");

        if (hasScript)
        {
            if (!hasKeyword)
            {
                param.AddTopic(
                    GhostScriptMissingKeyword.Format());
            }

            if (npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.DoesNotBleed) == false)
            {
                param.AddTopic(
                    GhostScriptMissingDoesntBleedFlag.Format());
            }

            if (!hasFlag)
            {
                foreach (var package in npc.Packages)
                {
                    param.LinkCache.TryResolve<IPackageGetter>(package.FormKey, out var aipackage);
                    if (aipackage is null) continue;
                    if ((aipackage.FormKey == FormKeys.SkyrimSE.Skyrim.Package.ForceGreet.FormKey)
                        || (aipackage.FormKey == FormKeys.SkyrimSE.Skyrim.Package.ForceGreetFromSitting.FormKey)
                        || (aipackage.FormKey == FormKeys.SkyrimSE.Skyrim.Package.ForceGreetWaitSitting.FormKey)
                        || (aipackage.PackageTemplate.FormKey == FormKeys.SkyrimSE.Skyrim.Package.ForceGreet.FormKey)
                        || (aipackage.PackageTemplate.FormKey == FormKeys.SkyrimSE.Skyrim.Package.ForceGreetFromSitting.FormKey)
                        || (aipackage.PackageTemplate.FormKey == FormKeys.SkyrimSE.Skyrim.Package.ForceGreetWaitSitting.FormKey))
                    {
                        param.AddTopic(GhostFlagForceGreetPackage.Format());
                    }
                }
            }
        }

        if (hasFlag && !hasKeyword)
        {
            param.AddTopic(
                GhostFlagMissingKeyword.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Keywords;
        yield return x => x.Configuration.Flags;
        yield return x => x.VirtualMachineAdapter!.Scripts;
    }
}
