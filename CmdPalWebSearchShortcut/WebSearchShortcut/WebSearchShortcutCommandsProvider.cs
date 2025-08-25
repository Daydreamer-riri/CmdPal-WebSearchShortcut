// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Commands;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Services;
using WebSearchShortcut.Shortcut;

namespace WebSearchShortcut;

public partial class WebSearchShortcutCommandsProvider : CommandProvider
{
    private readonly ICommandItem _addShortcutItem;
    private ICommandItem[] _topLevelCommands = [];
    private IFallbackCommandItem[] _fallbackCommands = [];

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

    private void AddNewCommand_AddedCommand(object sender, ShortcutEntry shortcut)
    {
        ExtensionHost.LogMessage($"Adding bookmark ({shortcut.Name},{shortcut.Url})");

        shortcut = ShortcutService.Add(shortcut);

        UpdateIconUrlAsync(shortcut);

        Refresh();
    }

    private void Edit_AddedCommand(object sender, ShortcutEntry shortcut)
    {
        ExtensionHost.LogMessage($"Edited bookmark ({shortcut.Name},{shortcut.Url})");

        UpdateIconUrlAsync(shortcut);

        ShortcutService.Update(shortcut);

        Refresh();
    }

    private async void UpdateIconUrlAsync(ShortcutEntry shortcut)
    {
        if (!string.IsNullOrWhiteSpace(shortcut.IconUrl)) return;

        var url = await IconService.UpdateIconUrlAsync(shortcut);

        ShortcutService.Update(shortcut);

        Refresh();

        ExtensionHost.LogMessage($"Updating icon URL for bookmark ({shortcut.Name},{shortcut.Url}) to {url}");
    }
    private void Refresh()
    {
        ReloadCommands();

        RaiseItemsChanged(0);
    }

    private void ReloadCommands()
    {
        _topLevelCommands = [
            _addShortcutItem,
            .. ShortcutService
                .GetShortcutsSnapshot()
                .Select(CreateCommandItem)
        ];
        _fallbackCommands = [
            .. ShortcutService
                .GetShortcutsSnapshot()
                .Select(shortcut => new FallbackSearchWebItem(shortcut))
        ];
    }

    private CommandItem CreateCommandItem(ShortcutEntry shortcut)
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
                ExtensionHost.LogMessage($"Deleting bookmark ({shortcut.Name},{shortcut.Url})");

                ShortcutService.Remove(shortcut.Id);

                Refresh();
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
