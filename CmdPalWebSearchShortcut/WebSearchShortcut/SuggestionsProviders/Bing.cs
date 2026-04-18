using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.SuggestionsProviders;

internal sealed class Bing : ISuggestionsProvider
{
    public string Name => "Bing";

    private HttpClient Http { get; } = new HttpClient();

    public async Task<IReadOnlyList<Suggestion>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            const string api = "https://api.bing.com/qsonhs.aspx?q=";

            await using var resultStream = await Http
                .GetStreamAsync(api + Uri.EscapeDataString(query), cancellationToken)
                .ConfigureAwait(false);

            using var json = await JsonDocument
                .ParseAsync(resultStream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var root = json.RootElement.GetProperty("AS");

            if (root.GetProperty("FullResults").GetInt32() == 0)
                return [];

            string[] titles = [
                .. root.GetProperty("Results")
                    .EnumerateArray()
                    .SelectMany(r => r.GetProperty("Suggests").EnumerateArray())
                    .Select(s => s.GetProperty("Txt").GetString())
                    .Where(s => s is not null)
                    .Select(s => s!)
            ];

            return [.. titles.Select(t => new Suggestion(t))];
        }
        catch (Exception e)
            when (e is HttpRequestException or { InnerException: TimeoutException })
        {
            ExtensionHost.LogMessage($"{e.Message}");

            return [];
        }
    }

    public override string ToString()
    {
        return "Bing";
    }
}
