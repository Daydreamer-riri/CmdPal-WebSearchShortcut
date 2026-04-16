using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebSearchShortcut.Suggestion;

internal static class SuggestionsRegistry
{
    public static string SuggestionsProvidersFilePath => Path.Combine(Utilities.BaseSettingsPath("WebSearchShortcut"), "WebSearchShortcut_suggestionsProviders.json");

    private static readonly SuggestionsProviderStore _store = new();
    private static readonly Lazy<Dictionary<string, SuggestionsProvider>> _cache = new(Load);

    public static ISuggestionsProvider TryGet(string? Id)
    {
        if (Id is null) return NullSuggestionsProvider.Instance;

        if (_cache.Value.TryGetValue(Id, out var provider))
        {
            return provider;
        }
        return NullSuggestionsProvider.Instance;
    }

    public static IReadOnlyList<ISuggestionsProvider> GetAll()
    {
        return [.. _cache.Value.Values.OrderBy(provider => provider.DisplayName)];
    }

    private static Dictionary<string, SuggestionsProvider> Load()
    {
        List<SuggestionsProviderEntry> suggestionsProviderEntries;

        try
        {
            suggestionsProviderEntries = _store.LoadOrCreate(SuggestionsProvidersFilePath) ?? [];
        }
        catch (Exception ex)
        {
            ExtensionHost.LogMessage(new LogMessage($"[WebSearchShortcut] Load failed - {ex.GetType().FullName}: {ex.Message}") { State = MessageState.Error });

            suggestionsProviderEntries = _store.CreateDefault();
        }

        ExtensionHost.LogMessage($"[WebSearchShortcut] Load succeeded");

        return suggestionsProviderEntries.ToDictionary(entry => entry.Id, entry => new SuggestionsProvider(entry), StringComparer.OrdinalIgnoreCase);
    }
}
