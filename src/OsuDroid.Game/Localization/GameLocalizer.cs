using System.Globalization;
using System.Resources;

namespace OsuDroid.Game.Localization;

public sealed class GameLocalizer
{
    private readonly ResourceManager resources = new("OsuDroid.Game.Localization.Strings", typeof(GameLocalizer).Assembly);

    public string this[string key] => Get(key);

    public string Get(string key) => resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;
}
