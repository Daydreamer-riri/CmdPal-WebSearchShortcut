using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using WebSearchShortcut.Helpers;

namespace WebSearchShortcut.Suggestion;

internal sealed class SuggestionsProviderStore : JsonFileStore<List<SuggestionsProviderEntry>>
{
    public override string CurrentVersion
    {
        get
        {
            var info = Assembly.GetEntryAssembly()?
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion;

            var semver = info?.Split('+')[0];
            return string.IsNullOrWhiteSpace(semver) ? "0.0.0" : semver;
        }
    }

    public override JsonTypeInfo<DataFile<List<SuggestionsProviderEntry>>> TypeInfo
        => AppJsonSerializerContext.Default.DataFileListSuggestionsProviderEntry;

    public override List<SuggestionsProviderEntry> CreateDefault() =>
    [
        new SuggestionsProviderEntry
        {
            Id = "Google",
            Name = "Google",
            Url = "https://www.google.com/complete/search?output=chrome&q=%s",
            RootPath = "$[1][*]",
            TitlePath = "$"
        },
        new SuggestionsProviderEntry
        {
            Id = "Bing",
            Name = "Bing",
            Url = "https://api.bing.com/qsonhs.aspx?q=%s",
            RootPath = "$.AS.Results[*].Suggests[*]",
            TitlePath = "$.Txt"
        },
        new SuggestionsProviderEntry
        {
            Id = "DuckDuckGo",
            Name = "DuckDuckGo",
            Url = "https://duckduckgo.com/ac/?q=%s",
            RootPath = "$[*]",
            TitlePath = "$.phrase"
        },
        new SuggestionsProviderEntry
        {
            Id = "YouTube",
            Name = "YouTube",
            Url = "https://suggestqueries-clients6.youtube.com/complete/search?ds=yt&client=youtube&gs_ri=youtube&q=%s",
            ResponseRegex = @"window\.google\.ac\.h\((.*)\)$",
            RootPath = "$[1][*][0]",
            TitlePath = "$[0]"
        },
        new SuggestionsProviderEntry
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
        },
        new SuggestionsProviderEntry
        {
            Id = "npm",
            Name = "npm",
            Url = "https://registry.npmjs.org/-/v1/search?text=%s",
            RootPath = "$.objects[*].package",
            TitlePath = "$.name",
            DescriptionPath = "$.description",
            TagPaths = ["$.version"]
        },
        new SuggestionsProviderEntry
        {
            Id = "caniuse",
            Name = "Can I Use",
            Url = "https://caniuse.com/process/query.php?search=%s",
            RootPath = "$.featureIds[*]"
        }
    ];
}

