using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Browser;
using WebSearchShortcut.Shortcut;

namespace WebSearchShortcut.Commands;

internal sealed partial class SearchWebCommand : InvokableCommand
{
    private readonly string _query;
    private readonly ShortcutEntry _shortcut;
    private readonly BrowserExecutionInfo _browserInfo;
    // private readonly SettingsManager _settingsManager;

    public SearchWebCommand(ShortcutEntry shortcut, string query)
    {
        Name = $"[UNBOUND] {nameof(SearchWebCommand)}.{nameof(Name)} required - shortcut='{shortcut.Name}', query='{query}'";

        _query = query;
        _shortcut = shortcut;
        _browserInfo = new BrowserExecutionInfo(shortcut);
        // _settingsManager = settingsManager;
    }

    public override CommandResult Invoke()
    {
        if (!ShellHelpers.OpenCommandInShell(_browserInfo.Path, _browserInfo.ArgumentsPattern, ShortcutEntry.GetSearchUrl(_shortcut, _query)))
        {
            // TODO GH# 138 --> actually display feedback from the extension somewhere.
            return CommandResult.KeepOpen();
        }

        // if (_settingsManager.ShowHistory != Resources.history_none)
        // {
        //   _settingsManager.SaveHistory(new HistoryItem(Arguments, DateTime.Now));
        // }

        return CommandResult.Dismiss();
    }
}
