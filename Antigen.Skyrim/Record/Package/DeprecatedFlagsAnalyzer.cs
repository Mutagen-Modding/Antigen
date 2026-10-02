using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Antigen.Skyrim.Record.Package;

public class DeprecatedFlagsAnalyzer: IIsolatedRecordAnalyzer<IPackageGetter>
{
    public static readonly TopicDefinition UnlockDoorsFlag = MutagenTopicBuilder.FromDiscussion(
            396,
            "Use of Deprecated Unlock Doors Flag",
            Severity.Warning)
        .WithoutFormatting("Package uses deprecated Unlock Doors flag, not the UnlockDoors procedure");

    public IEnumerable<TopicDefinition> Topics { get; } = [UnlockDoorsFlag];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IPackageGetter> param)
    {
        var package = param.Record;

        if (package.Flags.HasFlag(Mutagen.Bethesda.Skyrim.Package.Flag.UnlockDoorsAtPackageStart) ||
            package.Flags.HasFlag(Mutagen.Bethesda.Skyrim.Package.Flag.UnlockDoorsAtPackageEnd))
        {
            param.AddTopic(
                UnlockDoorsFlag.Format());
        }
    }

    public IEnumerable<Func<IPackageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
    }
}
