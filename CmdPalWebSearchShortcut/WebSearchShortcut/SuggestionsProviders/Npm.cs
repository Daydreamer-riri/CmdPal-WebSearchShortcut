using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.SuggestionsProviders;

internal sealed class Npm : ISuggestionsProvider
{
    public string Name => "Npm";

    private HttpClient Http { get; } = new HttpClient();

    public async Task<IReadOnlyList<Suggestion>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use the public registry API instead of the website internal API to avoid bot protection issues
            const string api = "https://registry.npmjs.org/-/v1/search?text=";

            await using var resultStream = await Http
                .GetStreamAsync(api + Uri.EscapeDataString(query), cancellationToken)
                .ConfigureAwait(false);

            using var json = await JsonDocument
                .ParseAsync(resultStream, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var results = json.RootElement.GetProperty("objects").EnumerateArray();

            Suggestion[] items = [
                .. results
                    .Select(o =>
                    {
                        var pkg = o.GetProperty("package");
                        var title = pkg.GetProperty("name").GetString();
                        var description = pkg.TryGetProperty("description", out var desc) ? desc.GetString() : "";
                        var version = pkg.TryGetProperty("version", out var ver) ? ver.GetString() : "";
                        return title is null
                            ? null
                            : new Suggestion(title, description ?? "", version is null ? null : [version]);
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
        return "Npm";
    }
}
