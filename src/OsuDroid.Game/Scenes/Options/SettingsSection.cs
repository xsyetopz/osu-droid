using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;

namespace OsuDroid.Game.Scenes.Options;

internal sealed record SettingsSection(
    OptionsSection Section,
    string Key,
    UiMaterialIcon Icon,
    UiAction Action,
    IReadOnlyList<SettingsCategory> Categories
);
