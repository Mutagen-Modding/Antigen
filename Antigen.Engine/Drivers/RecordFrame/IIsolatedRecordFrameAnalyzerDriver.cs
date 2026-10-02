using Mutagen.Bethesda.Plugins.Binary.Headers;

namespace Antigen.Drivers.RecordFrame;

public interface IIsolatedRecordFrameAnalyzerDriver : IRecordFrameAnalyzerBundle
{
    Task Drive(IsolatedDriverParams driverParams, MajorRecordFrame frame);
}
