using System.Globalization;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using static OsuDroid.Game.UI.DroidUiMetrics;

namespace OsuDroid.Game.Scenes;

public sealed partial class OptionsScene
{
    private static readonly UiColor rootBackground = UiColor.Opaque(19, 19, 26);
    private static readonly UiColor appBarBackground = UiColor.Opaque(30, 30, 46);
    private static readonly UiColor selectedSection = UiColor.Opaque(54, 54, 83);
    private static readonly UiColor rowBackground = UiColor.Opaque(22, 22, 34);
    private static readonly UiColor inputBackground = UiColor.Opaque(54, 54, 83);
    private static readonly UiColor white = UiColor.Opaque(255, 255, 255);
    private static readonly UiColor secondaryText = UiColor.Opaque(178, 178, 204);
    private static readonly UiColor disabledWhite = UiColor.Opaque(235, 235, 245);
    private static readonly UiColor checkboxAccent = UiColor.Opaque(243, 115, 115);
    private static readonly UiColor sliderTrack = UiColor.Opaque(54, 54, 83);

    private readonly Dictionary<string, bool> boolValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> intValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> stringValues = new(StringComparer.Ordinal);
    private readonly GameLocalizer localizer;
    private readonly IGameSettingsStore? settingsStore;
    private readonly OptionsPathDefaults pathDefaults;
    private ITextInputService textInputService;
    private OptionsSection activeSection;
    private float contentScrollOffset;
    private float sectionScrollOffset;
    private string? pendingSfxKey;
    private string? changedSettingKey;
    private int? activeSliderRowIndex;

    public OptionsScene(GameLocalizer localizer, IGameSettingsStore? settingsStore = null, ITextInputService? textInputService = null, OptionsPathDefaults? pathDefaults = null)
    {
        this.localizer = localizer;
        this.settingsStore = settingsStore;
        this.pathDefaults = pathDefaults ?? OptionsPathDefaults.Empty;
        this.textInputService = textInputService ?? new NoOpTextInputService();
        foreach (var row in sections.SelectMany(section => section.Categories).SelectMany(category => category.Rows))
        {
            if (row.Kind == SettingsRowKind.Checkbox)
                boolValues[row.Key] = settingsStore?.GetBool(row.Key, row.DefaultChecked) ?? row.DefaultChecked;
            else if (row.Kind == SettingsRowKind.Slider)
                intValues[row.Key] = ClampSliderValue(row, settingsStore?.GetInt(row.Key, row.DefaultValue) ?? row.DefaultValue);
            else if (row.Kind == SettingsRowKind.Select)
                intValues[row.Key] = ClampSelectValue(row, settingsStore?.GetInt(row.Key, row.DefaultValue) ?? row.DefaultValue);
            else if (row.Kind == SettingsRowKind.Input)
                stringValues[row.Key] = NormalizeInputValue(row, settingsStore?.GetString(row.Key, InputDefaultValue(row)) ?? InputDefaultValue(row));
        }
    }

    public OptionsSection ActiveSection => activeSection;

    public float ScrollOffset => contentScrollOffset;

    public float ContentScrollOffset => contentScrollOffset;

    public float SectionScrollOffset => sectionScrollOffset;

    public IReadOnlyList<string> Sections => sections.Select(section => localizer.Get(section.Key)).ToArray();

    public static IReadOnlyList<OptionsSection> AllSections => sections.Select(section => section.Section).ToArray();

    public IReadOnlyList<string> GeneralRows => generalCategories.SelectMany(category => category.Rows).Select(row => localizer.Get(row.TitleKey)).ToArray();

    public IReadOnlyList<string> GeneralCategories => generalCategories.Select(category => localizer.Get(category.TitleKey)).ToArray();

    public IReadOnlyList<string> ActiveRows => ActiveSectionData.Categories.SelectMany(category => category.Rows).Select(row => localizer.Get(row.TitleKey)).ToArray();

    public IReadOnlyList<string> ActiveCategories => ActiveSectionData.Categories.Select(category => localizer.Get(category.TitleKey)).ToArray();

    public void SetTextInputService(ITextInputService service) => textInputService = service;

    public string? ConsumePendingSfxKey()
    {
        var key = pendingSfxKey;
        pendingSfxKey = null;
        return key;
    }

    public string? ConsumeChangedSettingKey()
    {
        var key = changedSettingKey;
        changedSettingKey = null;
        return key;
    }

    public static float MaxScrollOffset(VirtualViewport viewport) => MaxContentScrollOffset(viewport);

    public static float MaxContentScrollOffset(VirtualViewport viewport) => Math.Max(0f, CalculateContentHeight(generalCategories) - VisibleContentHeight(viewport));

