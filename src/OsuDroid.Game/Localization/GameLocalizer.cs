using System.Globalization;
using System.Resources;

namespace OsuDroid.Game.Localization;

public sealed class GameLocalizer
{
    private readonly ResourceManager _resources = new(
        "OsuDroid.Game.Localization.Strings",
        typeof(GameLocalizer).Assembly
    );

    public string this[string key] => Get(key);

    public string Get(string key) => _resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    public string Format(string key, params object[] args) =>
        string.Format(CultureInfo.CurrentCulture, Get(key), args);
}
