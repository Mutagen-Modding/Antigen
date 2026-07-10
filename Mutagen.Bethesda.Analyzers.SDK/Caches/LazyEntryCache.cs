using System.Collections.Concurrent;

namespace Mutagen.Bethesda.Analyzers.SDK.Caches;

/// <summary>
/// A thread-safe cache that lazily computes and stores a value for each key exactly once,
/// with every caller observing the same instance.
/// </summary>
public sealed class LazyEntryCache<TKey, TValue>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _entries = new();
    private readonly Func<TKey, TValue> _valueFactory;

    public LazyEntryCache(Func<TKey, TValue> valueFactory)
    {
        _valueFactory = valueFactory;
    }

    public TValue GetOrAdd(TKey key)
    {
        return _entries.GetOrAdd(
            key,
            static (k, factory) => new Lazy<TValue>(() => factory(k)),
            _valueFactory).Value;
    }
}
