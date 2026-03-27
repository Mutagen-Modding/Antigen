using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

                if (
                    alias.ForcedReference.IsNull
                    && alias.UniqueActor.IsNull
                    && alias.Location == null
                    && alias.External == null
                    && alias.CreateReferenceToObject == null
                    && alias.FindMatchingRefFromEvent == null
                    && alias.FindMatchingRefNearAlias == null
                    && alias.Conditions.Count == 0
                    && alias.SpecificLocation.IsNull)
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
