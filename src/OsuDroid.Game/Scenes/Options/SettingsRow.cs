using OsuDroid.Game.UI.Actions;
namespace OsuDroid.Game.Scenes.Options;

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

