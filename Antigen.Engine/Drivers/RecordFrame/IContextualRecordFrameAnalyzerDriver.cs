using Mutagen.Bethesda.Plugins.Binary.Headers;

namespace Antigen.Drivers.RecordFrame;

public interface IContextualRecordFrameAnalyzerDriver : IRecordFrameAnalyzerBundle
{
    Task Drive(ContextualDriverParams driverParams, MajorRecordFrame frame);
}
