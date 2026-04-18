using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace WebSearchShortcut.Helpers;

public static class JsonPrettyFormatter
{
    private static readonly JsonWriterOptions PrettyWriterOptions = new()
    {
        Indented = true,
        IndentSize = 4,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string ToPrettyJson<T>(T obj, JsonTypeInfo<T> typeInfo)
    {
        byte[] utf8Json = JsonSerializer.SerializeToUtf8Bytes(obj, typeInfo);

        using JsonDocument doc = JsonDocument.Parse(utf8Json);

        using var output = new MemoryStream();
        using var writer = new Utf8JsonWriter(output, PrettyWriterOptions);

        doc.RootElement.WriteTo(writer);
        writer.Flush();

        return Encoding.UTF8.GetString(output.ToArray());
    }
}
