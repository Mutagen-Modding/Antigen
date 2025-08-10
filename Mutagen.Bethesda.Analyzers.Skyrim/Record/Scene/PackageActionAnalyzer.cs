using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Scene;

public class PackageActionAnalyzer : IContextualRecordAnalyzer<ISceneGetter>
{
    public static readonly TopicDefinition<uint?, int, IPackageGetter> NeverCompletingPackage = MutagenTopicBuilder.FromDiscussion(
            461,
            "Never Completing Package in Scene",
            Severity.Error)
        .WithFormatting<uint?, int, IPackageGetter>("Package action {0} in scene ending in phase {1} contains a never completing package {2}");

    public static readonly HashSet<FormKey> NeverCompletingPackageTemplates =
    [
        FormKeys.SkyrimSE.Skyrim.Package.HoldPosition.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.HoldPositionWithTravel16.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.HoldPositionWithTravel512.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.HoldPositionWithTravel1024.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.Follow.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.FollowPlayer.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.FollowAndKeepDistanceTemplate.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.FollowerPackageTemplate.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.Sandbox.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.SandboxAndGuard.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.SandboxAndKeepEyeOn.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.SandboxLockOnArrival.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.SandboxMultiLocation.FormKey,
        FormKeys.SkyrimSE.Skyrim.Package.SandboxWorkingMultiLocation.FormKey,
    ];

    public IEnumerable<TopicDefinition> Topics { get; } = [NeverCompletingPackage];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ISceneGetter> param)
    {
        var scene = param.Record;
        if (scene.IsDeleted) return;

        foreach (var packageAction in scene.Actions)
        {
            if (packageAction.EndPhase is null) continue;
            var endPhaseIndex = (int)packageAction.EndPhase;
            var endPhase = scene.Phases[endPhaseIndex];

            // All actions ending here need to be completed to continue the scene
            if (endPhase.CompletionConditions.Count != 0) continue;

            foreach (var package in packageAction.Packages
                         .Select(p => p.TryResolve(param.LinkCache))
                         .WhereNotNull())
            {
                if (NeverCompletingPackageTemplates.Contains(package.PackageTemplate.FormKey))
                {
                    param.AddTopic(
                        NeverCompletingPackage.Format(packageAction.Index, endPhaseIndex + 1, package));
                }
            }
        }
    }

    public IEnumerable<Func<ISceneGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Actions;
        yield return x => x.Phases;
    }
}