    public static float MaxSectionScrollOffset(VirtualViewport viewport) => Math.Max(0f, CalculateSectionHeight() - VisibleContentHeight(viewport));

    public static bool IsSectionScrollPoint(UiPoint point) => point.X >= ContentPaddingX && point.X <= ContentPaddingX + SectionRailWidth;

    public void SelectSection(OptionsSection section, VirtualViewport? viewport = null)
    {
        if (activeSection == section)
            return;

        activeSection = section;
        contentScrollOffset = 0f;
        if (viewport is { } actualViewport)
            ClampScroll(actualViewport);
    }

    public void HandleAction(UiAction action, VirtualViewport viewport)
    {
        switch (action)
        {
            case UiAction.OptionsSectionGeneral:
                SelectSection(OptionsSection.General, viewport);
                pendingSfxKey = "click-short";
                break;

            case UiAction.OptionsSectionGameplay:
                SelectSection(OptionsSection.Gameplay, viewport);
                pendingSfxKey = "click-short";
                break;

            case UiAction.OptionsSectionGraphics:
                SelectSection(OptionsSection.Graphics, viewport);
                pendingSfxKey = "click-short";
                break;

            case UiAction.OptionsSectionAudio:
                SelectSection(OptionsSection.Audio, viewport);
                pendingSfxKey = "click-short";
                break;

            case UiAction.OptionsSectionLibrary:
                SelectSection(OptionsSection.Library, viewport);
                pendingSfxKey = "click-short";
                break;

            case UiAction.OptionsSectionInput:
                SelectSection(OptionsSection.Input, viewport);
                pendingSfxKey = "click-short";
                break;

            case UiAction.OptionsSectionAdvanced:
                SelectSection(OptionsSection.Advanced, viewport);
                pendingSfxKey = "click-short";
                break;

            case UiAction.OptionsToggleServerConnection:
                if (!IsInteractive("stayOnline"))
                    break;
                Toggle("stayOnline");
                break;

            case UiAction.OptionsToggleLoadAvatar:
                if (!IsInteractive("loadAvatar"))
                    break;
                Toggle("loadAvatar");
                break;

            case UiAction.OptionsToggleAnnouncements:
                if (!IsInteractive("receiveAnnouncements"))
                    break;
                Toggle("receiveAnnouncements");
                break;

            case UiAction.OptionsToggleMusicPreview:
                if (!IsInteractive("musicpreview"))
                    break;
                Toggle("musicpreview");
                break;

            case UiAction.OptionsToggleShiftPitch:
                if (!IsInteractive("shiftPitchInRateChange"))
                    break;
                Toggle("shiftPitchInRateChange");
                break;

            case UiAction.OptionsToggleBeatmapSounds:
                if (!IsInteractive("beatmapSounds"))
                    break;
                Toggle("beatmapSounds");
                break;

            default:
                if (UiActionGroups.TryGetOptionsRowIndex(action, out var rowIndex))
                    HandleRowAction(rowIndex, viewport);
                break;
        }
    }

    public bool GetBoolValue(string key) => boolValues.TryGetValue(key, out var value) && value;

    public int GetIntValue(string key) => intValues.TryGetValue(key, out var value) ? value : 0;

    public string GetStringValue(string key) => stringValues.TryGetValue(key, out var value) ? value : string.Empty;

    public void SetIntValue(string key, int value)
    {
        var row = sections.SelectMany(section => section.Categories).SelectMany(category => category.Rows).FirstOrDefault(candidate => candidate.Key == key);
        if (row is null)
            return;

        var normalized = row.Kind == SettingsRowKind.Select ? ClampSelectValue(row, value) : ClampSliderValue(row, value);
        intValues[key] = normalized;
        settingsStore?.SetInt(key, normalized);
    }

    internal static bool IsInteractive(SettingsRow row) => row.IsEnabled && !row.IsLocked;

    private static bool IsInteractive(string key)
    {
        var row = sections.SelectMany(section => section.Categories).SelectMany(category => category.Rows).FirstOrDefault(candidate => candidate.Key == key);
        return row is not null && IsInteractive(row);
    }

    private string GetInputDisplayValue(SettingsRow row)
    {
        var value = GetStringValue(row.Key);
        if (string.IsNullOrEmpty(value) && row.ValueKey is not null)
            value = localizer[row.ValueKey];
        return IsPathInput(row.Key) ? OptionsPathDisplayFormatter.Format(value) : value;
    }

    public void Scroll(float deltaY, VirtualViewport viewport) => Scroll(deltaY, new UiPoint(ContentPaddingX + SectionRailWidth + ListGap, ContentTop), viewport);

