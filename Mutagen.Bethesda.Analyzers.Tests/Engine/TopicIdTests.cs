using Mutagen.Bethesda.Analyzers.SDK.Topics;
using Shouldly;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Tests.Engine;

public class TopicIdTests
{
    [Fact]
    public void ParseTypical()
    {
        var id = TopicId.Parse("A123");
        id.Id.ShouldBe<ushort>(123);
        id.Prefix.String.ShouldBe("A");
    }

    [Fact]
    public void ParseRejectsLongPrefix()
    {
        var prefix = "";
        for (int i = 0; i < TopicPrefix.MaxPrefixSize + 1; i++)
        {
            prefix += "A";
        }

        Assert.Throws<ArgumentException>(() =>
        {
            TopicId.Parse($"{prefix}123");
        });
    }

    [Fact]
    public void ParseRejectsIdWithNoNumbers()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            TopicId.Parse($"AB");
        });
    }

    [Fact]
    public void ParseRejectsIdWithNumbersInPrefix()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            TopicId.Parse($"1A1");
        });
    }

    [Fact]
    public void ParseRejectsIdWithPeriodInPrefix()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            TopicId.Parse($"A.B1");
        });
    }
}
