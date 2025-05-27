using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Plugins.Meta;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Message;

public class NoContentAnalyzer(GameConstants gameConstants) : IIsolatedRecordAnalyzer<IMessageGetter>
{
    public static readonly TopicDefinition<Language> NoContent = MutagenTopicBuilder.FromDiscussion(
            236,
            "No Content",
            Severity.Suggestion)
        .WithFormatting<Language>("Message has no content in {0}");

    public IEnumerable<TopicDefinition> Topics { get; } = [NoContent];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IMessageGetter> param)
    {
        var message = param.Record;
        if (message.Name is null) return;

        foreach (var language in gameConstants.Languages)
        {
            if (message.MenuButtons.Count == 0
                && message.Name.TryLookup(language, out var name) && name.IsNullOrWhitespace()
                && message.Description.TryLookup(language, out var desc) && desc.IsNullOrWhitespace())
            {
                param.AddTopic(
                    NoContent.Format(language));
            }
        }
    }

    public IEnumerable<Func<IMessageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Name;
        yield return x => x.Description;
        yield return x => x.MenuButtons;
    }
}
