using System;
using System.Net;
using System.Text.Json.Serialization;

namespace WebSearchShortcut.Shortcut;

internal sealed record class ShortcutEntry
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Keyword { get; set; }
    public string Url { get; set; } = string.Empty;
    // public string[]? Urls { get; set; }
    public string? SuggestionProvider { get; set; }
    public string? HomePage { get; set; }
    // public bool HideWhenEmptyQuery { get; set; } // TODO: wait powertoys cmdpal support for update with query
    public string? IconUrl { get; set; }
    public string? ReplaceWhitespace { get; set; }
    public string? BrowserPath { get; set; }
    public string? BrowserArgs { get; set; }

    [JsonIgnore]
    public string Domain
    {
        get
        {
            return new Uri(Url.Split(' ')[0].Split('?')[0]).GetLeftPart(UriPartial.Authority);
        }
    }

    public static string UrlEncode(ShortcutEntry shortcut, string query)
    {
        string encoded = WebUtility.UrlEncode(query);

        return string.IsNullOrEmpty(shortcut.ReplaceWhitespace)
            ? encoded
            : encoded.Replace("+", shortcut.ReplaceWhitespace);
    }

    public static string GetSearchUrl(ShortcutEntry shortcut, string query)
    {
        string arguments = shortcut.Url.Replace("%s", UrlEncode(shortcut, query));

        return arguments;
    }

    public static string GetHomePageUrl(ShortcutEntry shortcut)
    {
        return !string.IsNullOrWhiteSpace(shortcut.HomePage)
                ? shortcut.HomePage
                : shortcut.Domain;
    }
}
