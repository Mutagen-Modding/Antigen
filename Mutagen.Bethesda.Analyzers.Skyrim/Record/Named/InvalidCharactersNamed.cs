using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Named;

public class InvalidCharactersAnalyzerNamed : IIsolatedRecordAnalyzer<ISkyrimMajorRecordGetter>
{
    public static readonly TopicDefinition InvalidCharactersName = MutagenTopicBuilder.DevelopmentTopic(
            "Invalid Characters in Name",
            Severity.Error)
        .WithoutFormatting("The name contains invalid characters");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidCharactersName];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ISkyrimMajorRecordGetter> param)
    {
        if (param.Record is not INamedGetter { Name: not null } named) return;

        var invalidStrings = InvalidCharactersAnalyzerUtil.InvalidStrings.Where(invalidString => named.Name.Contains(invalidString.Key)).ToList();
        if (invalidStrings.Count == 0) return;

        param.AddTopic(
            InvalidCharactersName.Format(),
            ("Invalid Strings", invalidStrings.Select(x => x.Key)));
    }

    public IEnumerable<Func<ISkyrimMajorRecordGetter, object?>> FieldsOfInterest()
    {
        yield return x => x;
    }
}
