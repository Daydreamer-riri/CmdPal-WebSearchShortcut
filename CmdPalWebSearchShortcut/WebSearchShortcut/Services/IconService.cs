// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.CommandPalette.Extensions.Toolkit;
using WebSearchShortcut.Properties;
using WebSearchShortcut.Shortcut;

namespace WebSearchShortcut.Services;

/// <summary>
/// Provides icon-related services for web search shortcuts
/// </summary>
internal static class IconService
{
    private static readonly string DefaultIconFallback = "🔗";
    private static readonly string UserAgent = "Mozilla/5.0 (compatible; AcmeInc/1.0)";

    /// <summary>
    /// Blacklist of domains that have poor quality favicon.ico files
    /// For these domains, we'll directly use Google's favicon service
    /// </summary>
    private static readonly HashSet<string> FaviconBlacklist = new(StringComparer.OrdinalIgnoreCase)
    {
        "youtube.com",
        "www.youtube.com"
    };

    /// <summary>
    /// Gets an IconInfo for the specified shortcut, using cached icon URL if available or generating one from the URL
    /// </summary>
    /// <param name="shortcut">The web search shortcut data entry</param>
    /// <returns>IconInfo instance</returns>
    public static IconInfo GetIconInfo(ShortcutEntry shortcut)
    {
        return !string.IsNullOrWhiteSpace(shortcut.IconUrl)
            ? new IconInfo(shortcut.IconUrl)
            : new IconInfo(GetFaviconUrlFromUrl(shortcut.Url));
    }

    /// <summary>
    /// Gets favicon URL from a given URL string
    /// </summary>
    /// <param name="url">The URL to extract favicon from</param>
    /// <returns>Favicon URL or default icon fallback</returns>
    public static string GetFaviconUrlFromUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return DefaultIconFallback;
        }

        var baseString = url.Split(' ')[0].Split('?')[0];
        try
        {
            var uri = UriHelper.GetUri(baseString);
            if (uri == null)
            {
                return DefaultIconFallback;
            }
            // Check if the domain is in the blacklist
            if (FaviconBlacklist.Contains(uri.Host))
            {
                // Directly use Google's favicon service for blacklisted domains
                return $"https://www.google.com/s2/favicons?sz=64&domain={uri.GetLeftPart(UriPartial.Authority)}";
            }
            return $"{uri.Scheme}://{uri.Host}/favicon.ico";
        }
        catch (UriFormatException)
        {
            // Log if needed, but don't throw
        }

        return DefaultIconFallback;
    }

    /// <summary>
    /// Attempts to get a valid favicon URL with fallback to Google's favicon service
    /// </summary>
    /// <param name="uri">The URI to get favicon for</param>
    /// <returns>A valid favicon URL</returns>
    public static async Task<string> GetValidFaviconUrlAsync(Uri uri)
    {
        if (uri == null)
        {
            return DefaultIconFallback;
        }

        // Check if the domain is in the blacklist
        if (FaviconBlacklist.Contains(uri.Host))
        {
            // Directly use Google's favicon service for blacklisted domains
            return $"https://www.google.com/s2/favicons?sz=64&domain={uri.GetLeftPart(UriPartial.Authority)}";
        }

        // First try the direct favicon.ico
        var directFaviconUrl = $"{uri.Scheme}://{uri.Host}/favicon.ico";
        if (await IsValidFaviconAsync(directFaviconUrl))
        {
            return directFaviconUrl;
        }

        // Fallback to Google's favicon service
        return $"https://www.google.com/s2/favicons?sz=64&domain={uri.GetLeftPart(UriPartial.Authority)}";
    }

    /// <summary>
    /// Checks if a favicon URL is valid and returns an actual image
    /// </summary>
    /// <param name="faviconUrl">The favicon URL to check</param>
    /// <returns>True if the favicon is valid, false otherwise</returns>
    private static async Task<bool> IsValidFaviconAsync(string faviconUrl)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

            var response = await client.GetAsync(faviconUrl);
            return response.IsSuccessStatusCode
                && response.Content.Headers.ContentType?.MediaType != "text/html";
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Updates the icon URL for a web search shortcut data entry asynchronously
    /// </summary>
    /// <param name="shortcut">The shortcut to update</param>
    /// <returns>The updated icon URL</returns>
    public static async Task<string> UpdateIconUrlAsync(ShortcutEntry shortcut)
    {
        if (shortcut == null || !string.IsNullOrWhiteSpace(shortcut.IconUrl))
        {
            return shortcut?.IconUrl ?? DefaultIconFallback;
        }

        try
        {
            var uri = UriHelper.GetUri(shortcut.Domain ?? shortcut.Url);
            if (uri != null)
            {
                var iconUrl = await GetValidFaviconUrlAsync(uri);
                shortcut.IconUrl = iconUrl;
                return iconUrl;
            }
        }
        catch (Exception)
        {
            // Log if needed
        }

        return DefaultIconFallback;
    }
}
