using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Package;

public class PackageTemplateConditionsAnalyzer : IIsolatedRecordAnalyzer<IPackageGetter>
{
    public static readonly TopicDefinition<int> PackageTemplateWithConditions = MutagenTopicBuilder.FromDiscussion(
            533,
            "Package Template Conditions Won't Work",
            Severity.Warning)
        .WithFormatting<int>("Package template has {0} conditions that won't do anything");

    public IEnumerable<TopicDefinition> Topics { get; } = [PackageTemplateWithConditions];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IPackageGetter> param)
    {
        var package = param.Record;

        if (package is { Type: Bethesda.Skyrim.Package.Types.PackageTemplate, Conditions.Count: > 0 })
        {
            param.AddTopic(
                PackageTemplateWithConditions.Format(package.Conditions.Count));
        }
    }

    public IEnumerable<Func<IPackageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Type;
        yield return x => x.Conditions;
    }
}

