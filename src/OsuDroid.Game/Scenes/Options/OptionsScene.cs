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
    private ITextInputService textInputService;
    private OptionsSection activeSection;
    private float contentScrollOffset;
    private float sectionScrollOffset;
    private string? pendingSfxKey;

    public OptionsScene(GameLocalizer localizer, IGameSettingsStore? settingsStore = null, ITextInputService? textInputService = null)
    {
        this.localizer = localizer;
        this.settingsStore = settingsStore;
        this.textInputService = textInputService ?? new NoOpTextInputService();
        foreach (var row in sections.SelectMany(section => section.Categories).SelectMany(category => category.Rows))
        {
            if (row.Kind == SettingsRowKind.Checkbox)
                boolValues[row.Key] = settingsStore?.GetBool(row.Key, row.DefaultChecked) ?? row.DefaultChecked;
            else if (row.Kind == SettingsRowKind.Slider)
                intValues[row.Key] = settingsStore?.GetInt(row.Key, row.DefaultValue) ?? row.DefaultValue;
            else if (row.Kind == SettingsRowKind.Input)
                stringValues[row.Key] = settingsStore?.GetString(row.Key, string.Empty) ?? string.Empty;
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
                Toggle("stayOnline");
                break;

            case UiAction.OptionsToggleLoadAvatar:
                Toggle("loadAvatar");
                break;

            case UiAction.OptionsToggleAnnouncements:
                Toggle("receiveAnnouncements");
                break;

            case UiAction.OptionsToggleMusicPreview:
                Toggle("musicpreview");
                break;

            case UiAction.OptionsToggleShiftPitch:
                Toggle("shiftPitchInRateChange");
                break;

            case UiAction.OptionsToggleBeatmapSounds:
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

    public void Scroll(float deltaY, VirtualViewport viewport) => Scroll(deltaY, new UiPoint(ContentPaddingX + SectionRailWidth + ListGap, ContentTop), viewport);

    public void Scroll(float deltaY, UiPoint point, VirtualViewport viewport)
    {
        if (IsSectionScrollPoint(point))
            sectionScrollOffset = Math.Clamp(sectionScrollOffset + deltaY, 0f, MaxSectionScrollOffset(viewport));
        else
            contentScrollOffset = Math.Clamp(contentScrollOffset + deltaY, 0f, MaxActiveContentScrollOffset(viewport));
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
            pendingSfxKey = updated ? "check-on" : "check-off";
        }
    }

    private void HandleRowAction(int rowIndex, VirtualViewport viewport)
    {
        var rows = ActiveSectionData.Categories.SelectMany(category => category.Rows).ToArray();
        if ((uint)rowIndex >= (uint)rows.Length)
            return;

        var row = rows[rowIndex];
        if (!row.IsEnabled)
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
        intValues[row.Key] = next;
        settingsStore?.SetInt(row.Key, next);
    }

    private void CycleSelect(SettingsRow row)
    {
        var current = settingsStore?.GetInt(row.Key, 0) ?? 0;
        var next = (current + 1) % 3;
        settingsStore?.SetInt(row.Key, next);
    }

    private void FocusInput(SettingsRow row, int rowIndex, VirtualViewport viewport)
    {
        var rowBounds = FindRowBounds(rowIndex, viewport);
        textInputService.RequestTextInput(new TextInputRequest(
            GetStringValue(row.Key),
            text =>
            {
                stringValues[row.Key] = text;
                settingsStore?.SetString(row.Key, text);
            },
            text =>
            {
                stringValues[row.Key] = text;
                settingsStore?.SetString(row.Key, text);
            },
            rowBounds));
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

}
