using System.Text.RegularExpressions;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.LeveledItem;

public partial class ChanceNoneAnalyzer : IIsolatedRecordAnalyzer<ILeveledItemGetter>
{
    [GeneratedRegex(@"(\d+)$")]
    public static partial Regex ChanceEditorIDRegex { get; }

    public static readonly TopicDefinition<Percent, int> InvalidChanceNoneEditorID = MutagenTopicBuilder.FromDiscussion(
            471,
            "Invalid Chance in Editor ID",
            Severity.Suggestion)
        .WithFormatting<Percent, int>("Leveled Item has a Chance None of {0} but the EditorID which should end with the Chance ends with '{1}'");

    public IEnumerable<TopicDefinition> Topics { get; } = [InvalidChanceNoneEditorID];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ILeveledItemGetter> param)
    {
        var leveledItem = param.Record;
        if (leveledItem.IsDeleted) return;

        if (leveledItem.ChanceNone == Percent.Zero) return;
        if (leveledItem.EditorID is null) return;

        var match = ChanceEditorIDRegex.Match(leveledItem.EditorID);
        if (!match.Success) return;

        if (!int.TryParse(match.Groups[1].Value, out var editorIdChance)) return;

        if ((editorIdChance + leveledItem.ChanceNone).EqualsWithin(1))
        {
            param.AddTopic(
                InvalidChanceNoneEditorID.Format(leveledItem.ChanceNone, editorIdChance));
        }
    }

    public IEnumerable<Func<ILeveledItemGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.ChanceNone;
        yield return x => x.EditorID;
    }
}
