// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Commands;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Services;

namespace WebSearchShortcut;

public partial class WebSearchShortcutCommandsProvider : CommandProvider
{
    private readonly ICommandItem _addShortcutItem;
    private ICommandItem[] _topLevelCommands = [];
    private IFallbackCommandItem[] _fallbackCommands = [];
    private Storage? _storage;

    public WebSearchShortcutCommandsProvider()
    {
        DisplayName = Resources.WebSearchShortcut_DisplayName;
        Icon = Icons.Logo;

        var addShortcutPage = new AddShortcutPage(null)
        {
            Name = Resources.AddShortcutItem_Name
        };
        addShortcutPage.AddedCommand += AddNewCommand_AddedCommand;
        _addShortcutItem = new CommandItem(addShortcutPage)
        {
            Title = Resources.AddShortcutItem_Title,
            Icon = Icons.AddShortcut
        };
    }

    public override ICommandItem[] TopLevelCommands()
    {
        if (_topLevelCommands.Length == 0)
        {
            ReloadCommands();
        }

        return _topLevelCommands;
    }

    public override IFallbackCommandItem[] FallbackCommands() => _fallbackCommands;

    internal static string GetShortcutsJsonPath()
    {
        string directory = Utilities.BaseSettingsPath("WebSearchShortcut");
        Directory.CreateDirectory(directory);

        return Path.Combine(directory, "WebSearchShortcut.json");
    }

    private void AddNewCommand_AddedCommand(object sender, WebSearchShortcutDataEntry shortcut)
    {
        ExtensionHost.LogMessage($"Adding bookmark ({shortcut.Name},{shortcut.Url})");
        if (_storage != null)
        {
            _storage.Data.Add(shortcut);
            UpdateIconUrlAsync(shortcut);
        }

        SaveAndRefresh();
    }

    private void Edit_AddedCommand(object sender, WebSearchShortcutDataEntry shortcut)
    {
        ExtensionHost.LogMessage($"Edited bookmark ({shortcut.Name},{shortcut.Url})");
        UpdateIconUrlAsync(shortcut);

        SaveAndRefresh();
    }

    private async void UpdateIconUrlAsync(WebSearchShortcutDataEntry shortcut)
    {
        if (_storage is null) return;
        if (!string.IsNullOrWhiteSpace(shortcut.IconUrl)) return;

        var url = await IconService.UpdateIconUrlAsync(shortcut);
        SaveAndRefresh();
        ExtensionHost.LogMessage($"Updating icon URL for bookmark ({shortcut.Name},{shortcut.Url}) to {url}");
    }

    private void SaveAndRefresh()
    {
        if (_storage is not null)
        {
            var jsonPath = GetShortcutsJsonPath();
            Storage.WriteToFile(jsonPath, _storage);
        }

        ReloadCommands();
        RaiseItemsChanged(0);
    }

    private void ReloadCommands()
    {
        List<ICommandItem> items = [_addShortcutItem];
        List<IFallbackCommandItem> fallbackItem = [];

        if (_storage is null)
        {
            LoadShortcutFromFile();
        }

        if (_storage is not null)
        {
            items.AddRange(_storage.Data.Select(CreateCommandItem));
            fallbackItem.AddRange(_storage.Data.Select(shortcut => new FallbackSearchWebItem(shortcut)));
        }

        _topLevelCommands = [.. items];
        _fallbackCommands = [.. fallbackItem];
    }

    private void LoadShortcutFromFile()
    {
        try
        {
            var jsonFile = GetShortcutsJsonPath();
            _storage = Storage.ReadFromFile(jsonFile);
        }
        catch (Exception ex)
        {
            // debug log error
            Debug.WriteLine($"Error loading commands: {ex.Message}");
        }
    }

    private CommandItem CreateCommandItem(WebSearchShortcutDataEntry shortcut)
    {
        var openHomepageCommand = new CommandContextItem(
            new OpenHomePageCommand(shortcut)
            {
                Name = StringFormatter.Format(Resources.OpenHomepageItem_NameTemplate, new() { ["shortcut"] = shortcut.Name })
            }
        )
        {
            Title = StringFormatter.Format(Resources.OpenHomepageItem_TitleTemplate, new() { ["shortcut"] = shortcut.Name }),
            Icon = Icons.Home
        };

        var editShortcutPage = new AddShortcutPage(shortcut)
        {
            Name = StringFormatter.Format(Resources.EditShortcutItem_NameTemplate, new() { ["shortcut"] = shortcut.Name }),
        };
        editShortcutPage.AddedCommand += Edit_AddedCommand;
        var editCommand = new CommandContextItem(editShortcutPage)
        {
            Title = StringFormatter.Format(Resources.EditShortcutItem_TitleTemplate, new() { ["shortcut"] = shortcut.Name }),
            Icon = Icons.Edit
        };

        var deleteCommand = new CommandContextItem(
            title: StringFormatter.Format(Resources.DeleteShortcutItem_TitleTemplate, new() { ["shortcut"] = shortcut.Name }),
            name: $"[UNREACHABLE] DeleteCommand.Name - shortcut='{shortcut.Name}'",
            action: () =>
            {
                if (_storage != null)
                {
                    ExtensionHost.LogMessage($"Deleting bookmark ({shortcut.Name},{shortcut.Url})");

                    _storage.Data.Remove(shortcut);

                    SaveAndRefresh();
                }
            },
            result: CommandResult.KeepOpen()
        )
        {
            Icon = Icons.Delete,
            IsCritical = true
        };

        var commandItem = new CommandItem(
            new SearchWebPage(shortcut)
            {
                Name = StringFormatter.Format(Resources.ShortcutItem_NameTemplate, new() { ["shortcut"] = shortcut.Name })
            }
        )
        {
            Title = StringFormatter.Format(Resources.ShortcutItem_TitleTemplate, new() { ["shortcut"] = shortcut.Name }),
            Subtitle = StringFormatter.Format(Resources.ShortcutItem_SubtitleTemplate, new() { ["shortcut"] = shortcut.Name }),
            Icon = IconService.GetIconInfo(shortcut),
            MoreCommands = [openHomepageCommand, editCommand, deleteCommand]
        };

        return commandItem;
    }
}
