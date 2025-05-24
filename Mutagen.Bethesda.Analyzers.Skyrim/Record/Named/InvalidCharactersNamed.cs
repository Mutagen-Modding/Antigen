using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Util;
using Mutagen.Bethesda.Fonts;
using Mutagen.Bethesda.Fonts.DI;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Named;

public class InvalidCharactersAnalyzerNamed(IFontProviderFactory fontProviderFactory, Language language) : IIsolatedRecordAnalyzer<ISkyrimMajorRecordGetter>
{
    private readonly IFontProvider _fontProvider = fontProviderFactory.Create(language);

    public static readonly TopicDefinition InvalidCharactersName = MutagenTopicBuilder.FromDiscussion(
            238,
            "Invalid Characters in Name",
            Severity.Error)
        .WithoutFormatting("The name contains invalid characters");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidCharactersName];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ISkyrimMajorRecordGetter> param)
    {
        if (param.Record is not INamedGetter { Name: not null } named) return;

        var invalidChars = named.Name
            .ToCharArray()
            .Distinct()
            .Where(c => c != '"')
            .Where(c => !_fontProvider.ValidNameChars.Contains(c))
            .ToArray();

        if (invalidChars.Length == 0) return;

        param.AddTopic(
            InvalidCharactersName.Format(),
            ("Invalid Characters", invalidChars));
    }

    public IEnumerable<Func<ISkyrimMajorRecordGetter, object?>> FieldsOfInterest()
    {
        yield return x => x;
    }
}
