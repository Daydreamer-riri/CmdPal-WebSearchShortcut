using System.Collections.Generic;

namespace WebSearchShortcut.Suggestion;

internal sealed record class SuggestionsProviderEntry
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string>? Headers { get; set; }
    public string? ReplaceWhitespace {  get; set; }
    public string? ResponseRegex { get; set; }
    public string RootPath { get; set; } = string.Empty;
    public string TitlePath { get; set; } = string.Empty;
    public string? DescriptionPath { get; set; }
    public string[]? TagPaths { get; set; }
    public string? ImagePath { get; set; }
}
