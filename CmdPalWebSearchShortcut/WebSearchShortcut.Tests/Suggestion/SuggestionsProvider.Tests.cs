using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSearchShortcut.Suggestion;

namespace WebSearchShortcut.Tests.Suggestion;

[TestClass]
public class SuggestionsProviderTests
{
    private static readonly string[] TestQueries = ["apple", "google", "weather", "github"];

    [TestMethod]
    public async Task TestGoogle()
    {
        await VerifyProviderAsync(new SuggestionsProviderEntry
        {
            Id = "Google",
            Name = "Google",
            Url = "https://www.google.com/complete/search?output=chrome&q=%s",
            RootPath = "$[1][*]",
            TitlePath = "$"
        });
    }

    [TestMethod]
    public async Task TestBing()
    {
        await VerifyProviderAsync(new SuggestionsProviderEntry
        {
            Id = "Bing",
            Name = "Bing",
            Url = "https://api.bing.com/qsonhs.aspx?q=%s",
            RootPath = "$.AS.Results[*].Suggests[*]",
            TitlePath = "$.Txt"
        });
    }

    [TestMethod]
    public async Task TestDuckDuckGo()
    {
        await VerifyProviderAsync(new SuggestionsProviderEntry
        {
            Id = "DuckDuckGo",
            Name = "DuckDuckGo",
            Url = "https://duckduckgo.com/ac/?q=%s",
            RootPath = "$[*]",
            TitlePath = "$.phrase"
        });
    }

    [TestMethod]
    public async Task TestYouTube()
    {
        await VerifyProviderAsync(new SuggestionsProviderEntry
        {
            Id = "YouTube",
            Name = "YouTube",
            Url = "https://suggestqueries-clients6.youtube.com/complete/search?ds=yt&client=youtube&gs_ri=youtube&q=%s",
            ResponseRegex = @"window\.google\.ac\.h\((.*)\)$",
            RootPath = "$[1][*]",
            TitlePath = "[0]"
        });
    }

    [TestMethod]
    public async Task TestWikipedia()
    {
        await VerifyProviderAsync(new SuggestionsProviderEntry
        {
            Id = "Wikipedia",
            Name = "Wikipedia",
            Url = "https://en.wikipedia.org/w/rest.php/v1/search/title?limit=10&q=%s",
            Headers = new Dictionary<string, string>
            {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36" }
            },
            RootPath = "$.pages[*]",
            TitlePath = "$.title",
            DescriptionPath = "$.description",
            ImagePath = "$.thumbnail.url"
        });
    }

    [TestMethod]
    public async Task TestNpm()
    {
        await VerifyProviderAsync(new SuggestionsProviderEntry
        {
            Id = "npm",
            Name = "npm",
            Url = "https://registry.npmjs.org/-/v1/search?text=%s",
            RootPath = "$.objects[*].package",
            TitlePath = "$.name",
            DescriptionPath = "$.description",
            TagPaths = ["$.version"]
        });
    }

    [TestMethod]
    public async Task TestCaniuse()
    {
        await VerifyProviderAsync(new SuggestionsProviderEntry
        {
            Id = "caniuse",
            Name = "Can I Use",
            Url = "https://caniuse.com/process/query.php?search=%s",
            RootPath = "$.featureIds[*]",
        });
    }

    private static async Task VerifyProviderAsync(SuggestionsProviderEntry entry)
    {
        var provider = new SuggestionsProvider(entry);

        foreach (var query in TestQueries)
        {
            var suggestions = await provider.GetSuggestionsAsync(query);

            Assert.IsNotNull(suggestions, $"{Repr(provider.DisplayName)} returned null.");
            if (suggestions is null || suggestions.Count == 0)
            {
                Console.WriteLine($"[WARNING] {Repr(provider.DisplayName)} returned no suggestions for query: {Repr(query)}.");
                continue;
            }

            Console.WriteLine($"[Test] Provider: {Repr(provider.DisplayName)} | Query: {Repr(query)} | Count: {suggestions.Count}");

            foreach (var suggestion in suggestions)
            {
                var tagsRepr = suggestion.Tags is not null
                    ? $"[{string.Join(", ", suggestion.Tags.Select(Repr))}]"
                    : "null";

                Console.WriteLine($"  - Title: {Repr(suggestion.Title)} | Description: {Repr(suggestion.Description)} | Tags: {tagsRepr} | Image: {Repr(suggestion.Image)}");

                Assert.IsFalse(string.IsNullOrWhiteSpace(suggestion.Title), $"{provider.DisplayName} captured an empty title item.");
            }

            if (!string.IsNullOrEmpty(entry.DescriptionPath))
            {
                if (!suggestions.Any(s => s.Description != null))
                {
                    Console.WriteLine($"[WARNING] {provider.DisplayName}: DescriptionPath is configured but no descriptions were found for query {Repr(query)}. This might be normal for this specific query.");
                }
            }

            if (entry.TagPaths is { Length: > 0 })
            {
                if (!suggestions.Any(s => s.Tags != null))
                {
                    Console.WriteLine($"[WARNING] {provider.DisplayName}: TagPaths are configured but no tags were found for query {Repr(query)}.");
                }
            }

            if (!string.IsNullOrEmpty(entry.ImagePath))
            {
                if (!suggestions.Any(s => s.Image != null))
                {
                    Console.WriteLine($"[WARNING] {provider.DisplayName}: ImagePath is configured but no images were found for query {Repr(query)}.");
                }
            }
        }
    }

    private static string Repr(string? text) => text is not null ? $"\"{text}\"" : "null";
}
