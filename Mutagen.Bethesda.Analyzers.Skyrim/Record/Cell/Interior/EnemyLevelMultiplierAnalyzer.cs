using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Cell.Interior;

public class EnemyLevelMultiplierAnalyzer : IContextualRecordAnalyzer<ICellGetter>
{
    public static readonly TopicDefinition<int, int> HasTooFewLeveledEnemies = MutagenTopicBuilder.FromDiscussion(
            399,
            "Too Few Leveled Enemies In Dungeon",
            Severity.Suggestion)
        .WithFormatting<int, int>("Cell has only {0} leveled npcs out of a total of {1} npcs");

    public IEnumerable<TopicDefinition> Topics { get; } = [HasTooFewLeveledEnemies];

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ICellGetter> param)
    {
        var cell = param.Record;
        if (!cell.IsDungeonCell(param.LinkCache)) return;

        var totalNpcs = 0;
        var leveledNpcs = 0;
        foreach (var placedNpc in cell.GetAllPlaced(param.LinkCache).OfType<IPlacedNpcGetter>())
        {
            totalNpcs++;

            if (placedNpc.LevelModifier is not null)
            {
                leveledNpcs++;
            }
        }

        if (totalNpcs == 0) return;

        var leveledNpcPercentage = (float)leveledNpcs / totalNpcs;
        if (leveledNpcPercentage < 0.3f)
        {
            param.AddTopic(
                HasTooFewLeveledEnemies.Format(leveledNpcs, totalNpcs));
        }
    }

    public IEnumerable<Func<ICellGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Flags;
        yield return x => x.LockList;
        yield return x => x.Music;
    }
}
