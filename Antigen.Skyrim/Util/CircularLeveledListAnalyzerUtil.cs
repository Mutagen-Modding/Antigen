using Antigen.SDK.Analyzers;
using Antigen.SDK.Topics;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Records;

namespace Antigen.Skyrim.Util;

public static class CircularLeveledListAnalyzerUtil
{
    public static void FindCircularList<T>(
        ContextualRecordAnalyzerParams<T> param,
        Func<T, IEnumerable<FormKey>> nestedEntriesSelector,
        TopicDefinition topic)
        where T : class, IMajorRecordGetter
    {
        var stack = new Stack<T>();

        FindCircularListInternal(param.Record);

        void FindCircularListInternal(T t)
        {
            if (stack.Any(x => x.FormKey == t.FormKey))
            {
                param.AddTopic(topic.Format(), ("Path", stack.ToList()));
                return;
            }

            stack.Push(t);

            foreach (var formKey in nestedEntriesSelector(t))
            {
                if (!param.LinkCache.TryResolve<T>(formKey, out var leveledList)) continue;

                FindCircularListInternal(leveledList);
            }

            stack.Pop();
        }
    }
}
