using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace WebSearchShortcut.Helpers;

internal partial class CmdPalLogger(string extensionName) : ILogger
{
    public LogLevel LogLevel_ { get; set; } = LogLevel.Information;

    private static readonly AsyncLocal<List<string>> _scopeStack = new();

    private string GetLogPath()
    {
        string directory = Path.Combine(Utilities.BaseSettingsPath(extensionName), "Logs");

        Directory.CreateDirectory(directory);

        return Path.Combine(directory, $"{extensionName}_{DateTime.Now:yyyy-MM-dd}.log");
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var currentScopes = _scopeStack.Value ?? [];
        string scopePrefix = currentScopes.Count > 0
            ? $"[{string.Join(" > ", currentScopes)}] "
            : "";

        string message = formatter(state, exception);

        var logMessage = new LogMessage($"[{extensionName}] {scopePrefix} {message}")
        {
            State = MapToMessageState(logLevel)
        };

        string logLevelString = $"[{logLevel}]";
        logLevelString = $"{logLevelString,-13}";
        string dataTimeString = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fffffff}";

        ExtensionHost.LogMessage(logMessage);
        Debug.WriteLine($"{dataTimeString} {logLevelString} {scopePrefix} {message}");
        lock (extensionName)
        {
            File.AppendAllText(GetLogPath(), $"{dataTimeString} {logLevelString} {scopePrefix} {message}\n");
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel_ && logLevel != LogLevel.None;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        _scopeStack.Value ??= [];
        _scopeStack.Value.Add(state.ToString()!);

        return new DisposableAction(() => _scopeStack.Value.RemoveAt(_scopeStack.Value.Count - 1));
    }

    private partial class DisposableAction(Action action) : IDisposable
    {
        public void Dispose() => action();
    }

    private static MessageState MapToMessageState(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace or LogLevel.Debug or LogLevel.Information => MessageState.Info,
            LogLevel.Warning => MessageState.Warning,
            LogLevel.Error or LogLevel.Critical => MessageState.Error,
            _ => MessageState.Info
        };
    }
}

public static class LoggerExtensions
{
    public static IDisposable? BeginClassScope(this ILogger logger, object instance, [CallerMemberName] string method = "")
    {
        return logger.BeginScope($"{instance.GetType().Name}.{method}");
    }

    public static IDisposable? BeginStepScope(this ILogger logger, object instance, [CallerMemberName] string step = "")
    {
        return logger.BeginScope(step);
    }

    public static IDisposable? BeginStaticScope(this ILogger logger, Type type, [CallerMemberName] string method = "")
    {
        return logger.BeginScope($"{type.Name}.{method}");
    }

    public static IDisposable? BeginStepScope(this ILogger logger, Type type, [CallerMemberName] string step = "")
    {
        return logger.BeginScope(step);
    }
}
