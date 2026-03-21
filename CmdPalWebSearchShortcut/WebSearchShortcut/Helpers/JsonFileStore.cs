using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace WebSearchShortcut.Helpers;

public sealed class DataFile<T>
{
    public string Version { get; set; } = "0.0.0";
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public required T Data { get; set; } = default!;
}

public abstract class JsonFileStore<T>
{
    public abstract JsonTypeInfo<DataFile<T>> TypeInfo { get; }
    public virtual string CurrentVersion { get; } = "0.0.0";
    public virtual T CreateDefault() => default!;

    public T LoadOrCreate(string path)
    {
        EnsureDirectory(path);
        
        if (!File.Exists(path))
            Save(path, CreateDefault());

        string jsonString = File.ReadAllText(path);

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            Save(path, CreateDefault());
            jsonString = File.ReadAllText(path);
        }

        DataFile<T> dataFile = JsonSerializer.Deserialize(jsonString, TypeInfo) ?? throw new InvalidDataException($"Failed to parse data file '{path}'");

        return dataFile.Data;
    }

    public void Save(string path, T data)
    {
        EnsureDirectory(path);

        DataFile<T> dataFile = new()
        {
            Version = CurrentVersion,
            LastModified = DateTime.UtcNow,
            Data = data
        };
        string jsonString = JsonPrettyFormatter.ToPrettyJson(dataFile, TypeInfo);

        File.WriteAllText(path, jsonString);
    }

    private static void EnsureDirectory(string path)
    {
        string? directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory!);
    }
}
