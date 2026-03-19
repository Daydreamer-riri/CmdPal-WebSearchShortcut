using System.IO;
using Windows.Foundation;
using Microsoft.Extensions.Logging;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.Setting;

internal sealed class SettingsManager : JsonSettingsManager
{
    private const string _namespace = "WebSearchShortcut";
    private const string _defaultLogLevel = "Information";

    private static string SettingsJsonPath()
    {
        var directory = Utilities.BaseSettingsPath("Microsoft.CmdPal");

        Directory.CreateDirectory(directory);

        return Path.Combine(directory, "settings.json");
    }

    private static string Namespaced(string propertyName) => $"{_namespace}.{propertyName}";

    private readonly ChoiceSetSetting _logLevel = new(
        key: Namespaced("LogLevel"),
        choices: [
            new ChoiceSetSetting.Choice("Trace",       "Trace"),
            new ChoiceSetSetting.Choice("Debug",       "Debug"),
            new ChoiceSetSetting.Choice("Information", "Information"),
            new ChoiceSetSetting.Choice("Warning",     "Warning"),
            new ChoiceSetSetting.Choice("Error",       "Error"),
            new ChoiceSetSetting.Choice("None",        "None"),
        ]
    ) {
        Label = "Log Level",
        Description = "Log Level",
        Value = _defaultLogLevel
    };

    public LogLevel LogLevel_
    {
        get
        {
            return _logLevel.Value switch
            {
                "Trace"       => LogLevel.Trace,
                "Debug"       => LogLevel.Debug,
                "Information" => LogLevel.Information,
                "Warning"     => LogLevel.Warning,
                "Error"       => LogLevel.Error,
                "None"       => LogLevel.None,
                _             => LogLevel.Information
            };
        }
    }

    public static SettingsManager Instance { get; } = new();

    public SettingsManager()
    {
        FilePath = SettingsJsonPath();

        Settings.Add(_logLevel);

        LoadSettings();

        Settings.SettingsChanged += (s, a) => SaveSettings();
    }
}
