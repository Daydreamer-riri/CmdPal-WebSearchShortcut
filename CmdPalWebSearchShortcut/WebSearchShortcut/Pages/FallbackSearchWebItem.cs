using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Commands;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Services;

namespace WebSearchShortcut;

internal sealed partial class FallbackSearchWebItem : FallbackCommandItem
{
    private readonly SearchWebCommand _searchWebCommand;
    private readonly WebSearchShortcutDataEntry _shortcut;

    public FallbackSearchWebItem(WebSearchShortcutDataEntry shortcut)
        : base(new SearchWebCommand(shortcut, string.Empty) { Id = $"{shortcut.Id}.fallback" }, shortcut.Name)
    {
        _shortcut = shortcut;

        _searchWebCommand = (SearchWebCommand) Command!;
        _searchWebCommand.Name = string.Empty;

        Title = string.Empty;
        Subtitle = string.Empty;
        Icon = IconService.GetIconInfo(shortcut);
        MoreCommands = [
            new CommandContextItem(
                new OpenHomePageCommand(_shortcut)
                {
                    Name = StringFormatter.Format(Resources.OpenHomepageItem_NameTemplate, new() { ["shortcut"] = _shortcut.Name })
                }
            )
            {
                Title = StringFormatter.Format(Resources.OpenHomepageItem_TitleTemplate, new() { ["shortcut"] = _shortcut.Name }),
                Icon = Icons.Home
            }
        ];
    }

    public override void UpdateQuery(string query)
    {
        bool isEmpty = string.IsNullOrWhiteSpace(query);

        // Order matters: update command state before Title. (Verified 2025-08-21)
        // Title may be derived from/overwritten by Command.Name.
        // Set Command.Name before Title to avoid stale or inconsistent UI.
        _searchWebCommand.Name = isEmpty ? string.Empty : StringFormatter.Format(Resources.SearchQueryItem_NameTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] = query });
        _searchWebCommand.Query = query;

        Title = isEmpty ? string.Empty : StringFormatter.Format(Resources.SearchQueryItem_TitleTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] = query });
        Subtitle = isEmpty ? string.Empty : StringFormatter.Format(Resources.SearchQueryItem_SubtitleTemplate, new() { ["shortcut"] = _shortcut.Name, ["query"] = query });
    }
}
