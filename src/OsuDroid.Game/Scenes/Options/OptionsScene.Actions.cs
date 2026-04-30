using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Input;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private static readonly Dictionary<UiAction, OptionsSection> s_sectionActions = new()
    {
        [UiAction.OptionsSectionGeneral] = OptionsSection.General,
        [UiAction.OptionsSectionGameplay] = OptionsSection.Gameplay,
        [UiAction.OptionsSectionGraphics] = OptionsSection.Graphics,
        [UiAction.OptionsSectionAudio] = OptionsSection.Audio,
        [UiAction.OptionsSectionLibrary] = OptionsSection.Library,
        [UiAction.OptionsSectionInput] = OptionsSection.Input,
        [UiAction.OptionsSectionAdvanced] = OptionsSection.Advanced,
    };

    private static readonly Dictionary<UiAction, string> s_toggleActions = new()
    {
        [UiAction.OptionsToggleServerConnection] = "stayOnline",
        [UiAction.OptionsToggleLoadAvatar] = "loadAvatar",
        [UiAction.OptionsToggleAnnouncements] = "receiveAnnouncements",
        [UiAction.OptionsToggleMusicPreview] = "musicpreview",
        [UiAction.OptionsToggleShiftPitch] = "shiftPitchInRateChange",
        [UiAction.OptionsToggleBeatmapSounds] = "beatmapSounds",
    };

    public void SelectSection(OptionsSection section, VirtualViewport? viewport = null)
    {
        if (_activeSection == section)
        {
            return;
        }

        _activeSection = section;
        _contentScrollOffset = 0f;
        if (viewport is { } actualViewport)
        {
            RememberViewport(actualViewport);
            ClampScroll(actualViewport);
        }
    }

    public void HandleAction(UiAction action, VirtualViewport viewport)
    {
        RememberViewport(viewport);
        OptionsSection? selectedSection = SectionForAction(action);

        if (selectedSection is { } section)
        {
            SelectSection(section, viewport);
            _pendingSfxKey = "click-short";
            return;
        }

        string? toggleKey = ToggleKeyForAction(action);

        if (toggleKey is not null)
        {
            if (IsInteractive(toggleKey))
            {
                Toggle(toggleKey);
            }

            return;
        }

        if (UiActionGroups.TryGetOptionsActiveRowIndex(action, out int rowIndex))
        {
            HandleRowAction(rowIndex, viewport);
        }
    }

    private static OptionsSection? SectionForAction(UiAction action) =>
        s_sectionActions.TryGetValue(action, out OptionsSection section) ? section : null;

    private static string? ToggleKeyForAction(UiAction action) =>
        s_toggleActions.TryGetValue(action, out string? key) ? key : null;

    public bool GetBoolValue(string key) => _boolValues.TryGetValue(key, out bool value) && value;

    public int GetIntValue(string key) => _intValues.TryGetValue(key, out int value) ? value : 0;

    public string GetStringValue(string key) =>
        _stringValues.TryGetValue(key, out string? value) ? value : string.Empty;

    public void SetIntValue(string key, int value)
    {
        SettingsRow? row = AllRows().FirstOrDefault(candidate => candidate.Key == key);
        if (row is null)
        {
            return;
        }

        int normalized =
            row.Kind == SettingsRowKind.Select
                ? ClampSelectValue(row, value)
                : ClampSliderValue(row, value);
        _intValues[key] = normalized;
        _settingsStore?.SetInt(key, normalized);
    }

    internal static bool IsInteractive(SettingsRow row) => row.IsEnabled && !row.IsLocked;

    private static bool IsInteractive(string key)
    {
        SettingsRow? row = AllRows().FirstOrDefault(candidate => candidate.Key == key);
        return row is not null && IsInteractive(row);
    }

    private string GetInputDisplayValue(SettingsRow row)
    {
        string value = GetStringValue(row.Key);
        if (string.IsNullOrEmpty(value) && row.ValueKey is not null)
        {
            value = _localizer[row.ValueKey];
        }

        return IsPathInput(row.Key) ? OptionsPathDisplayFormatter.Format(value) : value;
    }

    private string GetSummaryText(SettingsRow row) =>
        _pathDefaults.UsesNativeDefaultSummaries
            ? row.Key switch
            {
                "corePath" => _localizer["Options_CorePathSummaryIos"],
                "skinTopPath" => _localizer.Format(
                    "Options_SkinTopPathSummaryIos",
                    OptionsPathDisplayFormatter.Format(_pathDefaults.SkinTopPath)
                ),
                "directory" => _localizer.Format(
                    "Options_DirectorySummaryIos",
                    OptionsPathDisplayFormatter.Format(_pathDefaults.SongsDirectory)
                ),
                _ => _localizer[row.SummaryKey],
            }
            : _localizer[row.SummaryKey];

    private void Toggle(string key)
    {
        if (_boolValues.TryGetValue(key, out bool value))
        {
            bool updated = !value;
            _boolValues[key] = updated;
            _settingsStore?.SetBool(key, updated);
            _changedSettingKey = key;
            _pendingSfxKey = updated ? "check-on" : "check-off";
        }
    }

    private void HandleRowAction(int rowIndex, VirtualViewport viewport)
    {
        SettingsRow[] rows = ActiveSectionData
            .Categories.SelectMany(category => category.Rows)
            .ToArray();
        if ((uint)rowIndex >= (uint)rows.Length)
        {
            return;
        }

        SettingsRow row = rows[rowIndex];
        if (!IsInteractive(row))
        {
            return;
        }

        switch (row.Kind)
        {
            case SettingsRowKind.Checkbox:
                Toggle(row.Key);
                break;

            case SettingsRowKind.Slider:
                StepSlider(row);
                _pendingSfxKey = "click-short";
                break;

            case SettingsRowKind.Input:
                FocusInput(row, rowIndex, viewport);
                _pendingSfxKey = "click-short";
                break;

            case SettingsRowKind.Select:
                CycleSelect(row);
                _pendingSfxKey = "click-short";
                break;

            case SettingsRowKind.Button:
                _changedSettingKey = row.Key;
                _pendingSfxKey = "click-short-confirm";
                break;
            default:
                break;
        }
    }

    private void StepSlider(SettingsRow row)
    {
        int current = GetIntValue(row.Key);
        int step = Math.Max(1, (row.Max - row.Min) / 10);
        int next = current + step;
        if (next > row.Max)
        {
            next = row.Min;
        }

        next = ClampSliderValue(row, next);
        _intValues[row.Key] = next;
        _settingsStore?.SetInt(row.Key, next);
        _changedSettingKey = row.Key;
    }

    private void CycleSelect(SettingsRow row)
    {
        int valueCount = row.ValueKeys?.Count ?? (row.ValueKey is null ? 0 : 1);
        if (valueCount <= 1)
        {
            return;
        }

        int current = ClampSelectValue(row, GetIntValue(row.Key));
        int next = (current + 1) % valueCount;
        _intValues[row.Key] = next;
        _settingsStore?.SetInt(row.Key, next);
        _changedSettingKey = row.Key;
    }

    private string GetSelectValue(SettingsRow row)
    {
        return row.ValueKeys is { Count: > 0 } valueKeys
                ? _localizer[valueKeys[ClampSelectValue(row, GetIntValue(row.Key))]]
            : row.ValueKey is null ? string.Empty
            : _localizer[row.ValueKey];
    }

    private void FocusInput(SettingsRow row, int rowIndex, VirtualViewport viewport)
    {
        UiRect? rowBounds = FindRowBounds(rowIndex, viewport);
        _textInputService.RequestTextInput(
            new TextInputRequest(
                GetStringValue(row.Key),
                text =>
                {
                    CompleteInputChange(row, text);
                },
                text =>
                {
                    CompleteInputChange(row, text);
                },
                rowBounds
            )
        );
    }

    private void CompleteInputChange(SettingsRow row, string? text)
    {
        string value = NormalizeInputValue(row, text);
        _stringValues[row.Key] = value;
        _settingsStore?.SetString(row.Key, value);
        _changedSettingKey = row.Key;
        _settingChanged?.Invoke(row.Key);
    }

    private string NormalizeInputValue(SettingsRow row, string? value) =>
        !IsPathInput(row.Key) ? value ?? string.Empty
        : string.IsNullOrWhiteSpace(value) ? InputDefaultValue(row)
        : _pathDefaults.NormalizePathValue(value.Trim());

    private string InputDefaultValue(SettingsRow row) =>
        IsPathInput(row.Key) ? _pathDefaults.GetDefaultValue(row.Key) : string.Empty;

    private static bool IsPathInput(string key) =>
        key is "corePath" or "skinTopPath" or "directory";

    private static int ClampSliderValue(SettingsRow row, int value) =>
        Math.Clamp(value, row.Min, row.Max);

    private static int ClampSelectValue(SettingsRow row, int value)
    {
        int valueCount = row.ValueKeys?.Count ?? (row.ValueKey is null ? 0 : 1);
        return valueCount <= 0 ? 0 : Math.Clamp(value, 0, valueCount - 1);
    }
}
