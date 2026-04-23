using System.Globalization;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private static readonly UiColor s_rootBackground = UiColor.Opaque(19, 19, 26);
    private static readonly UiColor s_appBarBackground = UiColor.Opaque(30, 30, 46);
    private static readonly UiColor s_selectedSection = UiColor.Opaque(54, 54, 83);
    private static readonly UiColor s_rowBackground = UiColor.Opaque(22, 22, 34);
    private static readonly UiColor s_inputBackground = UiColor.Opaque(54, 54, 83);
    private static readonly UiColor s_white = UiColor.Opaque(255, 255, 255);
    private static readonly UiColor s_secondaryText = UiColor.Opaque(178, 178, 204);
    private static readonly UiColor s_disabledWhite = UiColor.Opaque(235, 235, 245);
    private static readonly UiColor s_checkboxAccent = UiColor.Opaque(243, 115, 115);
    private static readonly UiColor s_sliderTrack = UiColor.Opaque(54, 54, 83);

    private readonly Dictionary<string, bool> _boolValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _intValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _stringValues = new(StringComparer.Ordinal);
    private readonly GameLocalizer _localizer;
    private readonly IGameSettingsStore? _settingsStore;
    private readonly OptionsPathDefaults _pathDefaults;
    private ITextInputService _textInputService;
    private OptionsSection _activeSection;
    private float _contentScrollOffset;
    private float _sectionScrollOffset;
    private string? _pendingSfxKey;
    private string? _changedSettingKey;
    private int? _activeSliderRowIndex;

    public OptionsScene(GameLocalizer localizer, IGameSettingsStore? settingsStore = null, ITextInputService? textInputService = null, OptionsPathDefaults? pathDefaults = null)
    {
        _localizer = localizer;
        _settingsStore = settingsStore;
        _pathDefaults = pathDefaults ?? OptionsPathDefaults.Empty;
        _textInputService = textInputService ?? new NoOpTextInputService();
        foreach (SettingsRow? row in s_sections.SelectMany(section => section.Categories).SelectMany(category => category.Rows))
        {
            if (row.Kind == SettingsRowKind.Checkbox)
            {
                _boolValues[row.Key] = _settingsStore?.GetBool(row.Key, row.DefaultChecked) ?? row.DefaultChecked;
            }
            else if (row.Kind == SettingsRowKind.Slider)
            {
                _intValues[row.Key] = ClampSliderValue(row, _settingsStore?.GetInt(row.Key, row.DefaultValue) ?? row.DefaultValue);
            }
            else if (row.Kind == SettingsRowKind.Select)
            {
                _intValues[row.Key] = ClampSelectValue(row, _settingsStore?.GetInt(row.Key, row.DefaultValue) ?? row.DefaultValue);
            }
            else if (row.Kind == SettingsRowKind.Input)
            {
                _stringValues[row.Key] = NormalizeInputValue(row, _settingsStore?.GetString(row.Key, InputDefaultValue(row)) ?? InputDefaultValue(row));
            }
        }
    }

    public OptionsSection ActiveSection => _activeSection;

    public float ScrollOffset => _contentScrollOffset;

    public float ContentScrollOffset => _contentScrollOffset;

    public float SectionScrollOffset => _sectionScrollOffset;

    public IReadOnlyList<string> Sections => s_sections.Select(section => _localizer.Get(section.Key)).ToArray();

    public static IReadOnlyList<OptionsSection> AllSections => s_sections.Select(section => section.Section).ToArray();

    public IReadOnlyList<string> GeneralRows => s_generalCategories.SelectMany(category => category.Rows).Select(row => _localizer.Get(row.TitleKey)).ToArray();

    public IReadOnlyList<string> GeneralCategories => s_generalCategories.Select(category => _localizer.Get(category.TitleKey)).ToArray();

    public IReadOnlyList<string> ActiveRows => ActiveSectionData.Categories.SelectMany(category => category.Rows).Select(row => _localizer.Get(row.TitleKey)).ToArray();

    public IReadOnlyList<string> ActiveCategories => ActiveSectionData.Categories.Select(category => _localizer.Get(category.TitleKey)).ToArray();

    public void SetTextInputService(ITextInputService service) => _textInputService = service;

    public string? ConsumePendingSfxKey()
    {
        string? key = _pendingSfxKey;
        _pendingSfxKey = null;
        return key;
    }

    public string? ConsumeChangedSettingKey()
    {
        string? key = _changedSettingKey;
        _changedSettingKey = null;
        return key;
    }

    public static float MaxScrollOffset(VirtualViewport viewport) => MaxContentScrollOffset(viewport);

    public static float MaxContentScrollOffset(VirtualViewport viewport) => Math.Max(0f, CalculateContentHeight(s_generalCategories) - VisibleContentHeight(viewport));

    public static float MaxSectionScrollOffset(VirtualViewport viewport) => Math.Max(0f, CalculateSectionHeight() - VisibleContentHeight(viewport));

    public static bool IsSectionScrollPoint(UiPoint point) => point.X is >= ContentPaddingX and <= (ContentPaddingX + SectionRailWidth);

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
            ClampScroll(actualViewport);
        }
    }

    public void HandleAction(UiAction action, VirtualViewport viewport)
    {
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

        if (UiActionGroups.TryGetOptionsRowIndex(action, out int rowIndex))
        {
            HandleRowAction(rowIndex, viewport);
        }
    }

