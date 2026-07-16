using System.Collections.Concurrent;
using Mutagen.Bethesda.Analyzers.SDK.Analyzers;
using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Mutagen.Bethesda.Analyzers.Skyrim.Extensions;
using Mutagen.Bethesda.Skyrim;

namespace Mutagen.Bethesda.Analyzers.Skyrim.Record.Conditions;

public class ConditionAnalyzer : IContextualRecordAnalyzer<ISkyrimMajorRecordGetter>
{
    private readonly (IConditionAnalyzer Analyzer, Type[] Types)[] _analyzers;
    private readonly ConcurrentDictionary<Type, IConditionAnalyzer[]> _interestCache = new();

    public IEnumerable<TopicDefinition> Topics { get; }

    public ConditionAnalyzer(IEnumerable<IConditionAnalyzer> conditionAnalyzers)
    {
        _analyzers = conditionAnalyzers
            .Select(a => (a, a.ConditionTypesOfInterest().ToArray()))
            .ToArray();
        Topics = _analyzers.SelectMany(x => x.Analyzer.Topics).ToArray();
    }

    private IConditionAnalyzer[] GetInterested(Type dataType)
    {
        return _interestCache.GetOrAdd(
            dataType,
            static (t, analyzers) => analyzers
                .Where(a => a.Types.Any(x => x.IsAssignableFrom(t)))
                .Select(a => a.Analyzer)
                .ToArray(),
            _analyzers);
    }

    public void AnalyzeRecord(ContextualRecordAnalyzerParams<ISkyrimMajorRecordGetter> param)
    {
        foreach (var block in param.Record.GetConditionsByField())
        {
            var conditions = block.ToArray();
            if (conditions.Length == 0) continue;

            var orBlockByIndex = BuildOrBlockLookup(conditions);

            for (var i = 0; i < conditions.Length; i++)
            {
                var interested = GetInterested(conditions[i].Data.GetType());
                if (interested.Length == 0) continue;

                foreach (var analyzer in interested)
                {
                    analyzer.AnalyzeCondition(new ConditionAnalyzerContext(
                        param with { AnalyzerType = analyzer.GetType() },
                        conditions,
                        i,
                        orBlockByIndex[i]));
                }
            }
        }
    }

    private static IReadOnlyList<IConditionGetter>[] BuildOrBlockLookup(IConditionGetter[] conditions)
    {
        var lookup = new IReadOnlyList<IConditionGetter>[conditions.Length];
        var index = 0;
        foreach (var orBlock in conditions.SplitOrBlocks())
        {
            var block = orBlock.ToArray();
            foreach (var _ in block)
            {
                lookup[index] = block;
                index++;
            }
        }

        return lookup;
    }

    public IEnumerable<Func<ISkyrimMajorRecordGetter, object?>> FieldsOfInterest()
    {
        yield break;
    }
}
