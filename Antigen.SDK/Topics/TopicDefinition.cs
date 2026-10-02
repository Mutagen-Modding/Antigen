namespace Antigen.SDK.Topics;

public partial record TopicDefinition
{
    public TopicId Id { get; init; }
    public string Title { get; init; }
    public Severity Severity { get; init; }
    public string MessageFormat { get; init; }
    public Uri? InformationUri { get; init; }

    public TopicDefinition(
        TopicId id,
        string title,
        Severity severity,
        string? messageFormat = null,
        Uri? informationUri = null)
    {
        Id = id;
        Title = title;
        Severity = severity;
        MessageFormat = messageFormat ?? title;
        InformationUri = informationUri;
    }

    public static TopicDefinition FromDiscussion(
        string nickname,
        ushort id,
        string title,
        Severity severity,
        string discussionsUri)
    {
        return new TopicDefinition(
            id: new TopicId(nickname, id),
            title: title,
            severity: severity,
            informationUri: new Uri($"{discussionsUri.TrimEnd('/')}/{id.ToString()}"));
    }

    public IFormattedTopicDefinition Format()
    {
        return new FormattedTopicDefinition
        {
            TopicDefinition = this,
        };
    }

    public override string ToString() => this.ToShortString();
}

public record TopicDefinition<T1> : TopicDefinition
{
    public TopicDefinition(
        TopicId id,
        string title,
        string messageFormat,
        Severity severity,
        Uri? informationUri = null)
        : base(id, title, severity, messageFormat, informationUri)
    {
    }

    public IFormattedTopicDefinition Format(T1 item1)
    {
        return new FormattedTopicDefinition<T1>
        {
            TopicDefinition = this,
            Item1 = item1,
        };
    }

    public override string ToString() => this.ToShortString();
}

public record TopicDefinition<T1, T2> : TopicDefinition
{
    public TopicDefinition(
        TopicId id,
        string title,
        string messageFormat,
        Severity severity,
        Uri? informationUri = null)
        : base(id, title, severity, messageFormat, informationUri)
    {
    }

    public IFormattedTopicDefinition Format(T1 item1, T2 item2)
    {
        return new FormattedTopicDefinition<T1, T2>
        {
            TopicDefinition = this,
            Item1 = item1,
            Item2 = item2,
        };
    }

    public override string ToString() => this.ToShortString();
}

public record TopicDefinition<T1, T2, T3> : TopicDefinition
{
    public TopicDefinition(
        TopicId id,
        string title,
        string messageFormat,
        Severity severity,
        Uri? informationUri = null)
        : base(id, title, severity, messageFormat, informationUri)
    {
    }

    public IFormattedTopicDefinition Format(T1 item1, T2 item2, T3 item3)
    {
        return new FormattedTopicDefinition<T1, T2, T3>
        {
            TopicDefinition = this,
            Item1 = item1,
            Item2 = item2,
            Item3 = item3,
        };
    }

    public override string ToString() => this.ToShortString();
}

public record TopicDefinition<T1, T2, T3, T4> : TopicDefinition
{
    public TopicDefinition(
        TopicId id,
        string title,
        string messageFormat,
        Severity severity,
        Uri? informationUri = null)
        : base(id, title, severity, messageFormat, informationUri)
    {
    }

    public IFormattedTopicDefinition Format(T1 item1, T2 item2, T3 item3, T4 item4)
    {
        return new FormattedTopicDefinition<T1, T2, T3, T4>
        {
            TopicDefinition = this,
            Item1 = item1,
            Item2 = item2,
            Item3 = item3,
            Item4 = item4,
        };
    }

    public override string ToString() => this.ToShortString();
}
