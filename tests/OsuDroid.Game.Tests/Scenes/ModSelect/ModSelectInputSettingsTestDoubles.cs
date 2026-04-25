using OsuDroid.Game.Runtime.Settings;
using OsuDroid.Game.UI.Input;

namespace OsuDroid.Game.Tests;

public sealed partial class ModSelectSceneTests
{

    private sealed class RecordingTextInputService : ITextInputService
    {
        public TextInputRequest? LastRequest { get; private set; }

        public void RequestTextInput(TextInputRequest request) => LastRequest = request;

        public void HideTextInput()
        {
        }
    }

    private sealed class MemorySettingsStore : IGameSettingsStore
    {
        private readonly Dictionary<string, GameSettingValue> _settings = new(StringComparer.Ordinal);

        public bool GetBool(string key, bool defaultValue) =>
            _settings.TryGetValue(key, out GameSettingValue setting) && setting.Kind == GameSettingValueKind.Flag ? setting.BoolValue : defaultValue;

        public int GetInt(string key, int defaultValue) =>
            _settings.TryGetValue(key, out GameSettingValue setting) && setting.Kind == GameSettingValueKind.Number ? setting.IntValue : defaultValue;

        public string GetString(string key, string defaultValue) =>
            _settings.TryGetValue(key, out GameSettingValue setting) && setting.Kind == GameSettingValueKind.Text ? setting.TextValue : defaultValue;

        public void SetBool(string key, bool value) => _settings[key] = GameSettingValue.FromBool(value);

        public void SetInt(string key, int value) => _settings[key] = GameSettingValue.FromInt(value);

        public void SetString(string key, string value) => _settings[key] = GameSettingValue.FromString(value);
    }
}
