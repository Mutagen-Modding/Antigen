using Noggog;

namespace Antigen.Config;

public interface IConfigReader<TConfig>
    where TConfig : class
{
    void ReadInto(FilePath path, TConfig config);
    void ReadInto(ReadOnlySpan<char> line, TConfig config);
}
