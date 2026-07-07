using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Cache;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc.Unique;

public class NoCleanupScriptAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    private const string CleanupScriptName = "WIDeadBodyCleanupScript";

    public static readonly TopicDefinition NoCleanupScript = MutagenTopicBuilder.FromDiscussion(
            279,
            "Unique Npc Has No Cleanup Script",
            Severity.Suggestion)
        .WithoutFormatting("Unique Npc has no cleanup script");

    public static readonly TopicDefinition DeathContainerPropertyNotFilled = MutagenTopicBuilder.FromDiscussion(
            343,
            "Death Container Not Found",
            Severity.Warning)
        .WithoutFormatting("Death container property is not filled in cleanup script");

    public static readonly TopicDefinition DeathContainerPropertyNotFilledWithContainer = MutagenTopicBuilder.FromDiscussion(
            523,
            "Death Container Not Filled With Container",
            Severity.Warning)
        .WithoutFormatting("Death container property is not assigned to a container");

    public static readonly TopicDefinition DeathContainerPropertyCanRespawn = MutagenTopicBuilder.FromDiscussion(
            524,
            "Death Container Can Respawn",
            Severity.Warning)
        .WithoutFormatting("Death container is assigned to a furniture that can respawn");

    public static readonly TopicDefinition WIPropertyNotFilled = MutagenTopicBuilder.FromDiscussion(
            344,
            "WI quest Property Not Found",
            Severity.Warning)
        .WithoutFormatting("WI quest property is not filled in cleanup script");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoCleanupScript, DeathContainerPropertyNotFilled, WIPropertyNotFilled];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;
        if (!npc.IsUniqueActorType(param.LinkCache)) return;

        // Skip NPCs using templates for scripts
        if (npc.Configuration.TemplateFlags.HasFlag(NpcConfiguration.TemplateFlag.Script)) return;

        // Essential NPCs can't die and don't need cleanup scripts
        if (npc.Configuration.Flags.HasFlag(NpcConfiguration.Flag.Essential)) return;

        // Children can't die and don't need cleanup scripts
        var race = npc.Race.TryResolve(param.LinkCache);
        if (race is not null && race.Flags.HasFlag(Race.Flag.Child)) return;

        var script = npc.GetScript(CleanupScriptName);
        if (script is null)
        {
            // Ignore if all placements of this NPC start dead anyway
            if (param.ResolveCache<ILinkUsageCache>()
                .GetUsagesOf<IPlacedNpcGetter>(npc).UsageLinks
                .Select(link => link.TryResolve(param.LinkCache))
                .WhereNotNull()
                .All(placedNpc => placedNpc.MajorFlags.HasFlag(PlacedNpc.MajorFlag.StartsDead)))
            {
                return;
            }

            param.AddTopic(
                NoCleanupScript.Format());
            return;
        }

        var deathContainer = script.GetProperty<IScriptObjectPropertyGetter>("DeathContainer");
        if (deathContainer is null || deathContainer.Object.IsNull)
        {
            param.AddTopic(
                DeathContainerPropertyNotFilled.Format());
        }
        else
        {
            if (param.LinkCache.TryResolve<IPlacedObjectGetter>(deathContainer.Object.FormKey, out var placedObject))
            {
                var baseObject = placedObject.Base.TryResolve(param.LinkCache);
                if (baseObject is IContainerGetter container)
                {
                    if (container.Flags.HasFlag(Bethesda.Skyrim.Container.Flag.Respawns))
                    {
                        param.AddTopic(
                            DeathContainerPropertyCanRespawn.Format());
                    }
                }
                else
                {
                    param.AddTopic(
                        DeathContainerPropertyNotFilledWithContainer.Format());
                }
            }
            else
            {
                param.AddTopic(
                    DeathContainerPropertyNotFilledWithContainer.Format());
            }
        }

        var wiQuest = script.GetProperty<IScriptObjectPropertyGetter>("WI");
        if (wiQuest is null)
        {
            param.AddTopic(
                WIPropertyNotFilled.Format());
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Configuration.Flags;
        yield return x => x.Configuration.TemplateFlags;
        yield return x => x.Keywords;
        yield return x => x.VirtualMachineAdapter!.Scripts;
    }
}
