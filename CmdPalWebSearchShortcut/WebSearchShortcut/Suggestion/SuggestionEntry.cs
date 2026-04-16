namespace WebSearchShortcut.Suggestion;

internal sealed record SuggestionEntry(string Title, string? Description = null, string[]? Tags = null, string? Image = null);
