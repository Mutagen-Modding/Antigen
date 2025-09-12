using System.Text.RegularExpressions;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Dialog.Topic;

public partial class GiveGoldAnalyzer : IIsolatedRecordAnalyzer<IDialogTopicGetter>
{
    [GeneratedRegex(@"\((\d+|<\w+>) (gold)\)", RegexOptions.IgnoreCase)]
    private static partial Regex GoldRegex { get; }

    public static readonly TopicDefinition<Language, string> GoldSpelledWrongTopic = MutagenTopicBuilder.FromDiscussion(
            465,
            "Gold Spelled Wrong in Topic",
            Severity.Suggestion)
        .WithFormatting<Language, string>("Topic prompt in {0} spells giving gold as '{1}' instead of 'gold'");

    public static readonly TopicDefinition<Language, string> GoldSpelledWrongPrompt = MutagenTopicBuilder.FromDiscussion(
            466,
            "Gold Spelled Wrong in Dialog Prompt",
            Severity.Suggestion)
        .WithFormatting<Language, string>("Dialog prompt in {0} spells giving gold as '{1}' instead of 'gold'");

    public IEnumerable<TopicDefinition> Topics { get; } = [GoldSpelledWrongTopic, GoldSpelledWrongPrompt];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IDialogTopicGetter> param)
    {
        var topic = param.Record;

        if (topic.Name is null) return;

        CheckGoldSpelling(topic.Name, (language, gold) => param.AddTopic(GoldSpelledWrongTopic.Format(language, gold)));

        foreach (var responses in topic.Responses)
        {
            if (responses.Prompt is null) continue;

            CheckGoldSpelling(responses.Prompt, (language, gold) => param.AddTopic(GoldSpelledWrongPrompt.Format(language, gold)));
        }
    }

    public static void CheckGoldSpelling(ITranslatedStringGetter prompt, Action<Language, string> addTopic)
    {
        foreach (var (language, name) in prompt)
        {
            var match = GoldRegex.Match(name);
            if (match.Success)
            {
                var gold = match.Groups[2].Value;
                if (!gold.Equals("gold", StringComparison.Ordinal))
                {
                    addTopic(language, gold);
                }
            }
        }
    }

    public IEnumerable<Func<IDialogTopicGetter, object?>> FieldsOfInterest()
    {
        yield return topic => topic.Name;
    }
}
