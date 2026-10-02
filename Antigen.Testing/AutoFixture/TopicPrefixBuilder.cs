using AutoFixture.Kernel;
using Antigen.SDK.Topics;

namespace Antigen.Testing.AutoFixture;

public class TopicPrefixBuilder : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        if (request is MultipleRequest mult)
        {
            var req = mult.Request;
            if (req is SeededRequest seed)
            {
                req = seed.Request;
            }

            if (req is Type t)
            {
                if (t == typeof(TopicPrefix))
                {
                    return new object[]
                    {
                        new TopicPrefix("A"),
                        new TopicPrefix("B"),
                        new TopicPrefix("C"),
                    };
                }
            }
        }
        else if (request is Type t)
        {
            if (t == typeof(TopicPrefix))
            {
                return new TopicPrefix("G");
            }
        }

        return new NoSpecimen();
    }
}
