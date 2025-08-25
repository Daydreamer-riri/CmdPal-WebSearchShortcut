using System;
using System.Linq;
using WebSearchShortcut.Shortcut;

namespace WebSearchShortcut.Browser;

internal sealed class BrowserExecutionInfo
{
    public string Path { get; }
    public string ArgumentsPattern { get; }

    public BrowserExecutionInfo(ShortcutEntry shortcut)
    {
        DefaultBrowserProvider.UpdateIfTimePassed();
        BrowserInfo defaultBrowser = DefaultBrowserProvider.GetDefaultBrowser();

        Path = !string.IsNullOrWhiteSpace(shortcut.BrowserPath)
            ? shortcut.BrowserPath.Trim()
            : defaultBrowser.Path.Trim();

        string? trimmedArgs;

        if (!string.IsNullOrWhiteSpace(shortcut.BrowserArgs))
        {
            trimmedArgs = shortcut.BrowserArgs.Trim();
        }
        else if (string.IsNullOrWhiteSpace(shortcut.BrowserPath))
        {
            trimmedArgs = defaultBrowser.ArgumentsPattern.Trim();
        }
        else
        {
            trimmedArgs = BrowsersDiscovery
                .GetInstalledBrowsers()
                .FirstOrDefault(browser => string.Equals(browser.Path.Trim(), shortcut.BrowserPath.Trim(), StringComparison.OrdinalIgnoreCase))?
                .ArgumentsPattern.Trim();
        }

        if (string.IsNullOrWhiteSpace(trimmedArgs))
        {
            trimmedArgs = string.Empty;
        }

        if (!trimmedArgs.Contains("%1"))
        {
            trimmedArgs += " %1";
        }

        ArgumentsPattern = trimmedArgs.Trim();
    }
}
