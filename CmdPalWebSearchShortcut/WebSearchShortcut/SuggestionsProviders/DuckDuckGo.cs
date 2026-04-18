using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;

namespace WebSearchShortcut.SuggestionsProviders;

internal sealed class DuckDuckGo : ISuggestionsProvider
{
    public string Name => "DuckDuckGo";

    private HttpClient Http { get; } = new HttpClient();

    public async Task<IReadOnlyList<Suggestion>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            const string api = "https://duckduckgo.com/ac/?q=";

            await using var resultStream = await Http
                .GetStreamAsync(api + Uri.EscapeDataString(query), cancellationToken)
                .ConfigureAwait(false);

            using var json = await JsonDocument
                .ParseAsync(resultStream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var results = json.RootElement;

            string[] titles = [
                .. results
                    .EnumerateArray()
                    .Select(o => o.GetProperty("phrase").GetString())
                    .Where(s =>
                        !string.IsNullOrEmpty(s) &&
                        !s.Equals(query, StringComparison.OrdinalIgnoreCase)
                    )
                    .Select(s => s!)
            ];

            return [.. titles.Select(t => new Suggestion(t))];
        }
        catch (Exception e)
        {
            ExtensionHost.LogMessage($"{e.Message}");

            return [];
        }
    }

    public override string ToString()
    {
        return "DuckDuckGo";
    }
}
