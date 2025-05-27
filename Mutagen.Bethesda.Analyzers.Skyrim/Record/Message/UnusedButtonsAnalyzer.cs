using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Message;

public class UnusedButtonsAnalyzer : IIsolatedRecordAnalyzer<IMessageGetter>
{
    public static readonly TopicDefinition<int> UnusedButtons = MutagenTopicBuilder.FromDiscussion(
            237,
            "Unused Buttons",
            Severity.Suggestion)
        .WithFormatting<int>("Notification message has {0} buttons that will not be displayed");

    public IEnumerable<TopicDefinition> Topics { get; } = [UnusedButtons];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IMessageGetter> param)
    {
        var message = param.Record;

        if (message.Flags.HasFlag(Bethesda.Skyrim.Message.Flag.MessageBox)) return;
        if (message.MenuButtons.Count == 0) return;

        param.AddTopic(
            UnusedButtons.Format(message.MenuButtons.Count));
    }

    public IEnumerable<Func<IMessageGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.MenuButtons;
    }
}
