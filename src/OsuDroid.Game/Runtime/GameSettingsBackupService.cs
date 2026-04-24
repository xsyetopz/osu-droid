using System.Text.Json;
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Runtime;

public sealed class GameSettingsBackupService(DroidGamePathLayout paths, IGameSettingsStore settingsStore)
{
    private static readonly HashSet<string> s_sensitiveKeys = new(StringComparer.Ordinal)
    {
        "installID",
        "onlineUsername",
        "onlinePassword",
        "starRatingVersion",
        "version",
    };

    private static readonly JsonSerializerOptions s_jsonOptions = new() { WriteIndented = true };

    public string BackupPath => Path.Combine(paths.CoreRoot, "osudroid.cfg");

    public bool Export()
    {
        if (settingsStore is not IExportableGameSettingsStore exportableStore)
        {
            return false;
        }

        try
        {
            string? directory = Path.GetDirectoryName(BackupPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var payload = exportableStore.GetAll()
                .Where(pair => !s_sensitiveKeys.Contains(pair.Key))
                .ToDictionary(pair => pair.Key, pair => pair.Value.ToJsonValue(), StringComparer.Ordinal);
            File.WriteAllText(BackupPath, JsonSerializer.Serialize(payload, s_jsonOptions));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool Import()
    {
        if (!File.Exists(BackupPath))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(BackupPath));
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            Dictionary<string, GameSettingValue> importedSettings = [];
            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                if (s_sensitiveKeys.Contains(property.Name) || !JsonGameSettingsStore.TryReadSettingValue(property.Value, out GameSettingValue settingValue))
                {
                    continue;
                }

                importedSettings[property.Name] = settingValue;
            }

            if (settingsStore is IExportableGameSettingsStore exportableStore)
            {
                exportableStore.SetMany(importedSettings);
            }
            else
            {
                ApplyImportedSettings(importedSettings);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ApplyImportedSettings(IReadOnlyDictionary<string, GameSettingValue> importedSettings)
    {
        foreach ((string key, GameSettingValue settingValue) in importedSettings)
        {
            switch (settingValue.Kind)
            {
                case GameSettingValueKind.Flag:
                    settingsStore.SetBool(key, settingValue.BoolValue);
                    break;
                case GameSettingValueKind.Number:
                    settingsStore.SetInt(key, settingValue.IntValue);
                    break;
                case GameSettingValueKind.Text:
                    settingsStore.SetString(key, settingValue.TextValue);
                    break;
                default:
                    break;
            }
        }
    }
}
