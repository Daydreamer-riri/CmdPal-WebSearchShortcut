using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Shortcut;

namespace WebSearchShortcut;

internal sealed partial class AddShortcutPage : ContentPage
{
    private readonly AddShortcutForm _addShortcutForm;

    public AddShortcutPage(ShortcutEntry? shortcut)
    {
        bool isAdd = shortcut is null;

        Id = "WebSearchShortcut.AddShortcut";
        Title = isAdd ? Resources.AddShortcutPage_Title_Add : Resources.AddShortcutPage_Title_Edit;
        Name = $"[UNBOUND] {nameof(AddShortcutPage)}.{nameof(Name)} required - shortcut={(shortcut is null ? "null" : $"'{shortcut.Name}'")}";
        Icon = isAdd ? Icons.AddShortcut : Icons.EditShortcut;

        _addShortcutForm = new AddShortcutForm(shortcut);
    }

    public override IContent[] GetContent() => [_addShortcutForm];
}
