using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Shouldly;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests.Engine;

public class TopicPrefixTests
{
    [Fact]
    public void CtorTypical()
    {
        var id = new TopicPrefix("A");
        id.String.ShouldBe("A");
    }

    [Fact]
    public void CtorRejectsLongPrefix()
    {
        var prefix = "";
        for (int i = 0; i < TopicPrefix.MaxPrefixSize + 1; i++)
        {
            prefix += "A";
        }

        Assert.Throws<ArgumentException>(() =>
        {
            new TopicPrefix(prefix);
        });
    }

    [Fact]
    public void CtorRejectsEmptyPrefix()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new TopicPrefix("");
        });
    }

    [Fact]
    public void CtorRejectsNullPrefix()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new TopicPrefix(null!);
        });
    }

    [Fact]
    public void CtorRejectsIdWithNumbersInPrefix()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new TopicPrefix("1A");
        });
    }

    [Fact]
    public void CtorRejectsIdWithPeriodInPrefix()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            new TopicPrefix("A.B");
        });
    }
}
