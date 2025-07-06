using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.Strings;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell;

public class InconsistentCellNameAnalyzer : IIsolatedRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<string, Language, char> CellHasInconsistentCharacter = MutagenTopicBuilder.FromDiscussion(
            398,
            "Cell Name Contains Inconsistent Character",
            Severity.Suggestion)
        .WithFormatting<string, Language, char>("Cell name '{0}' in {1} contains a character '{2}' which is inconsistent with the naming conventions.");

    public IEnumerable<TopicDefinition> Topics { get; } = [CellHasInconsistentCharacter];

    private static readonly char[] InconsistentCharacters = [','];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;
        if (cell.IsDeleted) return;
        var cellName = cell.Name;
        if (cellName is null) return;

        foreach (var (language, name) in cellName)
        {
            foreach (var containedChar in InconsistentCharacters.Where(c => name.Contains(c)))
            {
                param.AddTopic(
                    CellHasInconsistentCharacter.Format(name, language, containedChar));
            }
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.EditorID;
    }
}
