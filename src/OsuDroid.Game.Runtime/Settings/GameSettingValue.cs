namespace OsuDroid.Game.Runtime.Settings;

public readonly record struct GameSettingValue(
    GameSettingValueKind Kind,
    bool BoolValue,
    int IntValue,
    string TextValue
)
{
    public static GameSettingValue FromBool(bool settingValue) =>
        new(GameSettingValueKind.Flag, settingValue, 0, string.Empty);

    public static GameSettingValue FromInt(int settingValue) =>
        new(GameSettingValueKind.Number, false, settingValue, string.Empty);

    public static GameSettingValue FromString(string settingValue) =>
        new(GameSettingValueKind.Text, false, 0, settingValue);

    public object ToJsonValue() =>
        Kind switch
        {
            GameSettingValueKind.Flag => BoolValue,
            GameSettingValueKind.Number => IntValue,
            GameSettingValueKind.Text => TextValue,
            _ => TextValue,
        };
}
