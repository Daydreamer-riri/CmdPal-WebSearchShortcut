using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32;
using WebSearchShortcut.Helpers;

namespace WebSearchShortcut.Browser;

public static class BrowsersDiscovery
{
    private static ILogger _logger = NullLogger.Instance;
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

    private static RegistryLogService _registryService = new(NullLogger.Instance);
    private static ProgIdBrowserService _progIdBrowserResolver = new(NullLogger.Instance);

    private static Lazy<BrowserInfo[]> _cache = new(ScanInstalledBrowsers);
    public static IReadOnlyCollection<BrowserInfo> GetInstalledBrowsers() => _cache.Value;

    public static void Update(bool warm = false)
    {
        Interlocked.Exchange(ref _cache, new Lazy<BrowserInfo[]>(ScanInstalledBrowsers));

        if (warm)
            _ = _cache.Value;
    }

    private static BrowserInfo[] ScanInstalledBrowsers()
    {
        using var scope = Logger.BeginStaticScope(typeof(BrowsersDiscovery));

        string[] progIds = GetAssociatedProgIds();
        List<BrowserInfo> browsers = [];

        foreach (var progId in progIds)
        {
            try
            {
                BrowserInfo browser = _progIdBrowserResolver.GetBrowserInfoFromProgId(progId);
                browsers.Add(browser);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }
        }

        Logger.LogInformation("Browsers loaded. Found {Count} installed browsers.", browsers.Count);
        foreach (var browser in browsers)
        {
            Logger.LogInformation("\t'{Name}' ('{Path} {ArgumentsPattern}')",
                browser.Name,
                browser.Path,
                browser.ArgumentsPattern);
        }

        return [.. browsers.OrderBy(browser => browser.Name, StringComparer.OrdinalIgnoreCase)];
    }

    private static string[] GetAssociatedProgIds()
    {
        HashSet<string> progIdSet = [];

        progIdSet.UnionWith(ScanProgIdsFromRegistry(
            Registry.LocalMachine,
            @"SOFTWARE\Clients\StartMenuInternet",
            @"Capabilities\URLAssociations")
        );

        progIdSet.UnionWith(ScanProgIdsFromRegistry(
            Registry.ClassesRoot,
            @"Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages",
            @"App\Capabilities\URLAssociations")
        );

        progIdSet.UnionWith(ScanProgIdsFromRegistry(
            Registry.CurrentUser,
            @"SOFTWARE\Clients\StartMenuInternet",
            @"Capabilities\URLAssociations")
        );

        return [.. progIdSet];
    }

    private static HashSet<string> ScanProgIdsFromRegistry(RegistryKey hiveKey, string containerPath, string targetSubPath)
    {
        using var scope = Logger.BeginStepScope(typeof(BrowsersDiscovery));

        Logger.LogTrace("Starting registry scan. hiveKey: '{hiveKey}', containerPath: '{containerPath}', targetSubPath: '{targetSubPath}'", hiveKey.Name, containerPath, targetSubPath);

        HashSet<string> progIds = [];

        using RegistryKey? containerKey = _registryService.OpenSubKey(hiveKey, containerPath, isRequired: false);
        if (containerKey is null)
        {
            Logger.LogDebug("Unique ProgIds count: {Count}\n", progIds.Count);

            return progIds;
        }

        string[] subKeyNames = containerKey.GetSubKeyNames();
        Logger.LogDebug("\tFound {Count} subkeys under '{Path}'", subKeyNames.Length, containerKey.Name);

        foreach (string subKeyName in subKeyNames)
        {
            using var subScope = Logger.BeginScope($"'{subKeyName}'");

            using RegistryKey? itemKey = _registryService.OpenSubKey(containerKey, subKeyName, isRequired: false);
            if (itemKey is null)
                continue;

            using RegistryKey? urlAssocKey = _registryService.OpenSubKey(itemKey, targetSubPath, isRequired: false);
            if (urlAssocKey is null)
                continue;

            string? httpsValue = _registryService.GetValue<string>(urlAssocKey, "https", isRequired: false);
            string? httpValue = _registryService.GetValue<string>(urlAssocKey, "http", isRequired: false);
            string? progId = httpsValue ?? httpValue;

            if (string.IsNullOrWhiteSpace(progId))
            {
                Logger.LogDebug("No valid http/https ProgId found at: '{AssocPath}'", urlAssocKey.Name);
            }
            else
            {
                progIds.Add(progId.Trim());

                Logger.LogDebug("Successfully retrieved ProgId: '{ProgId}' from '{AssocPath}'", progId.Trim(), urlAssocKey.Name);
            }
        }

        Logger.LogDebug("Registry scan completed for '{FullPath}'. Unique ProgIds count: {Count}\n", containerKey.Name, progIds.Count);

        return progIds;
    }
}
