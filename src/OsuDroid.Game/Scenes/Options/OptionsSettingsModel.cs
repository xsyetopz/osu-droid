using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Scenes.Options;

internal enum SettingsRowKind
{
    Checkbox,
    Select,
    Input,
    Button,
    Slider,
}

internal enum SettingsRowAvailability
{
    Implemented,
    Locked,
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
    IReadOnlyList<string>? ValueKeys = null,
    SettingsRowAvailability Availability = SettingsRowAvailability.Implemented)
{
    public bool IsLocked => Availability == SettingsRowAvailability.Locked;
}

internal sealed record SettingsCategory(string TitleKey, IReadOnlyList<SettingsRow> Rows);

internal sealed record SettingsSection(OptionsSection Section, string Key, UiMaterialIcon Icon, UiAction Action, IReadOnlyList<SettingsCategory> Categories);

public sealed record OptionsPathDefaults(string CorePath, string SkinTopPath, string SongsDirectory, bool UsesNativeDefaultSummaries = false)
{
    public static OptionsPathDefaults Empty { get; } = new(string.Empty, string.Empty, string.Empty);

    public static OptionsPathDefaults FromPaths(DroidGamePathLayout paths) =>
        new(paths.CoreRoot, paths.Skin, paths.Songs, UsesNativeSummaryDefaults(paths.CoreRoot));

    public string GetDefaultValue(string key) => key switch
    {
        "corePath" => CorePath,
        "skinTopPath" => SkinTopPath,
        "directory" => SongsDirectory,
        _ => string.Empty,
    };

    public string NormalizePathValue(string value)
    {
        string legacyCorePath = GetLegacyCorePath();
        if (string.IsNullOrEmpty(legacyCorePath))
        {
            return value;
        }

        string normalizedValue = value.Replace('\\', '/');
        string normalizedLegacyCorePath = legacyCorePath.Replace('\\', '/');
        if (string.Equals(normalizedValue, normalizedLegacyCorePath, StringComparison.Ordinal))
        {
            return CorePath;
        }

        string legacyChildPrefix = $"{normalizedLegacyCorePath}/";
        return normalizedValue.StartsWith(legacyChildPrefix, StringComparison.Ordinal)
            ? CorePath + normalizedValue[normalizedLegacyCorePath.Length..]
            : value;
    }

    private static bool UsesNativeSummaryDefaults(string corePath)
    {
        string normalized = corePath.Replace('\\', '/');
        return normalized.Contains("/Library/osu!droid", StringComparison.Ordinal);
    }

    private string GetLegacyCorePath()
    {
        string? parent = Path.GetDirectoryName(CorePath);
        return string.IsNullOrWhiteSpace(parent)
            ? string.Empty
            : Path.Combine(parent, DroidPathRoots.LegacyCoreDirectoryName);
    }
}