    public void Scroll(float deltaY, UiPoint point, VirtualViewport viewport)
    {
        if (activeSliderRowIndex is not null)
            return;

        if (IsSectionScrollPoint(point))
            sectionScrollOffset = Math.Clamp(sectionScrollOffset + deltaY, 0f, MaxSectionScrollOffset(viewport));
        else
            contentScrollOffset = Math.Clamp(contentScrollOffset + deltaY, 0f, MaxActiveContentScrollOffset(viewport));
    }

    public bool TryBeginSliderDrag(string elementId, UiPoint point, VirtualViewport viewport)
    {
        if (!TryParseSliderRowIndex(elementId, out var rowIndex))
            return false;

        var row = RowAt(rowIndex);
        if (row?.Kind != SettingsRowKind.Slider || !IsInteractive(row))
            return false;

        activeSliderRowIndex = rowIndex;
        return UpdateSliderDrag(point, viewport);
    }

    public bool UpdateSliderDrag(UiPoint point, VirtualViewport viewport)
    {
        if (activeSliderRowIndex is not int rowIndex)
            return false;

        var row = RowAt(rowIndex);
        var bounds = FindRowBounds(rowIndex, viewport);
        if (row is null || bounds is null)
            return false;

        var next = SliderValueAtPoint(row, bounds.Value, point.X);
        if (GetIntValue(row.Key) == next)
            return true;

        intValues[row.Key] = next;
        settingsStore?.SetInt(row.Key, next);
        changedSettingKey = row.Key;
        return true;
    }

