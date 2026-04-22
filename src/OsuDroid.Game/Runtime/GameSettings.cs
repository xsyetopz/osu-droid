using System.Text.Json;

namespace OsuDroid.Game.Runtime;

public interface IGameSettingsStore
{
    bool GetBool(string key, bool defaultValue);

    int GetInt(string key, int defaultValue);

    string GetString(string key, string defaultValue);

    void SetBool(string key, bool value);

    void SetInt(string key, int value);

    void SetString(string key, string value);
}

public sealed class JsonGameSettingsStore(string filePath) : IGameSettingsStore
{
    private readonly object gate = new();
    private Dictionary<string, JsonElement>? values;

    public bool GetBool(string key, bool defaultValue)
    {
        lock (gate)
        {
            EnsureLoaded();
            if (values is null || !values.TryGetValue(key, out var element))
                return defaultValue;

            return element.ValueKind == JsonValueKind.True || (element.ValueKind == JsonValueKind.False ? false : defaultValue);
        }
    }

    public int GetInt(string key, int defaultValue)
    {
        lock (gate)
        {
            EnsureLoaded();
            if (values is null || !values.TryGetValue(key, out var element))
                return defaultValue;

            return element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var value) ? value : defaultValue;
        }
    }

    public void SetBool(string key, bool value)
    {
        lock (gate)
        {
            EnsureLoaded();
            values![key] = JsonDocument.Parse(value ? "true" : "false").RootElement.Clone();
            Save();
        }
    }

    public void SetInt(string key, int value)
    {
        lock (gate)
        {
            EnsureLoaded();
            values![key] = JsonDocument.Parse(value.ToString(System.Globalization.CultureInfo.InvariantCulture)).RootElement.Clone();
            Save();
        }
    }

    public string GetString(string key, string defaultValue)
    {
        lock (gate)
        {
            EnsureLoaded();
            if (values is null || !values.TryGetValue(key, out var element))
                return defaultValue;

            return element.ValueKind == JsonValueKind.String ? element.GetString() ?? defaultValue : defaultValue;
        }
    }

    public void SetString(string key, string value)
    {
        lock (gate)
        {
            EnsureLoaded();
            values![key] = JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement.Clone();
            Save();
        }
    }

    private void EnsureLoaded()
    {
        if (values is not null)
            return;

        if (!File.Exists(filePath))
        {
            values = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(filePath));
            values = document.RootElement.ValueKind == JsonValueKind.Object
                ? document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal)
                : new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        }
        catch
        {
            values = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        }
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var payload = values!.ToDictionary(pair => pair.Key, pair => pair.Value);
        File.WriteAllText(filePath, JsonSerializer.Serialize(payload));
    }
}
