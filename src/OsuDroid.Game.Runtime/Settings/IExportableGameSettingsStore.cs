namespace OsuDroid.Game.Runtime.Settings;

public interface IExportableGameSettingsStore : IGameSettingsStore
{
    IReadOnlyDictionary<string, GameSettingValue> GetAll();

    void SetMany(IReadOnlyDictionary<string, GameSettingValue> settings);
}
