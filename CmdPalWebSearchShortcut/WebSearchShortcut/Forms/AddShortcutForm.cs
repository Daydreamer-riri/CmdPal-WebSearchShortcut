using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Browser;
using WebSearchShortcut.Helpers;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Shortcut;

namespace WebSearchShortcut;

internal sealed partial class AddShortcutForm : FormContent
{
    private readonly ShortcutEntry? _shortcut;

    public AddShortcutForm(ShortcutEntry? shortcut)
    {
        _shortcut = shortcut;

        var name = shortcut?.Name ?? string.Empty;
        var url = shortcut?.Url ?? string.Empty;
        var suggestionProvider = shortcut?.SuggestionProvider ?? string.Empty;
        var replaceWhitespace = shortcut?.ReplaceWhitespace ?? string.Empty;
        var homePage = shortcut?.HomePage ?? string.Empty;
        var browserPath = shortcut?.BrowserPath ?? string.Empty;
        var browserArgs = shortcut?.BrowserArgs ?? string.Empty;

        BrowsersDiscovery.Reload();

        string readOnlyBanner = ShortcutService.ReadOnlyMode
            ? $$"""
                    {
                        "type": "TextBlock",
                        "text": {{JsonSerializer.Serialize(StringFormatter.Format(Resources.AddShortcutForm_ReadOnlyModeWarning, new() { ["filePath"] = ShortcutService.ShortcutFilePath}), AppJsonSerializerContext.Default.String)}},
                        "wrap": true,
                        "weight": "Bolder",
                        "color": "Attention"
                    },
            """
            : string.Empty;

        TemplateJson = $$"""
{
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "type": "AdaptiveCard",
    "version": "1.6",
    "body": [
{{readOnlyBanner}}
        {
            "id": "name",
            "type": "Input.Text",
            "label": {{JsonSerializer.Serialize(Resources.AddShortcutForm_Name_Label, AppJsonSerializerContext.Default.String)}},
            "value": {{JsonSerializer.Serialize(name, AppJsonSerializerContext.Default.String)}},
            "isRequired": true,
            "errorMessage": {{JsonSerializer.Serialize(Resources.AddShortcutForm_Name_ErrorMessage, AppJsonSerializerContext.Default.String)}}
        },
        {
            "id": "url",
            "type": "Input.Text",
            "style": "Url",
            "label": {{JsonSerializer.Serialize(Resources.AddShortcutForm_Url_Label, AppJsonSerializerContext.Default.String)}},
            "placeholder": {{JsonSerializer.Serialize(Resources.AddShortcutForm_Url_Placeholder, AppJsonSerializerContext.Default.String)}},
            "value": {{JsonSerializer.Serialize(url, AppJsonSerializerContext.Default.String)}},
            "isRequired": true,
            "errorMessage": {{JsonSerializer.Serialize(Resources.AddShortcutForm_Url_ErrorMessage, AppJsonSerializerContext.Default.String)}}
        },
        {
            "id": "suggestionProvider",
            "type": "Input.ChoiceSet",
            "label": {{JsonSerializer.Serialize(Resources.AddShortcutForm_SuggestionProvider_Label, AppJsonSerializerContext.Default.String)}},
            "placeholder": {{JsonSerializer.Serialize(Resources.AddShortcutForm_SuggestionProvider_Placeholder, AppJsonSerializerContext.Default.String)}},
            "choices": [
                {
                    "title": {{JsonSerializer.Serialize(Resources.AddShortcutForm_SuggestionProvider_None, AppJsonSerializerContext.Default.String)}},
                    "value": ""
                },
{{SuggestionsRegistry.ProviderNames.Select(key => $$"""
                {
                    "title": {{JsonSerializer.Serialize(key, AppJsonSerializerContext.Default.String)}},
                    "value": {{JsonSerializer.Serialize(key, AppJsonSerializerContext.Default.String)}}
                }
""")
    .Aggregate((a, b) => a + ",\n" + b)}}
            ],
            "value": {{JsonSerializer.Serialize(suggestionProvider, AppJsonSerializerContext.Default.String)}},
            "errorMessage": "// Just for space between items"
        },
        {
            "id": "homePage",
            "type": "Input.Text",
            "style": "Url",
            "label": {{JsonSerializer.Serialize(Resources.AddShortcutForm_Homepage_Label, AppJsonSerializerContext.Default.String)}},
            "placeholder": {{JsonSerializer.Serialize(Resources.AddShortcutForm_Homepage_Placeholder, AppJsonSerializerContext.Default.String)}},
            "value": {{JsonSerializer.Serialize(homePage, AppJsonSerializerContext.Default.String)}},
            "errorMessage": "// Just for space between items"
        },
        {
            "id": "replaceWhitespace",
            "type": "Input.Text",
            "label": {{JsonSerializer.Serialize(Resources.AddShortcutForm_ReplaceWhitespace_Label, AppJsonSerializerContext.Default.String)}},
            "placeholder": {{JsonSerializer.Serialize(Resources.AddShortcutForm_ReplaceWhitespace_Placeholder, AppJsonSerializerContext.Default.String)}},
            "value": {{JsonSerializer.Serialize(replaceWhitespace, AppJsonSerializerContext.Default.String)}},
            "errorMessage": "// Just for space between items"
        },
        {
            "id": "browserPath",
            "type": "Input.ChoiceSet",
            "label": {{JsonSerializer.Serialize(Resources.AddShortcutForm_BrowserPath_Label, AppJsonSerializerContext.Default.String)}},
            "placeholder": {{JsonSerializer.Serialize(browserPath, AppJsonSerializerContext.Default.String)}},
            "choices": [
                {
                    "title": {{JsonSerializer.Serialize(Resources.AddShortcutForm_BrowserPath_Default, AppJsonSerializerContext.Default.String)}},
                    "value": ""
                },
{{BrowsersDiscovery.GetAllInstalledBrowsers()
    .Where(b => !string.IsNullOrWhiteSpace(b.Path))
    .Select(b => $$"""
                {
                    "title": {{JsonSerializer.Serialize(b.Name, AppJsonSerializerContext.Default.String)}},
                    "value": {{JsonSerializer.Serialize(b.Path, AppJsonSerializerContext.Default.String)}}
                }
""")
    .Aggregate((a, b) => a + ",\n" + b)}}
            ],
            "value": {{JsonSerializer.Serialize(browserPath, AppJsonSerializerContext.Default.String)}},
            "errorMessage": "// Just for space between items"
        },
        {
            "id": "browserArgs",
            "type": "Input.Text",
            "label": {{JsonSerializer.Serialize(Resources.AddShortcutForm_BrowserArgs_Label, AppJsonSerializerContext.Default.String)}},
            "placeholder": {{JsonSerializer.Serialize(Resources.AddShortcutForm_BrowserArgs_Placeholder, AppJsonSerializerContext.Default.String)}},
            "value": {{JsonSerializer.Serialize(browserArgs, AppJsonSerializerContext.Default.String)}},
            "errorMessage": "// Just for space between items"
        }
    ],
    "actions": [
        {
            "type": "Action.Submit",
            "title": {{JsonSerializer.Serialize(Resources.AddShortcutForm_Save, AppJsonSerializerContext.Default.String)}},
            "data": {
                "name": "name",
                "url": "url",
                "suggestionProvider": "suggestionProvider",
                "replaceWhitespace": "replaceWhitespace",
                "homePage": "homePage",
                "browserPath": "browserPath",
                "browserArgs": "browserArgs"
            }
        }
    ]
}
""";
    }

    public override CommandResult SubmitForm(string inputs)
    {
        var root = JsonNode.Parse(inputs);
        if (root is null) return CommandResult.GoHome();

        bool isAdd = _shortcut is null;
        var shortcut = _shortcut ?? new ShortcutEntry();
        shortcut.Name = root["name"]?.GetValue<string>() ?? string.Empty;
        shortcut.Url = root["url"]?.GetValue<string>() ?? string.Empty;
        shortcut.SuggestionProvider = root["suggestionProvider"]?.GetValue<string>() ?? string.Empty;
        shortcut.ReplaceWhitespace = root["replaceWhitespace"]?.GetValue<string>() ?? string.Empty;
        shortcut.HomePage = root["homePage"]?.GetValue<string>() ?? string.Empty;
        shortcut.BrowserPath = root["browserPath"]?.GetValue<string>() ?? string.Empty;
        shortcut.BrowserArgs = root["browserArgs"]?.GetValue<string>() ?? string.Empty;

        if (isAdd)
            ShortcutService.Add(shortcut);
        else
            ShortcutService.Update(shortcut);

        return CommandResult.GoHome();
    }
}
