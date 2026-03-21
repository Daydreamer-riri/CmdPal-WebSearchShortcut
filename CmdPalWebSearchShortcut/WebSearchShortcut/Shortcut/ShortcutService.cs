using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.Shortcut;

internal sealed class ShortcutsChangedEventArgs(ShortcutEntry? before, ShortcutEntry? after) : EventArgs
{
    public ShortcutEntry? Before { get; } = before;
    public ShortcutEntry? After { get; } = after;
}

internal static class ShortcutService
{
    public static string ShortcutFilePath => Path.Combine(Utilities.BaseSettingsPath("WebSearchShortcut"), "WebSearchShortcut.json");
    public static bool ReadOnlyMode => _readOnlyMode;
    public static event EventHandler<ShortcutsChangedEventArgs>? ChangedEvent;

    private static readonly ShortcutStore _store = new();
    private static readonly Lazy<List<ShortcutEntry>> _cache = new(Load);
    private static volatile bool _readOnlyMode;
    private static readonly Lock _lock = new();

    public static IReadOnlyList<ShortcutEntry> GetShortcutsSnapshot()
    {
        lock (_lock)
        {
            var shortcuts = _cache.Value;

            var snapshot = new ShortcutEntry[shortcuts.Count];
            for (int i = 0; i < shortcuts.Count; i++)
                snapshot[i] = shortcuts[i] with { };

            return snapshot;
        }
    }

    public static void Add(ShortcutEntry shortcut)
    {
        ShortcutEntry addedShortcut;
        ShortcutsChangedEventArgs args;

        lock (_lock)
        {
            var shortcuts = _cache.Value;

            if (shortcuts.Exists(s => string.Equals(s.Id, shortcut.Id, StringComparison.Ordinal)))
                throw new ArgumentException($"Shortcut with Id '{shortcut.Id}' already exists.", nameof(shortcut));

            shortcuts.Add(shortcut with { });

            EnsureIds(shortcuts);

            addedShortcut = shortcuts[^1] with { };

            Save(shortcuts);

            args = new ShortcutsChangedEventArgs(before: null, after: addedShortcut);
        }

        ExtensionHost.LogMessage($"[WebSearchShortcut] Add Shortcut - id={addedShortcut.Id} name=\"{addedShortcut.Name}\" url={addedShortcut.Url}");

        ChangedEvent?.Invoke(typeof(ShortcutService), args);
    }

    public static void Remove(string Id)
    {
        ShortcutEntry removedShortcut;
        ShortcutsChangedEventArgs args;

        lock (_lock)
        {
            var shortcuts = _cache.Value;
            var idx = shortcuts.FindIndex(s => s.Id == Id);

            if (idx < 0)
                throw new KeyNotFoundException($"Shortcut with Id '{Id}' was not found.");

            removedShortcut = shortcuts[idx];
            shortcuts.RemoveAt(idx);

            Save(shortcuts);

            args = new ShortcutsChangedEventArgs(before: removedShortcut, after: null);
        }

        ExtensionHost.LogMessage($"[WebSearchShortcut] Remove Shortcut - id={removedShortcut.Id} name=\"{removedShortcut.Name}\" url={removedShortcut.Url}");

        ChangedEvent?.Invoke(typeof(ShortcutService), args);
    }

    public static void Update(ShortcutEntry shortcut)
    {
        ShortcutEntry updatedShortcut;
        ShortcutsChangedEventArgs args;

        lock (_lock)
        {
            var shortcuts = _cache.Value;
            var idx = shortcuts.FindIndex(s => s.Id == shortcut.Id);

            if (idx < 0)
                throw new KeyNotFoundException($"Shortcut with Id '{shortcut.Id}' was not found.");

            var before = shortcuts[idx];
            shortcuts[idx] = shortcut with { };
            updatedShortcut = shortcut with { };

            Save(shortcuts);

            args = new ShortcutsChangedEventArgs(before: before, after: updatedShortcut);
        }

        ExtensionHost.LogMessage($"[WebSearchShortcut] Update Shortcut - id={updatedShortcut.Id} name=\"{updatedShortcut.Name}\" url={updatedShortcut.Url}");

        ChangedEvent?.Invoke(typeof(ShortcutService), args);
    }

    private static List<ShortcutEntry> Load()
    {
        List<ShortcutEntry> shortcuts;

        try
        {
            shortcuts = _store.LoadOrCreate(ShortcutFilePath) ?? [];
        }
        catch (Exception ex)
        {
            _readOnlyMode = true;

            ExtensionHost.LogMessage(new LogMessage($"[WebSearchShortcut] Load failed - {ex.GetType().FullName}: {ex.Message}") { State = MessageState.Error });

            var defaults = _store.CreateDefault();

            EnsureIds(defaults);

            return defaults;
        }

        if (EnsureIds(shortcuts))
            Save(shortcuts);

        ExtensionHost.LogMessage($"[WebSearchShortcut] Load succeeded");

        return shortcuts;
    }

    private static void Save(List<ShortcutEntry> shortcuts)
    {
        if (_readOnlyMode)
            return;

        try
        {
            _store.Save(ShortcutFilePath, shortcuts);
        }
        catch (Exception ex)
        {
            _readOnlyMode = true;

            ExtensionHost.LogMessage(new LogMessage($"[WebSearchShortcut] Save failed - {ex.GetType().FullName}: {ex.Message}") { State = MessageState.Error });
        }

        ExtensionHost.LogMessage($"[WebSearchShortcut] Save succeeded");
    }

    private static bool EnsureIds(List<ShortcutEntry> shortcuts)
    {
        bool modified = false;

        HashSet<string> seenIds = [];
        for (int i = 0; i < shortcuts.Count; i++)
        {
            var shortcut = shortcuts[i];
            string id = shortcut.Id;
            if (string.IsNullOrWhiteSpace(id) || seenIds.Contains(id))
            {
                id = GenerateId();
                shortcuts[i] = shortcut with { Id = id };
                modified = true;
            }
            seenIds.Add(id);
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
