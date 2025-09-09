using System;
using System.Linq;
using WebSearchShortcut.Shortcut;

namespace WebSearchShortcut.Browser;

internal sealed class BrowserExecutionInfo
{
    public string? Path { get; }
    public string? ArgumentsPattern { get; }

    public BrowserExecutionInfo(ShortcutEntry shortcut)
    {
        DefaultBrowserProvider.UpdateIfTimePassed();

        Path = !string.IsNullOrWhiteSpace(shortcut.BrowserPath)
               ? shortcut.BrowserPath
               : DefaultBrowserProvider.Path;

        string? trimmedArgs;

        if (!string.IsNullOrWhiteSpace(shortcut.BrowserArgs))
        {
            trimmedArgs = shortcut.BrowserArgs.Trim();
        }
        else if (string.IsNullOrWhiteSpace(shortcut.BrowserPath))
        {
            trimmedArgs = DefaultBrowserProvider.ArgumentsPattern;
        }
        else
        {
            trimmedArgs = BrowsersDiscovery
                              .GetAllInstalledBrowsers()
                              .FirstOrDefault(b => string.Equals(b.Path, shortcut.BrowserPath, StringComparison.OrdinalIgnoreCase))
                              ?.ArgumentsPattern.Trim();
        }

        trimmedArgs ??= string.Empty;

        ArgumentsPattern = trimmedArgs.Contains("%1", StringComparison.Ordinal)
                         ? trimmedArgs
                        : trimmedArgs + " %1";
    }
}
