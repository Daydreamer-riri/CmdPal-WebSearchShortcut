using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WebSearchShortcut.Helpers;

using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.History;

internal sealed class HistoryStorage
{
    public Dictionary<string, List<HistoryEntry>> Data { get; set; } = [];

    public static HistoryStorage ReadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            HistoryStorage empty = new();

            WriteToFile(path, empty);

            return empty;
        }

        string jsonString;

        try
        {
            jsonString = File.ReadAllText(path);
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"[HistoryStorage] ReadFile failed: {ex.GetType().Name}: {ex.Message}");

            throw;
        }

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return new HistoryStorage();
        }

        try
        {
            return JsonSerializer.Deserialize(jsonString, AppJsonSerializerContext.Default.HistoryStorage) ?? new HistoryStorage();
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"[HistoryStorage] JsonDeserialize failed: {ex.GetType().Name}: {ex.Message}");

            throw;
        }
    }

    public static void WriteToFile(string path, HistoryStorage data)
    {
        var jsonString = JsonPrettyFormatter.ToPrettyJson(data, AppJsonSerializerContext.Default.HistoryStorage);

        try
        {
            File.WriteAllText(path, jsonString);
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage($"[HistoryStorage] WriteFile failed: {ex.GetType().Name}: {ex.Message}");

            throw;
        }
    }
}
