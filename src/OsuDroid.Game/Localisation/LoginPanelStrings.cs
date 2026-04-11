using osu.Framework.Localisation;

namespace OsuDroid.Game.Localisation;

public static class LoginPanelStrings
{
    private const string Prefix = "osu.Game.Resources.Localisation.LoginPanel";

    public static LocalisableString DoNotDisturb => new TranslatableString(Key("do_not_disturb"), "Do not disturb");
    public static LocalisableString AppearOffline => new TranslatableString(Key("appear_offline"), "Appear offline");
    public static LocalisableString SignedIn => new TranslatableString(Key("signed_in"), "Signed in");
    public static LocalisableString Account => new TranslatableString(Key("account"), "Account");
    public static LocalisableString SignOut => new TranslatableString(Key("sign_out"), "Sign out");
    public static LocalisableString RememberUsername => new TranslatableString(Key("remember_username"), "Remember username");
    public static LocalisableString StaySignedIn => new TranslatableString(Key("stay_signed_in"), "Stay signed in");
    public static LocalisableString Register => new TranslatableString(Key("register"), "Register");

    private static string Key(string key) => $"{Prefix}:{key}";
}
