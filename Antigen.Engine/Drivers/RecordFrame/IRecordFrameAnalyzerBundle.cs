using Mutagen.Bethesda.Plugins;

namespace Antigen.Drivers.RecordFrame;

public interface IRecordFrameAnalyzerBundle : IDriver
{
    RecordType TargetType { get; }
}