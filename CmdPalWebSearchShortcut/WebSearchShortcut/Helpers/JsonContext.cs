using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebSearchShortcut.Shortcut;
using WebSearchShortcut.Suggestion;

namespace WebSearchShortcut.Helpers;

[JsonSourceGenerationOptions(
    IncludeFields = true,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip
)]
[JsonSerializable(typeof(ShortcutEntry))]
[JsonSerializable(typeof(DataFile<List<ShortcutEntry>>))]
[JsonSerializable(typeof(SuggestionsProviderEntry))]
[JsonSerializable(typeof(DataFile<List<SuggestionsProviderEntry>>))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
