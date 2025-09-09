// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Services;
using WebSearchShortcut.Shortcut;

namespace WebSearchShortcut;

public partial class WebSearchShortcutCommandsProvider : CommandProvider
{
    private readonly ICommandItem _addShortcutItem;
    private ICommandItem[] _topLevelCommands = [];

    public WebSearchShortcutCommandsProvider()
    {
        DisplayName = Resources.WebSearchShortcut_DisplayName;
        Icon = Icons.Logo;

        _addShortcutItem = new CommandItem(
            new AddShortcutPage(null)
            {
                Name = Resources.AddShortcutItem_Name
            }
        )
        {
            Title = Resources.AddShortcutItem_Title,
            Icon = Icons.AddShortcut
        };

        ShortcutService.ChangedEvent += OnShortcutsChanged;
    }

    public override ICommandItem[] TopLevelCommands()
    {
        if (_topLevelCommands.Length == 0)
            ReloadCommands();

        return _topLevelCommands;
    }

    private void OnShortcutsChanged(object? sender, ShortcutsChangedEventArgs args)
    {
        var before = args.Before;
        var after = args.After;

        Debug.Assert(
            before is not null || after is not null,
            "ShortcutsChanged: both Before and After are null (unexpected)."
        );

        if ((before?.Name, before?.Domain, before?.IconUrl) == (after?.Name, after?.Domain, after?.IconUrl))
            return;

        ReloadCommands();

        RaiseItemsChanged(0);

        if (after is null) return;
        if (before == after) return;

        UpdateIconUrlAsync(after);
    }

    private void ReloadCommands()
    {
        _topLevelCommands = [
            _addShortcutItem,
            .. ShortcutService
                .GetShortcutsSnapshot()
                .Select(CreateCommandItem)
        ];
    }

    private static async void UpdateIconUrlAsync(ShortcutEntry shortcut)
    {
        if (!string.IsNullOrWhiteSpace(shortcut.IconUrl)) return;

        var iconUrl = await IconService.UpdateIconUrlAsync(shortcut);

        ShortcutService.Update(shortcut);

        ExtensionHost.LogMessage($"[WebSearchShortcut] Updating icon URL for shortcut id={shortcut.Id} name=\"{shortcut.Name}\" url={shortcut.Url} to {iconUrl}");
    }

    private CommandItem CreateCommandItem(ShortcutEntry shortcut)
    {
        var editCommand = new CommandContextItem(
            new AddShortcutPage(shortcut)
            {
                Name = StringFormatter.Format(Resources.EditShortcutItem_NameTemplate, new() { ["shortcut"] = shortcut.Name }),
            }
        )
        {
            Title = StringFormatter.Format(Resources.EditShortcutItem_TitleTemplate, new() { ["shortcut"] = shortcut.Name }),
            Icon = Icons.Edit
        };

        var deleteCommand = new CommandContextItem(
            title: StringFormatter.Format(Resources.DeleteShortcutItem_TitleTemplate, new() { ["shortcut"] = shortcut.Name }),
            name: $"[UNREACHABLE] DeleteCommand.Name - shortcut='{shortcut.Name}'",
            action: null,
            result: CommandResult.Confirm(
                new ConfirmationArgs()
                {
                    Title = StringFormatter.Format(Resources.DeleteShortcutConfirm_TitleTemplate, new() { ["shortcut"] = shortcut.Name }),
                    Description = !ShortcutService.ReadOnlyMode
                        ? StringFormatter.Format(Resources.DeleteShortcutConfirm_DescriptionTemplate, new() { ["shortcut"] = shortcut.Name })
                        : StringFormatter.Format(Resources.DeleteShortcutConfirm_DescriptionTemplate_ReadOnlyMode, new() { ["shortcut"] = shortcut.Name, ["filePath"] = ShortcutService.ShortcutFilePath }),
                    PrimaryCommand = new AnonymousCommand(() => ShortcutService.Remove(shortcut.Id))
                    {
                        Name = StringFormatter.Format(Resources.DeleteShortcutConfirm_ButtonTemplate, new() { ["shortcut"] = shortcut.Name }),
                        Result = CommandResult.GoHome()
                    },
                    IsPrimaryCommandCritical = true
                }
            )
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
            MoreCommands = [editCommand, deleteCommand]
        };

        return commandItem;
    }
}
