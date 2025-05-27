using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Npc;

public class InterruptOverridePackageAnalyzer : IContextualRecordAnalyzer<INpcGetter>
{
    public static readonly TopicDefinition<IPackageGetter> InvalidInterruptOverridePackageUsage = MutagenTopicBuilder.FromDiscussion(
            359,
            "Invalid Interrupt Override Package Usage",
            Severity.Warning)
        .WithFormatting<IPackageGetter>("Npc has an interrupt override package {0} directly assigned, which will not work as expected");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidInterruptOverridePackageUsage];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<INpcGetter> param)
    {
        var npc = param.Record;

        if (npc.Packages.Count == 0) return;

        foreach (var packageLink in npc.Packages) {
            var package = packageLink.TryResolve(param.LinkCache);
            if (package is null) continue;

            if (package.InterruptOverride != Bethesda.Skyrim.Package.Interrupt.None)
            {
                param.AddTopic(
                    InvalidInterruptOverridePackageUsage.Format(package));
            }
        }
    }

    public IEnumerable<Func<INpcGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Packages;
    }
}
