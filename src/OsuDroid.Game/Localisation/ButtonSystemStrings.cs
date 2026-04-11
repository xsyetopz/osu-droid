using osu.Framework.Localisation;

namespace OsuDroid.Game.Localisation;

public static class ButtonSystemStrings
{
    private const string Prefix = "osu.Game.Resources.Localisation.ButtonSystem";

    public static LocalisableString Play => new TranslatableString(Key("play"), "play");
    public static LocalisableString Solo => new TranslatableString(Key("solo"), "solo");
    public static LocalisableString Browse => new TranslatableString(Key("browse"), "browse");
    public static LocalisableString Back => new TranslatableString(Key("back"), "back");

    private static string Key(string key) => $"{Prefix}:{key}";
}
