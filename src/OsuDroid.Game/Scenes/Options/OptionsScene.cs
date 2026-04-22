using System.Globalization;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

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

    public const float AndroidReferencePixelWidth = 2340f;
    public const float AndroidReferenceDensity = 3f;
    public const float AndroidDpScale = VirtualViewport.LegacyWidth / (AndroidReferencePixelWidth / AndroidReferenceDensity);
    public const float AppBarHeight = 56f * AndroidDpScale;
    public const float ContentPaddingX = 32f * AndroidDpScale;
    public const float ContentTop = AppBarHeight + 32f * AndroidDpScale;
    public const float SectionRailWidth = 200f * AndroidDpScale;
    public const float SectionHeight = 48f * AndroidDpScale;
    public const float SectionStep = SectionHeight;
    public const float SectionIconSize = 24f * AndroidDpScale;
    public const float SectionPadding = 12f * AndroidDpScale;
    public const float SectionDrawablePadding = 12f * AndroidDpScale;
    public const float ListGap = 32f * AndroidDpScale;
    public const float CategoryTopMargin = 12f * AndroidDpScale;
    public const float CategoryHeaderHeight = 48f * AndroidDpScale;
    public const float RowPadding = 18f * AndroidDpScale;
    public const float RowHeight = 64f * AndroidDpScale;
    public const float RowTitleSize = 14f * AndroidDpScale;
    public const float RowSummarySize = 12f * AndroidDpScale;
    public const float InputHeight = 34f * AndroidDpScale;
    public const float InputGap = 8f * AndroidDpScale;
    public const float AndroidRoundedRectRadius = 14f * AndroidDpScale;
    public const float AndroidSidebarRadius = 15f * AndroidDpScale;
    public const float SeekbarContainerMarginX = 18f * AndroidDpScale;
    public const float SeekbarTrackMarginX = 2f * AndroidDpScale;
    public const float SeekbarTopMargin = 16f * AndroidDpScale;
    public const float SeekbarTrackHeight = 6f * AndroidDpScale;
    public const float SeekbarThumbSize = 16f * AndroidDpScale;
    public const float ControlColumnWidth = 280f * AndroidDpScale;
    public const float ControlGap = 18f * AndroidDpScale;
    public const float InputRowHeight = RowPadding * 2f + RowTitleSize + 4f + 6f * AndroidDpScale + RowSummarySize + 4f + InputGap + InputHeight;
    public const float SliderRowHeight = RowPadding * 2f + RowTitleSize + 4f + 6f * AndroidDpScale + RowSummarySize + 4f + SeekbarTopMargin + SeekbarThumbSize;

    private readonly Dictionary<string, bool> boolValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> intValues = new(StringComparer.Ordinal);
    private readonly GameLocalizer localizer;
    private readonly IGameSettingsStore? settingsStore;
    private OptionsSection activeSection;
    private float contentScrollOffset;
    private float sectionScrollOffset;

    public OptionsScene(GameLocalizer localizer, IGameSettingsStore? settingsStore = null)
    {
        this.localizer = localizer;
        this.settingsStore = settingsStore;
        foreach (var row in sections.SelectMany(section => section.Categories).SelectMany(category => category.Rows))
        {
            if (row.Kind == SettingsRowKind.Checkbox)
                boolValues[row.Key] = settingsStore?.GetBool(row.Key, row.DefaultChecked) ?? row.DefaultChecked;
            else if (row.Kind == SettingsRowKind.Slider)
                intValues[row.Key] = settingsStore?.GetInt(row.Key, row.DefaultValue) ?? row.DefaultValue;
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
                break;

            case UiAction.OptionsSectionGameplay:
                SelectSection(OptionsSection.Gameplay, viewport);
                break;

            case UiAction.OptionsSectionGraphics:
                SelectSection(OptionsSection.Graphics, viewport);
                break;

            case UiAction.OptionsSectionAudio:
                SelectSection(OptionsSection.Audio, viewport);
                break;

            case UiAction.OptionsSectionLibrary:
                SelectSection(OptionsSection.Library, viewport);
                break;

            case UiAction.OptionsSectionInput:
                SelectSection(OptionsSection.Input, viewport);
                break;

            case UiAction.OptionsSectionAdvanced:
                SelectSection(OptionsSection.Advanced, viewport);
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
        }
    }

    public bool GetBoolValue(string key) => boolValues.TryGetValue(key, out var value) && value;

    public int GetIntValue(string key) => intValues.TryGetValue(key, out var value) ? value : 0;

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
        }
    }

    private void ClampScroll(VirtualViewport viewport)
    {
        contentScrollOffset = Math.Clamp(contentScrollOffset, 0f, MaxActiveContentScrollOffset(viewport));
        sectionScrollOffset = Math.Clamp(sectionScrollOffset, 0f, MaxSectionScrollOffset(viewport));
    }

    private float MaxActiveContentScrollOffset(VirtualViewport viewport) => Math.Max(0f, CalculateContentHeight(ActiveSectionData.Categories) - VisibleContentHeight(viewport));

}
