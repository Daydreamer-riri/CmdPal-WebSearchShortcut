using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Win32;
using WebSearchShortcut.Helpers;

namespace WebSearchShortcut.Browser;

public class RegistryLogService(ILogger logger)
{
    public RegistryLogService() : this(NullLogger.Instance) { }

    public RegistryKey OpenSubKey(RegistryKey parent, string name) => OpenSubKey(parent, name, isRequired: true)!;

    public RegistryKey? OpenSubKey(RegistryKey parent, string name, bool isRequired = false)
    {
        using var scope = logger.BeginClassScope(this);

        logger.LogTrace("[TRY-OPEN] '{ParentPath}' \\ '{SubKeyName}'", parent.Name, name);

        RegistryKey? subKey;
        try
        {
            subKey = parent.OpenSubKey(name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ERR-OPEN] '{ParentPath}' \\ '{SubKeyName}' | Error: {Message}", parent.Name, name, ex.Message);

            throw;
        }

        if (subKey is null)
        {
            if (isRequired)
            {
                logger.LogError("[ERR-OPEN] '{ParentPath}' \\ '{SubKeyName}' | Status: Not found", parent.Name, name);

                throw new KeyNotFoundException($"Registry Key '{parent.Name}\\{name}' was not found.");
            }

            logger.LogDebug("[ERR-OPEN] '{ParentPath}' \\ '{SubKeyName}' | Status: Not found", parent.Name, name);

            return null;
        }

        logger.LogTrace("[SUC-OPEN] '{ParentPath}' \\ '{SubKeyName}'", parent.Name, name);

        return subKey;
    }

    public T GetValue<T>(RegistryKey key, string? valueName = null) => GetValue<T>(key, valueName, isRequired: true)!;

    public T? GetValue<T>(RegistryKey key, string? valueName = null, bool isRequired = false)
    {
        using var scope = logger.BeginClassScope(this);

        string displayName = valueName is null ? "null" : $"'{valueName}'";
        logger.LogTrace("[TRY-READ] '{Path}' | Name: {Name}", key.Name, displayName);

        object? rawValue;
        try
        {
            rawValue = key.GetValue(valueName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[ERR-READ] '{Path}' | Name: {Name} | Error: {Msg}", key.Name, displayName, ex.Message);

            throw;
        }
         
        if (rawValue is null)
        {
            if (isRequired)
            {
                logger.LogError("[VAL-MISS] '{Path}' | Name: {Name} | Status: Not Found", key.Name, displayName);

                throw new KeyNotFoundException($"Registry value {displayName} at {key.Name} was not found.");
            }

            logger.LogDebug("[VAL-MISS] '{Path}' | Name: {Name} | Status: Not Found", key.Name, displayName);

            return default;
        }

        object displayData = rawValue is string ? $"'{rawValue}'" : rawValue;

        if (rawValue is not T convertedValue)
        {
            logger.LogError("[VAL-ERR ] '{Path}' | Name: {Name} | Data: {Val} | Expected: {Exp}, Actual: {Act}", key.Name, displayName, displayData, typeof(T).Name, rawValue.GetType().Name);

            throw new InvalidCastException($"Registry type mismatch at {key.Name}. Expected {typeof(T).Name}, but got {rawValue.GetType().Name}.");
        }

        logger.LogDebug("[VAL-READ] '{Path}' | Name: {Name} | Data: {Val} ({Type})", key.Name, displayName, displayData, rawValue.GetType().Name);

        return convertedValue;
    }
}