#pragma warning disable IDE0072 // UiAction has cross-scene members; Options maps only Options actions.
    private static OptionsSection? SectionForAction(UiAction action) => action switch
    {
        UiAction.OptionsSectionGeneral => OptionsSection.General,
        UiAction.OptionsSectionGameplay => OptionsSection.Gameplay,
        UiAction.OptionsSectionGraphics => OptionsSection.Graphics,
        UiAction.OptionsSectionAudio => OptionsSection.Audio,
        UiAction.OptionsSectionLibrary => OptionsSection.Library,
        UiAction.OptionsSectionInput => OptionsSection.Input,
        UiAction.OptionsSectionAdvanced => OptionsSection.Advanced,
        _ => null,
    };

    private static string? ToggleKeyForAction(UiAction action) => action switch
    {
        UiAction.OptionsToggleServerConnection => "stayOnline",
        UiAction.OptionsToggleLoadAvatar => "loadAvatar",
        UiAction.OptionsToggleAnnouncements => "receiveAnnouncements",
        UiAction.OptionsToggleMusicPreview => "musicpreview",
        UiAction.OptionsToggleShiftPitch => "shiftPitchInRateChange",
        UiAction.OptionsToggleBeatmapSounds => "beatmapSounds",
        _ => null,
    };
#pragma warning restore IDE0072

    public bool GetBoolValue(string key) => _boolValues.TryGetValue(key, out bool value) && value;

    public int GetIntValue(string key) => _intValues.TryGetValue(key, out int value) ? value : 0;

    public string GetStringValue(string key) => _stringValues.TryGetValue(key, out string? value) ? value : string.Empty;

    public void SetIntValue(string key, int value)
    {
        SettingsRow? row = s_sections.SelectMany(section => section.Categories).SelectMany(category => category.Rows).FirstOrDefault(candidate => candidate.Key == key);
        if (row is null)
        {
            return;
        }

        int normalized = row.Kind == SettingsRowKind.Select ? ClampSelectValue(row, value) : ClampSliderValue(row, value);
        _intValues[key] = normalized;
        _settingsStore?.SetInt(key, normalized);
    }

    internal static bool IsInteractive(SettingsRow row) => row.IsEnabled && !row.IsLocked;

    private static bool IsInteractive(string key)
    {
        SettingsRow? row = s_sections.SelectMany(section => section.Categories).SelectMany(category => category.Rows).FirstOrDefault(candidate => candidate.Key == key);
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

    public void Scroll(float deltaY, VirtualViewport viewport) => Scroll(deltaY, new UiPoint(ContentPaddingX + SectionRailWidth + ListGap, ContentTop), viewport);

    public void Scroll(float deltaY, UiPoint point, VirtualViewport viewport)
    {
        if (_activeSliderRowIndex is not null)
        {
            return;
        }

        if (IsSectionScrollPoint(point))
        {
            _sectionScrollOffset = Math.Clamp(_sectionScrollOffset + deltaY, 0f, MaxSectionScrollOffset(viewport));
        }
        else
        {
            _contentScrollOffset = Math.Clamp(_contentScrollOffset + deltaY, 0f, MaxActiveContentScrollOffset(viewport));
        }
    }

    public bool TryBeginSliderDrag(string elementId, UiPoint point, VirtualViewport viewport)
    {
        if (!TryParseSliderRowIndex(elementId, out int rowIndex))
        {
            return false;
        }

        SettingsRow? row = RowAt(rowIndex);
        if (row?.Kind != SettingsRowKind.Slider || !IsInteractive(row))
        {
            return false;
        }

        _activeSliderRowIndex = rowIndex;
        return UpdateSliderDrag(point, viewport);
    }

    public bool UpdateSliderDrag(UiPoint point, VirtualViewport viewport)
    {
        if (_activeSliderRowIndex is not int rowIndex)
        {
            return false;
        }

        SettingsRow? row = RowAt(rowIndex);
        UiRect? bounds = FindRowBounds(rowIndex, viewport);
        if (row is null || bounds is null)
        {
            return false;
        }

        int next = SliderValueAtPoint(row, bounds.Value, point.X);
        if (GetIntValue(row.Key) == next)
        {
            return true;
        }

        _intValues[row.Key] = next;
        _settingsStore?.SetInt(row.Key, next);
        _changedSettingKey = row.Key;
        return true;
    }

    public void EndSliderDrag(UiPoint point, VirtualViewport viewport)
    {
        UpdateSliderDrag(point, viewport);
        _activeSliderRowIndex = null;
    }

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport)
    {
        ClampScroll(viewport);
        return CreateSnapshot(viewport, ActiveSectionData, _contentScrollOffset, _sectionScrollOffset);
    }

    public GameFrameSnapshot CreateSnapshotForSection(OptionsSection section, VirtualViewport viewport)
    {
        SettingsSection sectionData = s_sections.Single(settingsSection => settingsSection.Section == section);
        return CreateSnapshot(viewport, sectionData, 0f, 0f);
    }

    private GameFrameSnapshot CreateSnapshot(VirtualViewport viewport, SettingsSection sectionData, float activeContentScrollOffset, float activeSectionScrollOffset)
    {
        return new GameFrameSnapshot(
            "Options",
            _localizer["Options_Title"],
            _localizer["Options_Subtitle"],
            Sections,
            (int)sectionData.Section,
            false,
            CreateUiFrame(viewport, sectionData, activeContentScrollOffset, activeSectionScrollOffset));
    }

    private SettingsSection ActiveSectionData => s_sections.Single(section => section.Section == _activeSection);

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
        SettingsRow[] rows = ActiveSectionData.Categories.SelectMany(category => category.Rows).ToArray();
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
            : row.ValueKey is null ? string.Empty : _localizer[row.ValueKey];
    }

    private void FocusInput(SettingsRow row, int rowIndex, VirtualViewport viewport)
    {
        UiRect? rowBounds = FindRowBounds(rowIndex, viewport);
        _textInputService.RequestTextInput(new TextInputRequest(
            GetStringValue(row.Key),
            text =>
            {
                string value = NormalizeInputValue(row, text);
                _stringValues[row.Key] = value;
                _settingsStore?.SetString(row.Key, value);
                _changedSettingKey = row.Key;
            },
            text =>
            {
                string value = NormalizeInputValue(row, text);
                _stringValues[row.Key] = value;
                _settingsStore?.SetString(row.Key, value);
                _changedSettingKey = row.Key;
            },
            rowBounds));
    }

    private string NormalizeInputValue(SettingsRow row, string? value) => !IsPathInput(row.Key) ? value ?? string.Empty : string.IsNullOrWhiteSpace(value) ? InputDefaultValue(row) : value.Trim();

    private string InputDefaultValue(SettingsRow row) =>
        IsPathInput(row.Key) ? _pathDefaults.GetDefaultValue(row.Key) : string.Empty;

    private static bool IsPathInput(string key) =>
        key is "corePath" or "skinTopPath" or "directory";

    private static int ClampSliderValue(SettingsRow row, int value) => Math.Clamp(value, row.Min, row.Max);

    private static int ClampSelectValue(SettingsRow row, int value)
    {
        int valueCount = row.ValueKeys?.Count ?? (row.ValueKey is null ? 0 : 1);
        return valueCount <= 0 ? 0 : Math.Clamp(value, 0, valueCount - 1);
    }

    private UiRect? FindRowBounds(int targetRowIndex, VirtualViewport viewport)
    {
        float listX = ContentPaddingX + SectionRailWidth + ListGap;
        float listWidth = viewport.VirtualWidth - listX - ContentPaddingX;
        float y = ContentTop - _contentScrollOffset;
        int rowIndex = 0;

        foreach (SettingsCategory category in ActiveSectionData.Categories)
        {
            y += CategoryTopMargin + CategoryHeaderHeight;
            foreach (SettingsRow row in category.Rows)
            {
                float rowHeight = GetRowHeight(row);
                if (rowIndex == targetRowIndex)
                {
                    return new UiRect(listX, y, listWidth, rowHeight);
                }

                y += rowHeight;
                rowIndex++;
            }
        }

        return null;
    }

    private void ClampScroll(VirtualViewport viewport)
    {
        _contentScrollOffset = Math.Clamp(_contentScrollOffset, 0f, MaxActiveContentScrollOffset(viewport));
        _sectionScrollOffset = Math.Clamp(_sectionScrollOffset, 0f, MaxSectionScrollOffset(viewport));
    }

    private float MaxActiveContentScrollOffset(VirtualViewport viewport) => Math.Max(0f, CalculateContentHeight(ActiveSectionData.Categories) - VisibleContentHeight(viewport));

    private SettingsRow? RowAt(int rowIndex)
    {
        SettingsRow[] rows = ActiveSectionData.Categories.SelectMany(category => category.Rows).ToArray();
        return (uint)rowIndex < (uint)rows.Length ? rows[rowIndex] : null;
    }

    private static bool TryParseSliderRowIndex(string elementId, out int rowIndex)
    {
        rowIndex = -1;
        const string prefix = "options-row-";
        const string infix = "-slider-";
        if (!elementId.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        int suffixIndex = elementId.IndexOf(infix, prefix.Length, StringComparison.Ordinal);
        return suffixIndex < 0
            ? false
            : int.TryParse(elementId.AsSpan(prefix.Length, suffixIndex - prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out rowIndex);
    }

    private static int SliderValueAtPoint(SettingsRow row, UiRect bounds, float pointX)
    {
        float trackWidth = bounds.Width - SeekbarTrackMarginX * 2f;
        float trackX = bounds.X + SeekbarTrackMarginX;
        float normalized = Math.Clamp((pointX - trackX) / trackWidth, 0f, 1f);
        return ClampSliderValue(row, (int)MathF.Round(row.Min + normalized * (row.Max - row.Min)));
    }

}