    public void EndSliderDrag(UiPoint point, VirtualViewport viewport)
    {
        UpdateSliderDrag(point, viewport);
        activeSliderRowIndex = null;
    }

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport)
    {
        ClampScroll(viewport);
        return CreateSnapshot(viewport, ActiveSectionData, contentScrollOffset, sectionScrollOffset);
    }

    public GameFrameSnapshot CreateSnapshotForSection(OptionsSection section, VirtualViewport viewport)
    {
        var sectionData = sections.Single(settingsSection => settingsSection.Section == section);
        return CreateSnapshot(viewport, sectionData, 0f, 0f);
    }

    private GameFrameSnapshot CreateSnapshot(VirtualViewport viewport, SettingsSection sectionData, float activeContentScrollOffset, float activeSectionScrollOffset)
    {
        return new GameFrameSnapshot(
            "Options",
            localizer["Options_Title"],
            localizer["Options_Subtitle"],
            Sections,
            (int)sectionData.Section,
            false,
            CreateUiFrame(viewport, sectionData, activeContentScrollOffset, activeSectionScrollOffset));
    }

    private SettingsSection ActiveSectionData => sections.Single(section => section.Section == activeSection);

    private void Toggle(string key)
    {
        if (boolValues.TryGetValue(key, out var value))
        {
            var updated = !value;
            boolValues[key] = updated;
            settingsStore?.SetBool(key, updated);
            changedSettingKey = key;
            pendingSfxKey = updated ? "check-on" : "check-off";
        }
    }

    private void HandleRowAction(int rowIndex, VirtualViewport viewport)
    {
        var rows = ActiveSectionData.Categories.SelectMany(category => category.Rows).ToArray();
        if ((uint)rowIndex >= (uint)rows.Length)
            return;

        var row = rows[rowIndex];
        if (!IsInteractive(row))
            return;

        switch (row.Kind)
        {
            case SettingsRowKind.Checkbox:
                Toggle(row.Key);
                break;

            case SettingsRowKind.Slider:
                StepSlider(row);
                pendingSfxKey = "click-short";
                break;

            case SettingsRowKind.Input:
                FocusInput(row, rowIndex, viewport);
                pendingSfxKey = "click-short";
                break;

            case SettingsRowKind.Select:
                CycleSelect(row);
                pendingSfxKey = "click-short";
                break;

            case SettingsRowKind.Button:
                changedSettingKey = row.Key;
                pendingSfxKey = "click-short-confirm";
                break;
        }
    }

    private void StepSlider(SettingsRow row)
    {
        var current = GetIntValue(row.Key);
        var step = Math.Max(1, (row.Max - row.Min) / 10);
        var next = current + step;
        if (next > row.Max)
            next = row.Min;
        next = ClampSliderValue(row, next);
        intValues[row.Key] = next;
        settingsStore?.SetInt(row.Key, next);
        changedSettingKey = row.Key;
    }

    private void CycleSelect(SettingsRow row)
    {
        var valueCount = row.ValueKeys?.Count ?? (row.ValueKey is null ? 0 : 1);
        if (valueCount <= 1)
            return;

        var current = ClampSelectValue(row, GetIntValue(row.Key));
        var next = (current + 1) % valueCount;
        intValues[row.Key] = next;
        settingsStore?.SetInt(row.Key, next);
        changedSettingKey = row.Key;
    }

    private string GetSelectValue(SettingsRow row)
    {
        if (row.ValueKeys is { Count: > 0 } valueKeys)
            return localizer[valueKeys[ClampSelectValue(row, GetIntValue(row.Key))]];

        return row.ValueKey is null ? string.Empty : localizer[row.ValueKey];
    }

    private void FocusInput(SettingsRow row, int rowIndex, VirtualViewport viewport)
    {
        var rowBounds = FindRowBounds(rowIndex, viewport);
        textInputService.RequestTextInput(new TextInputRequest(
            GetStringValue(row.Key),
            text =>
            {
                var value = NormalizeInputValue(row, text);
                stringValues[row.Key] = value;
                settingsStore?.SetString(row.Key, value);
                changedSettingKey = row.Key;
            },
            text =>
            {
                var value = NormalizeInputValue(row, text);
                stringValues[row.Key] = value;
                settingsStore?.SetString(row.Key, value);
                changedSettingKey = row.Key;
            },
            rowBounds));
    }

    private string NormalizeInputValue(SettingsRow row, string? value)
    {
        if (!IsPathInput(row.Key))
            return value ?? string.Empty;

        return string.IsNullOrWhiteSpace(value) ? InputDefaultValue(row) : value.Trim();
    }

    private string InputDefaultValue(SettingsRow row) =>
        IsPathInput(row.Key) ? pathDefaults.GetDefaultValue(row.Key) : string.Empty;

    private static bool IsPathInput(string key) =>
        key is "corePath" or "skinTopPath" or "directory";

    private static int ClampSliderValue(SettingsRow row, int value) => Math.Clamp(value, row.Min, row.Max);

    private static int ClampSelectValue(SettingsRow row, int value)
    {
        var valueCount = row.ValueKeys?.Count ?? (row.ValueKey is null ? 0 : 1);
        return valueCount <= 0 ? 0 : Math.Clamp(value, 0, valueCount - 1);
    }

    private UiRect? FindRowBounds(int targetRowIndex, VirtualViewport viewport)
    {
        var listX = ContentPaddingX + SectionRailWidth + ListGap;
        var listWidth = viewport.VirtualWidth - listX - ContentPaddingX;
        var y = ContentTop - contentScrollOffset;
        var rowIndex = 0;

        foreach (var category in ActiveSectionData.Categories)
        {
            y += CategoryTopMargin + CategoryHeaderHeight;
            foreach (var row in category.Rows)
            {
                var rowHeight = GetRowHeight(row);
                if (rowIndex == targetRowIndex)
                    return new UiRect(listX, y, listWidth, rowHeight);
                y += rowHeight;
                rowIndex++;
            }
        }

        return null;
    }

    private void ClampScroll(VirtualViewport viewport)
    {
        contentScrollOffset = Math.Clamp(contentScrollOffset, 0f, MaxActiveContentScrollOffset(viewport));
        sectionScrollOffset = Math.Clamp(sectionScrollOffset, 0f, MaxSectionScrollOffset(viewport));
    }

    private float MaxActiveContentScrollOffset(VirtualViewport viewport) => Math.Max(0f, CalculateContentHeight(ActiveSectionData.Categories) - VisibleContentHeight(viewport));

    private SettingsRow? RowAt(int rowIndex)
    {
        var rows = ActiveSectionData.Categories.SelectMany(category => category.Rows).ToArray();
        return (uint)rowIndex < (uint)rows.Length ? rows[rowIndex] : null;
    }

    private static bool TryParseSliderRowIndex(string elementId, out int rowIndex)
    {
        rowIndex = -1;
        const string prefix = "options-row-";
        const string infix = "-slider-";
        if (!elementId.StartsWith(prefix, StringComparison.Ordinal))
            return false;

        var suffixIndex = elementId.IndexOf(infix, prefix.Length, StringComparison.Ordinal);
        if (suffixIndex < 0)
            return false;

        return int.TryParse(elementId.AsSpan(prefix.Length, suffixIndex - prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out rowIndex);
    }

    private static int SliderValueAtPoint(SettingsRow row, UiRect bounds, float pointX)
    {
        var containerWidth = bounds.Width - SeekbarContainerMarginX * 2f;
        var controlWidth = Math.Min(ControlColumnWidth, Math.Max(96f * DpScale, containerWidth * 0.44f));
        var trackX = bounds.Right - SeekbarContainerMarginX - controlWidth;
        var normalized = Math.Clamp((pointX - trackX) / controlWidth, 0f, 1f);
        return ClampSliderValue(row, (int)MathF.Round(row.Min + normalized * (row.Max - row.Min)));
    }

}
