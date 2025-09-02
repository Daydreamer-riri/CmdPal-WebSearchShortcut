using System;
using System.Text.Json.Serialization;

namespace WebSearchShortcut.History;

internal sealed record HistoryEntry
{
    public string Query { get; init; }
    public DateTimeOffset Timestamp { get; init; }

    [JsonConstructor]
    public HistoryEntry(string query, DateTimeOffset timestamp)
        => (Query, Timestamp) = (query, timestamp);

    public HistoryEntry(string query) : this(query, DateTimeOffset.UtcNow) { }
}
