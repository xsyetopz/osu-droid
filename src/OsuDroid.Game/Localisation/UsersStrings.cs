using osu.Framework.Localisation;

namespace OsuDroid.Game.Localisation;

public static class UsersStrings
{
    private const string Prefix = "osu.Game.Resources.Localisation.Web.Users";

    public static LocalisableString LoginUsername => new TranslatableString(Key("login_username"), "username");
    public static LocalisableString LoginPassword => new TranslatableString(Key("login_password"), "password");
    public static LocalisableString LoginButton => new TranslatableString(Key("login_button"), "log in");

    private static string Key(string key) => $"{Prefix}:{key}";
}
