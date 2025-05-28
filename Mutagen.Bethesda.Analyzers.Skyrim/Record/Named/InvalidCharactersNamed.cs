using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Fonts;
using Mutagen.Bethesda.Fonts.DI;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Named;

public class InvalidCharactersAnalyzerNamed(IFontProviderFactory fontProviderFactory, GameConstants gameConstants) : IIsolatedRecordAnalyzer<ISkyrimMajorRecordGetter>
{
    private readonly Dictionary<Language, IFontProvider> _fontProviders = gameConstants.Languages
        .ToDictionary(
            l => l,
            fontProviderFactory.Create);

    public static readonly TopicDefinition<string?, Language> InvalidCharactersName = MutagenTopicBuilder.FromDiscussion(
            238,
            "Invalid Characters in Name",
            Severity.Error)
        .WithFormatting<string?, Language>("The name {0} contains invalid characters in {1}");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidCharactersName];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ISkyrimMajorRecordGetter> param)
    {
        if (param.Record is not ITranslatedNamedGetter { Name: not null } named) return;


        foreach (var (language, name) in named.Name)
        {
            var invalidChars = name
                .ToCharArray()
                .Distinct()
                .Where(c => c != '"')
                .Where(c => !_fontProviders[language].ValidNameChars.Contains(c))
                .ToArray();

            if (invalidChars.Length == 0) return;

            param.AddTopic(
                InvalidCharactersName.Format(name, language),
                ("Invalid Characters", invalidChars));

        }
    }

    public IEnumerable<Func<ISkyrimMajorRecordGetter, object?>> FieldsOfInterest()
    {
        yield return x => x;
    }
}
