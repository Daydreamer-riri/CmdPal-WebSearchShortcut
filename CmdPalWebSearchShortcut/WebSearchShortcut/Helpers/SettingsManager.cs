using System.IO;
using Windows.Foundation;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.Helpers;

internal class SettingsManager : JsonSettingsManager
{
    private const string _namespace = "WebSearchShortcut";
    private static string Namespaced(string propertyName) => $"{_namespace}.{propertyName}";

    private const int _defaultMaxDisplayCount = 10;
    private const int _defaultMaxHistoryDisplayCount = 3;

    private readonly IntSetting _maxDisplayCount = new(
        Namespaced(nameof(MaxDisplayCount)),
        Resources.Settings_MaxDisplayCountLabel,
        Resources.Settings_MaxDisplayCountDescription,
        _defaultMaxDisplayCount,
        1, null
    )
    { ErrorMessage = Resources.Settings_MaxDisplayCountErrorMessage };

    private readonly IntSetting _maxHistoryDisplayCount = new(
        Namespaced(nameof(MaxHistoryDisplayCount)),
        Resources.Settings_MaxHistoryDisplayCountLabel,
        Resources.Settings_MaxHistoryDisplayCountDescription,
        _defaultMaxHistoryDisplayCount,
        0, null
    )
    { ErrorMessage = Resources.Settings_MaxHistoryDisplayCountErrorMessage };

    public int MaxDisplayCount => _maxDisplayCount.Value;
    public int MaxHistoryDisplayCount => _maxHistoryDisplayCount.Value;

    public event TypedEventHandler<object, Settings>? SettingsChanged
    {
        add => Settings.SettingsChanged += value;
        remove => Settings.SettingsChanged -= value;
    }

    internal static string SettingsJsonPath()
    {
        var directory = Utilities.BaseSettingsPath("Microsoft.CmdPal");

        Directory.CreateDirectory(directory);

        return Path.Combine(directory, "settings.json");
    }

    public SettingsManager()
    {
        FilePath = SettingsJsonPath();

        Settings.Add(_maxDisplayCount);
        Settings.Add(_maxHistoryDisplayCount);

        LoadSettings();

        Settings.SettingsChanged += (s, a) => SaveSettings();
    }
}
