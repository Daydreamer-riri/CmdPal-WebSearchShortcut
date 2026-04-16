using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace WebSearchShortcut.Suggestion;

internal interface ISuggestionsProvider
{
    public string Id { get; }
    public string DisplayName { get; }
    public Task<IReadOnlyList<SuggestionEntry>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default);
}

internal class NullSuggestionsProvider : ISuggestionsProvider
{
    public string Id => "";
    public string DisplayName => "";
    public async Task<IReadOnlyList<SuggestionEntry>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default) => [];

    public static NullSuggestionsProvider Instance { get; } = new();
}

internal class SuggestionsProvider(SuggestionsProviderEntry entry) : ISuggestionsProvider
{
    private HttpClient HttpClient { get; } = new HttpClient();

    public string Id => entry.Id;
    public string DisplayName => entry.Name;

    public async Task<IReadOnlyList<SuggestionEntry>> GetSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(query))  return [];

        try
        {
            string processedQuery = QueryUrlEncode(query, entry.ReplaceWhitespace);

            string requestUrl = GetSearchUrl(entry.Url, processedQuery);

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            if (entry.Headers is not null)
            {
                foreach (var header in entry.Headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!string.IsNullOrEmpty(entry.ResponseRegex))
            {
                var match = Regex.Match(jsonContent, entry.ResponseRegex);
                if (match.Success && match.Groups.Count > 1)
                {
                    jsonContent = match.Groups[1].Value;
                }
            }

            JToken json = JToken.Parse(jsonContent);

            var items = json.SelectTokens(entry.RootPath).ToArray();

            return [
                .. items.Select(
                    item => {
                        var title = item.SelectToken(entry.TitlePath)?.ToString();
                        if (string.IsNullOrWhiteSpace(title)) return null;

                        var description = !string.IsNullOrEmpty(entry.DescriptionPath) ? item.SelectToken(entry.DescriptionPath)?.ToString() : null;

                        var tags = entry.TagPaths?.Select(tagPath => item.SelectToken(tagPath)?.ToString()).Where(tag => tag != null).Cast<string>().ToArray();

                        var image = !string.IsNullOrEmpty(entry.ImagePath) ? item.SelectToken(entry.ImagePath)?.ToString() : null;
                        if (image is not null && image.StartsWith("//", StringComparison.Ordinal))
                        {
                            image = "https:" + image;
                        }

                        return new SuggestionEntry(
                            Title: title,
                            Description: description,
                            Tags: tags,
                            Image: image
                        );
                    }
                ).Where(e => e != null).Cast<SuggestionEntry>()
            ];

            //var titleTokens = json.SelectTokens(entry.TitlePath).ToArray();

            //if (titleTokens.Length == 0)  return [];

            //var descriptionTokens = !string.IsNullOrEmpty(entry.DescriptionPath)
            //    ? json.SelectTokens(entry.DescriptionPath).ToArray()
            //    : null;

            //var imageTokens = !string.IsNullOrEmpty(entry.ImagePath)
            //    ? json.SelectTokens(entry.ImagePath).ToArray()
            //    : null;

            //if (descriptionTokens is not null && descriptionTokens.Length != titleTokens.Length)
            //{
            //    descriptionTokens = null;
            //}

            //if (imageTokens is not null && imageTokens.Length != titleTokens.Length)
            //{
            //    imageTokens = null;
            //}

            //List<JToken[]> tagTokensList = [];
            //if (entry.TagPaths is not null)
            //{
            //    foreach (var tagPath in entry.TagPaths)
            //    {
            //        var tagTokens = json.SelectTokens(tagPath).ToArray();

            //        if (tagTokens.Length == titleTokens.Length)
            //        {
            //            tagTokensList.Add(tagTokens);
            //        }
            //        else
            //        {
            //        }
            //    }
            //}

            //return [
            //    .. titleTokens.Select(
            //        (titleToken, i) => {
            //            var title = titleToken.ToString();

            //            if (string.IsNullOrWhiteSpace(title)) return null;

            //            var imageUrl = imageTokens?[i]?.ToString();
            //            if (imageUrl is not null && imageUrl.StartsWith("//"))
            //            {
            //                imageUrl = "https:" + imageUrl;
            //            }

            //            return new SuggestionEntry(
            //                Title: title,
            //                Description: descriptionTokens?[i]?.ToString(),
            //                Tags: [
            //                    .. tagTokensList
            //                        .Select(tagTokens => tagTokens[i]?.ToString())
            //                        .Where(tag => !string.IsNullOrWhiteSpace(tag))
            //                        .Cast<string>()
            //                ],
            //                Image: imageUrl
            //            );
            //        }
            //    ).Where(entry => entry is not null).Cast<SuggestionEntry>()
            //];
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static string QueryUrlEncode(string query, string? replaceWhiteSpace)
    {
        string encoded = WebUtility.UrlEncode(query);

        return string.IsNullOrEmpty(replaceWhiteSpace)
            ? encoded
            : encoded.Replace("+", replaceWhiteSpace);
    }

    private static string GetSearchUrl(string url, string query) => url.Replace("%s", query);
}
