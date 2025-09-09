using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using WebSearchShortcut.Helpers;

namespace WebSearchShortcut.Shortcut;

internal sealed class ShortcutStore : JsonFileStore<List<ShortcutEntry>>
{
    public override string CurrentVersion
    {
        get
        {
            var info = Assembly.GetEntryAssembly()?
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion;

            var semver = info?.Split('+')[0];
            return string.IsNullOrWhiteSpace(semver) ? "0.0.0" : semver;
        }
    }
    public override JsonTypeInfo<DataFile<List<ShortcutEntry>>> TypeInfo
        => AppJsonSerializerContext.Default.DataFileListShortcutEntry;
    public override List<ShortcutEntry> CreateDefault() =>
    [
        new ShortcutEntry
        {
            Name = "Google",
            Url = "https://www.google.com/search?q=%s",
            SuggestionProvider = "Google"
        },
        new ShortcutEntry
        {
            Name = "Bing",
            Url = "https://www.bing.com/search?q=%s",
            SuggestionProvider = "Bing"
        },
        new ShortcutEntry
        {
            Name = "Youtube",
            Url = "https://www.youtube.com/results?search_query=%s",
            SuggestionProvider = "YouTube"
        },
    ];
}
