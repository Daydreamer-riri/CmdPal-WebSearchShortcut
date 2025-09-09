using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Browser;
using WebSearchShortcut.Shortcut;

namespace WebSearchShortcut.Commands;

internal sealed partial class OpenHomePageCommand : InvokableCommand
{
    private readonly ShortcutEntry _shortcut;
    private readonly BrowserExecutionInfo _browserInfo;

    public OpenHomePageCommand(ShortcutEntry shortcut)
    {
        Name = $"[UNBOUND] {nameof(OpenHomePageCommand)}.{nameof(Name)} required - shortcut='{shortcut.Name}'";

        _shortcut = shortcut;
        _browserInfo = new BrowserExecutionInfo(shortcut);
    }

    public override CommandResult Invoke()
    {
        if (!ShellHelpers.OpenCommandInShell(_browserInfo.Path, _browserInfo.ArgumentsPattern, ShortcutEntry.GetHomePageUrl(_shortcut)))
        {
            // TODO GH# 138 --> actually display feedback from the extension somewhere.
            return CommandResult.KeepOpen();
        }

        return CommandResult.Dismiss();
    }
}
