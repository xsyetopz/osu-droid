using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

internal enum SettingsRowKind
{
    Checkbox,
    Select,
    Input,
    Button,
    Slider,
}

internal sealed record SettingsRow(
    string Key,
    string TitleKey,
    string SummaryKey,
    SettingsRowKind Kind,
    bool DefaultChecked = false,
    string? ValueKey = null,
    int Min = 0,
    int Max = 100,
    int DefaultValue = 0,
    bool IsEnabled = true,
    UiAction Action = UiAction.None,
    bool IsBottom = false);

internal sealed record SettingsCategory(string TitleKey, IReadOnlyList<SettingsRow> Rows);

internal sealed record SettingsSection(OptionsSection Section, string Key, UiMaterialIcon Icon, UiAction Action, IReadOnlyList<SettingsCategory> Categories);
