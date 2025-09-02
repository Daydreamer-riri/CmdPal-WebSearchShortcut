using System.Text.Json.Serialization;
using WebSearchShortcut.History;

namespace WebSearchShortcut;

[JsonSourceGenerationOptions(IncludeFields = true)]
[JsonSerializable(typeof(Storage))]
[JsonSerializable(typeof(WebSearchShortcutDataEntry))]
[JsonSerializable(typeof(HistoryStorage))]
[JsonSerializable(typeof(HistoryEntry))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
