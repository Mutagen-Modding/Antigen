using AutoFixture.Xunit2;
using Mutagen.Bethesda.Testing.AutoData;
using Xunit;

namespace Mutagen.Bethesda.Analyzers.Testing.AutoFixture;

public class MutagenModTheoryDataAttribute(params object[] objects) : CompositeDataAttribute(new InlineDataAttribute(objects), new MutagenModAutoDataAttribute())
{
}
