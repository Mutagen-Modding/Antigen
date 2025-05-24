using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell;

public class CellUnderscoreAnalyzer : IIsolatedRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<string?> CellUnderscoreWrong = MutagenTopicBuilder.FromDiscussion(
            112,
            "Cell EditorID Has Underscore",
            Severity.Error)
        .WithFormatting<string?>("Cell EditorIDs must not have underscores: {0}");

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ICellGetter> param)
    {
        if ((!param.Record.EditorID?.Contains('_')) ?? true)
        {
            return;
        }

        param.AddTopic(CellUnderscoreWrong.Format(param.Record.EditorID));
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.EditorID;
    }

    public IEnumerable<TopicDefinition> Topics => CellUnderscoreWrong.AsEnumerable();
}
