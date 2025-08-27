using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Microsoft.CommandPalette.Extensions.Toolkit;

public sealed class IntSetting : Setting<int>
{
    public int? Min { get; set; }
    public int? Max { get; set; }
    public string Placeholder { get; set; } = string.Empty;

    private IntSetting()
        : base()
    {
        Value = 0;
    }

    public IntSetting(string key, int defaultValue, int? min = null, int? max = null)
        : base(key, defaultValue)
    {
        Min = min;
        Max = max;
    }

    public IntSetting(string key, string label, string description, int defaultValue,
                      int? min = null, int? max = null)
        : base(key, label, description, defaultValue)
    {
        Min = min;
        Max = max;
    }

    public override Dictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>
        {
            { "id", Key },
            { "type", "Input.Number" },
            { "title", Label },
            { "label", Description },
            { "value", Value },
            { "isRequired", IsRequired },
            { "errorMessage", ErrorMessage },
            { "placeholder", Placeholder },
        };

        if (Min.HasValue) dict["min"] = Min.Value;
        if (Max.HasValue) dict["max"] = Max.Value;

        return dict;
    }

    public static IntSetting LoadFromJson(JsonObject jsonObject) => new() { Value = jsonObject["value"]?.GetValue<int>() ?? 0 };

    public override void Update(JsonObject payload)
    {
        if (payload.TryGetPropertyValue(Key, out JsonNode? node) && node is not null)
        {
            if (node is JsonValue jsonValue && jsonValue.TryGetValue<int>(out var value))
            {
                Value = value;
            }
            else if (int.TryParse(node.ToString(), out value))
            {
                Value = value;
            }
        }

        if (Min.HasValue && Value < Min.Value) Value = Min.Value;
        if (Max.HasValue && Value > Max.Value) Value = Max.Value;
    }

    public override string ToState()
    {
        return $"\"{Key}\": {Value}";
    }
}
