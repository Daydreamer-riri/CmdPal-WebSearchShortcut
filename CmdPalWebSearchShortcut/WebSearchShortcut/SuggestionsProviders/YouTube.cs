using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.SuggestionsProviders;

internal sealed class YouTube : ISuggestionsProvider
{
    public string Name => "YouTube";

    private HttpClient Http { get; } = new HttpClient();

    public async Task<IReadOnlyList<Suggestion>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            const string api = "https://suggestqueries-clients6.youtube.com/complete/search?ds=yt&client=youtube&gs_ri=youtube&q=";

            var result = await Http
                .GetStringAsync(api + Uri.EscapeDataString(query), cancellationToken)
                .ConfigureAwait(false);

            var match = Regex.Match(result, @"window\.google\.ac\.h\((.*)\)$");
            if (!match.Success)
            {
                ExtensionHost.LogMessage("No match found in the response.");

                return [];
            }

            var jsonContent = match.Groups[1].Value;
            using var json = JsonDocument.Parse(jsonContent);
            var results = json.RootElement[1].EnumerateArray();

            Suggestion[] items = [
                .. results
                    .Select(o =>
                    {
                        var title = o[0].GetString();
                        return title is null ? null : new Suggestion(title);
                    })
                    .Where(s => s is not null)
                    .Select(s => s!)
            ];

            return items;
        }
        catch (Exception e)
        {
            ExtensionHost.LogMessage($"{e.Message}");

            return [];
        }
    }

    public override string ToString()
    {
        return "YouTube";
    }
}
