using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32;
using WebSearchShortcut.Helpers;

namespace WebSearchShortcut.Browser;

public static class DefaultBrowserProvider
{
    private record CacheEntry(BrowserInfo Browser, long LastUpdateTick);

    private static ILogger _logger = NullLogger.Instance;
    private static RegistryLogService _registryService = new(NullLogger.Instance);
    private static ProgIdBrowserService _progIdBrowserResolver = new(NullLogger.Instance);

    public static ILogger Logger
    {
        get => _logger;
        set
        {
            _logger = value;
            _registryService = new RegistryLogService(value);
            _progIdBrowserResolver = new ProgIdBrowserService(value);
        }
    }

    private static readonly BrowserInfo MSEdgeBrowser = new(
        "MSEdgeHTM",
        "Microsoft Edge",
        System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            @"Microsoft\Edge\Application\msedge.exe"
        ),
        "--single-argument %1"
    );

    private static readonly Lock _updateLock = new();

    private static volatile CacheEntry _cache = new(MSEdgeBrowser, -UpdateTimeout);

    public const long UpdateTimeout = 300;

    public static BrowserInfo GetDefaultBrowser() => _cache.Browser;

    public static void UpdateIfTimePassed()
    {
        if (Environment.TickCount64 - _cache.LastUpdateTick < UpdateTimeout)
            return;

        lock (_updateLock)
        {
            if (Environment.TickCount64 - _cache.LastUpdateTick >= UpdateTimeout)
            {
                UpdateInternal();
            }
        }
    }

    public static void Update()
    {
        lock (_updateLock)
        {
            UpdateInternal();
        }
    }

    private static void UpdateInternal()
    {
        using var scope = Logger.BeginStaticScope(typeof(DefaultBrowserProvider));

        long now = Environment.TickCount64;

        BrowserInfo browser = _cache.Browser;

        try
        {
            string? progId = GetDefaultBrowserProgId();
            if (!string.IsNullOrEmpty(progId))
            {
                browser = _progIdBrowserResolver.GetBrowserInfoFromProgId(progId);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception when retrieving browser path/name. Path and Name are set to use Microsoft Edge.");
        }
        finally
        {
            _cache = new CacheEntry(browser, Environment.TickCount64);
        }

        Logger.LogInformation("Current default browser is '{Name}' ('{Path} {ArgumentsPattern}')", browser.Name, browser.Path, browser.ArgumentsPattern);
    }

    private static string? GetDefaultBrowserProgId()
    {
        using var scope = Logger.BeginStepScope(typeof(DefaultBrowserProvider));

        string[] paths = [
            @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoiceLatest\ProgId",
            @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\https\UserChoice",
            @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoiceLatest\ProgId",
            @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice",
        ];

        foreach (string path in paths)
        {
            RegistryKey? userChoiceKey = _registryService.OpenSubKey(Registry.CurrentUser, path, isRequired: false);

            if (userChoiceKey is null)
                continue;

            string? progId = _registryService.GetValue<string>(userChoiceKey, "ProgId", isRequired: false);

            if (!string.IsNullOrEmpty(progId))
                return progId;
        }

        return null;
    }
}
