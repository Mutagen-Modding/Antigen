using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Quest
{
    public class AliasForcedNoneAnalyzer : IIsolatedRecordAnalyzer<IQuestGetter>
    {
        public static readonly TopicDefinition<string?> AliasForcedNone = MutagenTopicBuilder.FromDiscussion(
                554,
                "Non-optional Alias Forced to None",
                Severity.Error)
            .WithFormatting<string?>("Alias {0} is forced to none but is not optional");

        public IEnumerable<TopicDefinition> Topics => [AliasForcedNone];

        void IIsolatedRecordAnalyzer<IQuestGetter>.AnalyzeRecord(IsolatedRecordAnalyzerParams<IQuestGetter> param)
        {
            foreach (var alias in param.Record.Aliases)
            {
                if (alias.Flags?.HasFlag(QuestAlias.Flag.Optional) ?? false)
                    continue;

                if (alias.IsForcedNone(param.Record))
                {
                    param.AddTopic(AliasForcedNone.Format(alias.Name));
                }
            }
        }

        IEnumerable<Func<IQuestGetter, object?>> IIsolatedRecordAnalyzer<IQuestGetter>.FieldsOfInterest()
        {
            yield return x => x.Aliases;
        }
    }
}
