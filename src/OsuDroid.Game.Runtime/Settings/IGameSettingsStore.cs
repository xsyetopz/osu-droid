namespace OsuDroid.Game.Runtime.Settings;

public interface IGameSettingsStore
{
    bool GetBool(string key, bool defaultValue);

    int GetInt(string key, int defaultValue);

    string GetString(string key, string defaultValue);

    void SetBool(string key, bool value);

    void SetInt(string key, int value);

    void SetString(string key, string value);
}
