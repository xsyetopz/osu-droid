using System.Text.Json;

namespace OsuDroid.Game.Runtime.Settings;

public sealed class JsonGameSettingsStore(string filePath) : IExportableGameSettingsStore
{
    private readonly object _gate = new();
    private Dictionary<string, JsonElement>? _values;

    public bool GetBool(string key, bool defaultValue)
    {
        lock (_gate)
        {
            EnsureLoaded();
            return _values is null || !_values.TryGetValue(key, out JsonElement element)
                ? defaultValue
                : element.ValueKind == JsonValueKind.True || (element.ValueKind == JsonValueKind.False ? false : defaultValue);
        }
    }

    public int GetInt(string key, int defaultValue)
    {
        lock (_gate)
        {
            EnsureLoaded();
            return _values is null || !_values.TryGetValue(key, out JsonElement element)
                ? defaultValue
                : element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out int value) ? value : defaultValue;
        }
    }

    public void SetBool(string key, bool value)
    {
        lock (_gate)
        {
            EnsureLoaded();
            _values![key] = JsonDocument.Parse(value ? "true" : "false").RootElement.Clone();
            Save();
        }
    }

    public void SetInt(string key, int value)
    {
        lock (_gate)
        {
            EnsureLoaded();
            _values![key] = JsonDocument.Parse(value.ToString(System.Globalization.CultureInfo.InvariantCulture)).RootElement.Clone();
            Save();
        }
    }

    public string GetString(string key, string defaultValue)
    {
        lock (_gate)
        {
            EnsureLoaded();
            return _values is null || !_values.TryGetValue(key, out JsonElement element)
                ? defaultValue
                : element.ValueKind == JsonValueKind.String ? element.GetString() ?? defaultValue : defaultValue;
        }
    }

    public void SetString(string key, string value)
    {
        lock (_gate)
        {
            EnsureLoaded();
            _values![key] = JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement.Clone();
            Save();
        }
    }

    public IReadOnlyDictionary<string, GameSettingValue> GetAll()
    {
        lock (_gate)
        {
            EnsureLoaded();
            var settings = new Dictionary<string, GameSettingValue>(StringComparer.Ordinal);
            foreach ((string key, JsonElement element) in _values!)
            {
                if (TryReadSettingValue(element, out GameSettingValue settingValue))
                {
                    settings[key] = settingValue;
                }
            }

            return settings;
        }
    }

    public void SetMany(IReadOnlyDictionary<string, GameSettingValue> settings)
    {
        lock (_gate)
        {
            EnsureLoaded();
            foreach ((string key, GameSettingValue settingValue) in settings)
            {
                _values![key] = JsonDocument.Parse(JsonSerializer.Serialize(settingValue.ToJsonValue())).RootElement.Clone();
            }

            Save();
        }
    }

    private void EnsureLoaded()
    {
        if (_values is not null)
        {
            return;
        }

        if (!File.Exists(filePath))
        {
            _values = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(filePath));
            _values = document.RootElement.ValueKind == JsonValueKind.Object
                ? document.RootElement.EnumerateObject().ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal)
                : new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        }
        catch
        {
            _values = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        }
    }

    private void Save()
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var payload = _values!.ToDictionary(pair => pair.Key, pair => pair.Value);
        File.WriteAllText(filePath, JsonSerializer.Serialize(payload));
    }

    public static bool TryReadSettingValue(JsonElement element, out GameSettingValue settingValue)
    {
        if (element.ValueKind == JsonValueKind.True)
        {
            settingValue = GameSettingValue.FromBool(true);
            return true;
        }

        if (element.ValueKind == JsonValueKind.False)
        {
            settingValue = GameSettingValue.FromBool(false);
            return true;
        }

        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out int intValue))
        {
            settingValue = GameSettingValue.FromInt(intValue);
            return true;
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            settingValue = GameSettingValue.FromString(element.GetString() ?? string.Empty);
            return true;
        }

        settingValue = default;
        return false;
    }
}
