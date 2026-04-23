using OsuDroid.Game.UI;
using OsuDroid.Game.Runtime.Paths;

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
    bool IsBottom = false,
    IReadOnlyList<string>? ValueKeys = null);

internal sealed record SettingsCategory(string TitleKey, IReadOnlyList<SettingsRow> Rows);

internal sealed record SettingsSection(OptionsSection Section, string Key, UiMaterialIcon Icon, UiAction Action, IReadOnlyList<SettingsCategory> Categories);

public sealed record OptionsPathDefaults(string CorePath, string SkinTopPath, string SongsDirectory)
{
    public static OptionsPathDefaults Empty { get; } = new(string.Empty, string.Empty, string.Empty);

    public static OptionsPathDefaults FromPaths(DroidGamePathLayout paths) =>
        new(paths.CoreRoot, paths.Skin, paths.Songs);

    public string GetDefaultValue(string key) => key switch
    {
        "corePath" => CorePath,
        "skinTopPath" => SkinTopPath,
        "directory" => SongsDirectory,
        _ => string.Empty,
    };
}
