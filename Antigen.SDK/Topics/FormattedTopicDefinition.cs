namespace Antigen.SDK.Topics;

public interface IFormattedTopicDefinition
{
    TopicDefinition TopicDefinition { get; }
    string FormattedMessage { get; }
    IFormattedTopicDefinition Transform<TParams>(
        TParams param,
        Func<TParams, object?, object?> transformer);
}

public record FormattedTopicDefinition : IFormattedTopicDefinition
{
    public required TopicDefinition TopicDefinition { get; init; }

    public override string ToString() => FormattedMessage;

    public string FormattedMessage => TopicDefinition.MessageFormat;

    public IFormattedTopicDefinition Transform<TParams>(
        TParams param,
        Func<TParams, object?, object?> transformer)
    {
        return this;
    }
}

public record FormattedTopicDefinition<T1> : IFormattedTopicDefinition
{
    public required TopicDefinition TopicDefinition { get; init; }
    public required T1 Item1 { get; init; }
    public override string ToString() => FormattedMessage;

    public string FormattedMessage => string.Format(TopicDefinition.MessageFormat, Item1);

    public IFormattedTopicDefinition Transform<TParams>(
        TParams param,
        Func<TParams, object?, object?> transformer)
    {
        return new FormattedTopicDefinition<object?>()
        {
            TopicDefinition = TopicDefinition,
            Item1 = transformer(param, Item1)
        };
    }
}

public class FormattedTopicDefinition<T1, T2> : IFormattedTopicDefinition
{
    public required TopicDefinition TopicDefinition { get; init; }
    public required T1 Item1 { get; init; }
    public required T2 Item2 { get; init; }

    public override string ToString() => FormattedMessage;

    public string FormattedMessage => string.Format(TopicDefinition.MessageFormat, Item1, Item2);

    public IFormattedTopicDefinition Transform<TParams>(
        TParams param,
        Func<TParams, object?, object?> transformer)
    {
        return new FormattedTopicDefinition<object?, object?>()
        {
            TopicDefinition = TopicDefinition,
            Item1 = transformer(param, Item1),
            Item2 = transformer(param, Item2)
        };
    }
}

public record FormattedTopicDefinition<T1, T2, T3> : IFormattedTopicDefinition
{
    public required TopicDefinition TopicDefinition { get; init; }
    public required T1 Item1 { get; init; }
    public required T2 Item2 { get; init; }
    public required T3 Item3 { get; init; }

    public override string ToString() => FormattedMessage;

    public string FormattedMessage => string.Format(TopicDefinition.MessageFormat, Item1, Item2, Item3);

    public IFormattedTopicDefinition Transform<TParams>(
        TParams param,
        Func<TParams, object?, object?> transformer)
    {
        return new FormattedTopicDefinition<object?, object?, object?>()
        {
            TopicDefinition = TopicDefinition,
            Item1 = transformer(param, Item1),
            Item2 = transformer(param, Item2),
            Item3 = transformer(param, Item3)
        };
    }
}

public record FormattedTopicDefinition<T1, T2, T3, T4> : IFormattedTopicDefinition
{
    public required TopicDefinition TopicDefinition { get; init; }
    public required T1 Item1 { get; init; }
    public required T2 Item2 { get; init; }
    public required T3 Item3 { get; init; }
    public required T4 Item4 { get; init; }

    public override string ToString() => FormattedMessage;

    public string FormattedMessage => string.Format(TopicDefinition.MessageFormat, Item1, Item2, Item3, Item4);

    public IFormattedTopicDefinition Transform<TParams>(
        TParams param,
        Func<TParams, object?, object?> transformer)
    {
        return new FormattedTopicDefinition<object?, object?, object?, object?>()
        {
            TopicDefinition = TopicDefinition,
            Item1 = transformer(param, Item1),
            Item2 = transformer(param, Item2),
            Item3 = transformer(param, Item3),
            Item4 = transformer(param, Item4)
        };
    }
}
