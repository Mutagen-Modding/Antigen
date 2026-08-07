using ReactiveUI;
using ReactiveUI.SourceGenerators;

namespace Antigen.ViewModels;

public sealed partial class ActiveVmController : ReactiveObject, ISingleton
{
    [Reactive] public partial ResizablePanelVM? Active { get; set; }
}
