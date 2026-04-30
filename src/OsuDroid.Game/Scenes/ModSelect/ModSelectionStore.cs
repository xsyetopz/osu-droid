using System.Text.Json;
using OsuDroid.Game.Runtime.Settings;

namespace OsuDroid.Game.Scenes.ModSelect;

public static class ModSelectionStore
{
    public const string SelectedModSettingsSettingKey = "selectedModSettings";

    public static IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> LoadSettings(
        IGameSettingsStore store
    )
    {
        string json = store.GetString(SelectedModSettingsSettingKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, IReadOnlyDictionary<string, string>>(
                StringComparer.OrdinalIgnoreCase
            );
        }

        try
        {
            Dictionary<string, Dictionary<string, string>>? values = JsonSerializer.Deserialize<
                Dictionary<string, Dictionary<string, string>>
            >(json);
            return values?.ToDictionary(
                    pair => pair.Key,
                    pair => (IReadOnlyDictionary<string, string>)pair.Value,
                    StringComparer.OrdinalIgnoreCase
                )
                ?? new Dictionary<string, IReadOnlyDictionary<string, string>>(
                    StringComparer.OrdinalIgnoreCase
                );
        }
        catch (JsonException)
        {
            return new Dictionary<string, IReadOnlyDictionary<string, string>>(
                StringComparer.OrdinalIgnoreCase
            );
        }
    }

    public static void SaveSettings(
        IGameSettingsStore store,
        IReadOnlyDictionary<string, Dictionary<string, string>> settings
    ) => store.SetString(SelectedModSettingsSettingKey, JsonSerializer.Serialize(settings));
}
