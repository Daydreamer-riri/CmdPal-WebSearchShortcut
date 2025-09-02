using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using Windows.Foundation;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut;

internal sealed partial class AddShortcutPage : ContentPage
{
    private readonly AddShortcutForm _addShortcutForm;

    public AddShortcutPage(WebSearchShortcutDataEntry? shortcut)
    {
        bool isAdd = shortcut is null;

        Id = "WebSearchShortcut.AddShortcut";
        Title = isAdd ? Resources.AddShortcutPage_Title_Add : Resources.AddShortcutPage_Title_Edit;
        Name = $"[UNBOUND] {nameof(AddShortcutPage)}.{nameof(Name)} required - shortcut={(shortcut is null ? "null" : $"'{shortcut.Name}'")}";
        Icon = isAdd ? Icons.AddShortcut : Icons.EditShortcut;

        _addShortcutForm = new AddShortcutForm(shortcut);
    }

    internal event TypedEventHandler<object, WebSearchShortcutDataEntry>? AddedCommand
    {
        add => _addShortcutForm.AddedCommand += value;
        remove => _addShortcutForm.AddedCommand -= value;
    }

    public override IContent[] GetContent() => [_addShortcutForm];
}
