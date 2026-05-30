using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Package;

public class FragmentAnalyzerPackage : IIsolatedRecordAnalyzer<IPackageGetter>
{
    public static readonly TopicDefinition<string> DuplicateFragment = MutagenTopicBuilder.FromDiscussion(
            587,
            "Duplicate package fragment name",
            Severity.Error)
        .WithFormatting<string>("Fragment function {0} is used multiple times");

    public static readonly TopicDefinition EmptyFragment = MutagenTopicBuilder.FromDiscussion(
            588,
            "Empty package fragment script",
            Severity.Suggestion)
        .WithoutFormatting("Package has script attached, but no fragments");

    public IEnumerable<TopicDefinition> Topics => [DuplicateFragment, EmptyFragment];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IPackageGetter> param)
    {
        var vmad = param.Record.VirtualMachineAdapter;
        if (vmad == null)
            return;

        if (vmad.ScriptFragments == null)
        {
            param.AddTopic(EmptyFragment.Format());
        }
        else
        {
            FragmentAnalyzerUtil.CheckDuplicateFragments(
                param,
                DuplicateFragment,
                [vmad.ScriptFragments.OnBegin?.FragmentName, vmad.ScriptFragments.OnChange?.FragmentName, vmad.ScriptFragments.OnEnd?.FragmentName]);
        }
    }

    public IEnumerable<Func<IPackageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.VirtualMachineAdapter;
    }
}
