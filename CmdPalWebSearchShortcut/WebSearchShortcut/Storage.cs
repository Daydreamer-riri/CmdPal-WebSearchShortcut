using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WebSearchShortcut.Helpers;

namespace WebSearchShortcut;

internal sealed class Storage
{
    public List<WebSearchShortcutDataEntry> Data { get; set; } = [];

    // public List<WebSearchShortcutDataEntry> GetTopLevelShortcuts()
    // {
    //     return Data.FindAll(x => !x.HideWhenEmptyQuery);
    // }

    // public List<WebSearchShortcutDataEntry> GetFallbackShortcuts()
    // {
    //     return Data.FindAll(x => x.HideWhenEmptyQuery);
    // }

    public static Storage ReadFromFile(string path)
    {
        var data = new Storage();

        if (!File.Exists(path))
        {
            var defaultStorage = new Storage();
            defaultStorage.Data.AddRange(GetDefaultEntries());
            WriteToFile(path, defaultStorage);
        }

        // if the file exists, load the saved shortcuts
        if (File.Exists(path))
        {
            var jsonStringReading = File.ReadAllText(path);

            if (!string.IsNullOrEmpty(jsonStringReading))
            {
                data = JsonSerializer.Deserialize(jsonStringReading, AppJsonSerializerContext.Default.Storage) ?? new Storage();

                bool modified = EnsureIds(data.Data);

                if (modified)
                {
                    WriteToFile(path, data);
                }
            }
        }

        return data;
    }

    private static List<WebSearchShortcutDataEntry> GetDefaultEntries()
    {
        return
        [
            new WebSearchShortcutDataEntry
            {
                Name = "Google",
                Url = "https://www.google.com/search?q=%s",
                SuggestionProvider = "Google",
            },
            new WebSearchShortcutDataEntry
            {
                Name = "Bing",
                Url = "https://www.bing.com/search?q=%s",
                SuggestionProvider = "Bing",
            },
            new WebSearchShortcutDataEntry
            {
                Name = "Youtube",
                Url = "https://www.youtube.com/results?search_query=%s",
                SuggestionProvider = "YouTube"
            },
            new WebSearchShortcutDataEntry
            {
                Name = "DuckDuckGo",
                Url = "https://duckduckgo.com/?q=%s",
                SuggestionProvider = "DuckDuckGo",
                // HideWhenEmptyQuery = true
            },
            new WebSearchShortcutDataEntry
            {
                Name = "Wikipedia",
                Url = "https://en.wikipedia.org/w/index.php?fulltext=1&search=%s",
                SuggestionProvider = "Wikipedia",
                // HideWhenEmptyQuery = true
            },
            new WebSearchShortcutDataEntry
            {
                Name = "npm",
                Url = "https://www.npmjs.com/search?q=%s",
                SuggestionProvider = "Npm",
                // HideWhenEmptyQuery = true
            }
        ];
    }

    public static void WriteToFile(string path, Storage data)
    {
        EnsureIds(data.Data);

        var jsonString = JsonPrettyFormatter.ToPrettyJson(data, AppJsonSerializerContext.Default.Storage);

        File.WriteAllText(path, jsonString);
    }

    private static bool EnsureIds(List<WebSearchShortcutDataEntry> shortcuts)
    {
        bool modified = false;
        HashSet<string> existingIds = [];

        foreach (var shortcut in shortcuts)
        {
            while (string.IsNullOrWhiteSpace(shortcut.Id) || existingIds.Contains(shortcut.Id))
            {
                modified = true;

                shortcut.Id = GenerateId();
            }
            existingIds.Add(shortcut.Id);
        }

        return modified;
    }

    private static string GenerateId()
    {
        byte[] buffer = new byte[8];
        Random.Shared.NextBytes(buffer);
        ulong randomNumber = BitConverter.ToUInt64(buffer, 0);

        return $"WebSearchShortcut{randomNumber}";
    }
}
