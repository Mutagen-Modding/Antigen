using System.Diagnostics;
using System.Reactive;
using ReactiveUI;

namespace Antigen.Resources.Command;

public static class OpenLinkCommands
{
    public static readonly ReactiveCommand<Uri?, Unit> OpenUriCommand = ReactiveCommand.Create<Uri?>(OpenUri);

    private static void OpenUri(Uri? uri)
    {
        if (uri is null) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = uri.ToString(),
            UseShellExecute = true
        });
    }
}