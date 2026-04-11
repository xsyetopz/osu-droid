using osu.Framework.Localisation;

namespace OsuDroid.Game.Localisation;

public static class SongSelectStrings
{
    private const string Prefix = "osu.Game.Resources.Localisation.SongSelect";

    public static LocalisableString Mods => new TranslatableString(Key("mods"), "Mods");
    public static LocalisableString Random => new TranslatableString(Key("random"), "Random");
    public static LocalisableString Options => new TranslatableString(Key("options"), "Options");
    public static LocalisableString Sort => new TranslatableString(Key("sort"), "Sort");
    public static LocalisableString Group => new TranslatableString(Key("group"), "Group");
    public static LocalisableString Artist => new TranslatableString(Key("artist"), "Artist");
    public static LocalisableString Author => new TranslatableString(Key("author"), "Author");
    public static LocalisableString Difficulty => new TranslatableString(Key("difficulty"), "Difficulty");
    public static LocalisableString Length => new TranslatableString(Key("length"), "Length");
    public static LocalisableString Ranked => new TranslatableString(Key("ranked"), "Ranked");
    public static LocalisableString Details => new TranslatableString(Key("details"), "Details");
    public static LocalisableString NoMatchingBeatmaps => new TranslatableString(Key("no_matching_beatmaps"), "No matching beatmaps");
    public static LocalisableString NoMatchingBeatmapsDescription => new TranslatableString(Key("no_matching_beatmaps_description"), "No beatmaps match your filter criteria!");

    private static string Key(string key) => $"{Prefix}:{key}";
}
