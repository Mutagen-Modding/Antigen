using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest;

public class DisabledCreateObjectAliasAnalyzer : IIsolatedRecordAnalyzer<IQuestGetter>
{
    public static readonly TopicDefinition<string> DisabledCreateObjectAlias = MutagenTopicBuilder.FromDiscussion(
            422,
            "Initially Disabled Create Object Alias",
            Severity.Error)
        .WithFormatting<string>("Quest has alias {0} that is initially disabled and creates a reference to an object");

    public IEnumerable<TopicDefinition> Topics { get; } = [DisabledCreateObjectAlias];

    public void AnalyzeRecord(IsolatedRecordAnalyzerParams<IQuestGetter> param)
    {
        var quest = param.Record;
        if (quest.IsDeleted) return;

        foreach (var alias in quest.Aliases)
        {
            if (!alias.Flags.HasValue || !alias.Flags.Value.HasFlag(QuestAlias.Flag.InitiallyDisabled)) continue;

            if (alias.CreateReferenceToObject is { Create: CreateReferenceToObject.CreateEnum.In })
            {
                param.AddTopic(
                    DisabledCreateObjectAlias.Format(alias.Name ?? alias.ID.ToString()));
            }
        }
    }

    public IEnumerable<Func<IQuestGetter, object?>> FieldsOfInterest()
    {
        yield return x => x.Aliases;
    }
}
