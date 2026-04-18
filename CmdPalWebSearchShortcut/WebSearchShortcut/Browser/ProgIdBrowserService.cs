using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32;
using WebSearchShortcut.Helpers;

namespace WebSearchShortcut.Browser;

public class ProgIdBrowserService(ILogger logger)
{
    private readonly RegistryLogService _registryService = new(logger);

    public ProgIdBrowserService() : this(NullLogger.Instance) { }

    public BrowserInfo GetBrowserInfoFromProgId(string progId)
    {
        using var scope = logger.BeginClassScope(this);

        try
        {
            using RegistryKey progIdKey = _registryService.OpenSubKey(Registry.ClassesRoot, progId);

            string name = GetBrowserName(progIdKey);
            var (path, args) = GetBrowserPathArgs(progIdKey);

            return new BrowserInfo(progId, name, path, args);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, ex.Message);

            throw;
        }
    }

    private string GetBrowserName(RegistryKey progIdKey)
    {
        using var scope = logger.BeginStepScope(this);

        string fallbackName = progIdKey.Name;
        string? appName = null;

        appName   = GetApplicationName(progIdKey);
        appName ??= GetFriendlyTypeName(progIdKey);

        if (appName is not null && appName.StartsWith('@'))
        {
            string indirectString = appName;

            try
            {
                appName = GetIndirectString(indirectString);
            }
            catch (Exception ex)
            {
                appName = fallbackName;

                logger.LogWarning(ex, "Failed to resolve indirect string resource '{IndirectString}'. ", indirectString);
            }
        }

        appName ??= fallbackName;

        return CleanBrowserName(appName);
    }

    private string? GetApplicationName(RegistryKey progIdKey)
    {
        using RegistryKey? appSubKey = _registryService.OpenSubKey(progIdKey, "Application", isRequired: false);

        if (appSubKey is null)
            return null;

        string? applicationName = _registryService.GetValue<string>(appSubKey, "ApplicationName", isRequired: false);

        return applicationName;
    }

    private string? GetFriendlyTypeName(RegistryKey progIdKey)
    {
        string? friendlyTypeName = _registryService.GetValue<string>(progIdKey, "FriendlyTypeName", isRequired: false);

        return friendlyTypeName;
    }

    private static string CleanBrowserName(string name)
    {
        string[] targets = { "URL", "HTML", "Document", "Web" };

        foreach (var target in targets)
        {
            name = name.Replace(target, "", StringComparison.OrdinalIgnoreCase);
        }

        return name.Trim();
    }

    private (string path, string args) GetBrowserPathArgs(RegistryKey progIdKey)
    {
        using var scope = logger.BeginStepScope(this);

        string path = string.Empty;
        string args = string.Empty;
        string? commandPattern = null;
        try
        {
            using RegistryKey commandKey = _registryService.OpenSubKey(progIdKey, @"shell\open\command");
            commandPattern = _registryService.GetValue<string>(commandKey, null);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogError(ex, ex.Message);

            throw;
        }

        commandPattern = commandPattern.Trim();

        if (commandPattern.StartsWith('@'))
        {
            string indirectString = commandPattern;

            try
            {
                commandPattern = GetIndirectString(indirectString);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to resolve indirect string resource '{IndirectString}'. ", indirectString);

                throw;
            }

            commandPattern = commandPattern.Trim();
        }

        try
        {
            (path, args) = ParseCommandPattern(commandPattern);
        }
        catch (FormatException ex)
        {
            logger.LogError(ex, ex.Message);

            throw;
        }

        try
        {
            path = Environment.ExpandEnvironmentVariables(path);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);

            throw;
        }

        if (!File.Exists(path) && !Uri.TryCreate(path, UriKind.Absolute, out _))
        {
            logger.LogError($"Invalid browser path from ProgId: {progIdKey.Name} → {path}");

            throw new ArgumentException($"Invalid browser path: {path}");
        }

        return (path, args);
    }
    
    private static string GetIndirectString(string str)
    {
        var stringBuilder = new StringBuilder(128);
        unsafe
        {
            var buffer = stackalloc char[128];
            var capacity = 128;
            void* reserved = null;

            int S_OK = SHLoadIndirectString(str, buffer, (uint)capacity, ref reserved);
            if (S_OK == 0)
            {
                return new string(buffer);
            }
        }

        throw new ArgumentNullException(nameof(str), "Could not load indirect string.");

        // Add this P/Invoke definition at the end of the class
        [System.Runtime.InteropServices.DllImport("shlwapi.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        static extern unsafe int SHLoadIndirectString(string pszSource, char* pszOutBuf, uint cchOutBuf, ref void* ppvReserved);
    }

    private static string EnsureQuotedExePath(string commandPattern)
    {
        if (commandPattern.StartsWith('\"') || !commandPattern.Contains(".exe", StringComparison.OrdinalIgnoreCase))
            return commandPattern;

        int pathEndIndex = commandPattern.IndexOf(".exe", StringComparison.OrdinalIgnoreCase) + 4;

        if (pathEndIndex < commandPattern.Length && commandPattern[pathEndIndex] == ' ')
        {
            return $"\"{commandPattern[..pathEndIndex]}\"{commandPattern[pathEndIndex..]}";
        }

        return commandPattern;
    }

    private static (string path, string args) ParseCommandPattern(string commandPattern)
    {
        if (string.IsNullOrWhiteSpace(commandPattern))
            throw new FormatException($"Cannot parse command pattern into path and arguments: '{commandPattern}'");

        string trimmed = commandPattern.Trim();

        if (trimmed.StartsWith('\"'))
        {
            int endQuoteIndex = trimmed.IndexOf('\"', 1);

            if (endQuoteIndex == -1)
                throw new FormatException($"Cannot parse command pattern into path and arguments: '{commandPattern}'");

            string path = trimmed.Substring(1, endQuoteIndex - 1).Trim();
            if (string.IsNullOrEmpty(path))
                throw new FormatException($"Cannot parse command pattern into path and arguments: '{commandPattern}'");

            string args = (endQuoteIndex + 1 < trimmed.Length)
                ? trimmed[(endQuoteIndex + 1)..].Trim()
                : string.Empty;

            return (path, args);
        }

        int spaceIndex = trimmed.IndexOf(' ');
        if (spaceIndex != -1)
        {
            string path = trimmed[..spaceIndex].Trim();
            string args = trimmed[(spaceIndex + 1)..].Trim();

            return (path, args);
        }

        return (trimmed, string.Empty);
    }
}
