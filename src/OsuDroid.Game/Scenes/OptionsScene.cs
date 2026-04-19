using System.Globalization;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public enum OptionsSection
{
    General,
    Gameplay,
    Graphics,
    Audio,
    Library,
    Input,
    Advanced,
}

public sealed class OptionsScene
{
    private enum SettingsRowKind
    {
        Checkbox,
        Select,
        Input,
        Button,
        Slider,
    }

    private sealed record SettingsRow(
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

    private sealed record SettingsCategory(string TitleKey, IReadOnlyList<SettingsRow> Rows);
    private sealed record SettingsSection(OptionsSection Section, string Key, UiMaterialIcon Icon, UiAction Action, IReadOnlyList<SettingsCategory> Categories);

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

    private static readonly SettingsCategory[] generalCategories =
    [
        new("Options_CategoryOnline", [
            new("stayOnline", "Options_ServerConnectionTitle", "Options_ServerConnectionSummary", SettingsRowKind.Checkbox, true, Action: UiAction.OptionsToggleServerConnection),
            new("loadAvatar", "Options_LoadAvatarTitle", "Options_LoadAvatarSummary", SettingsRowKind.Checkbox, true, Action: UiAction.OptionsToggleLoadAvatar),
            new("difficultyAlgorithm", "Options_DifficultyAlgorithmTitle", "Options_DifficultyAlgorithmSummary", SettingsRowKind.Select, ValueKey: "Options_DifficultyAlgorithmValue", IsBottom: true),
        ]),
        new("Options_CategoryAccount", [
            new("login", "Options_LoginTitle", "Options_LoginSummary", SettingsRowKind.Input),
            new("password", "Options_PasswordTitle", "Options_PasswordSummary", SettingsRowKind.Input),
            new("registerAcc", "Options_RegisterTitle", "Options_RegisterSummary", SettingsRowKind.Button, IsBottom: true),
        ]),
        new("Options_CategoryCommunity", [
            new("receiveAnnouncements", "Options_ReceiveAnnouncementsTitle", "Options_ReceiveAnnouncementsSummary", SettingsRowKind.Checkbox, true, Action: UiAction.OptionsToggleAnnouncements, IsBottom: true),
        ]),
        new("Options_CategoryUpdates", [
            new("update", "Options_UpdateTitle", "Options_UpdateSummary", SettingsRowKind.Button, IsBottom: true),
        ]),
        new("Options_CategoryConfigBackup", [
            new("backup", "Options_BackupTitle", "Options_BackupSummary", SettingsRowKind.Button),
            new("restore", "Options_RestoreTitle", "Options_RestoreSummary", SettingsRowKind.Button, IsBottom: true),
        ]),
        new("Options_CategoryLocalization", [
            new("language", "Options_LanguageTitle", "Options_LanguageSummary", SettingsRowKind.Select, ValueKey: "Options_LanguageValue", IsBottom: true),
        ]),
    ];

    private static readonly SettingsSection[] sections =
    [
        new(OptionsSection.General, "Options_General", UiMaterialIcon.ViewGridOutline, UiAction.OptionsSectionGeneral, generalCategories),
        new(OptionsSection.Gameplay, "Options_Gameplay", UiMaterialIcon.GamepadVariantOutline, UiAction.OptionsSectionGameplay, [
            new("Options_CategoryHitObjects", [
                new("showfirstapproachcircle", "Options_ShowFirstApproachCircleTitle", "Options_ShowFirstApproachCircleSummary", SettingsRowKind.Checkbox, true),
            ]),
            new("Options_CategoryBackground", [
                new("keepBackgroundAspectRatio", "Options_KeepBackgroundAspectRatioTitle", "Options_KeepBackgroundAspectRatioSummary", SettingsRowKind.Checkbox, true),
                new("enableStoryboard", "Options_EnableStoryboardTitle", "Options_EnableStoryboardSummary", SettingsRowKind.Checkbox, true),
                new("enableVideo", "Options_EnableVideoTitle", "Options_EnableVideoSummary", SettingsRowKind.Checkbox, true),
            ]),
            new("Options_CategoryPlayfield", [
                new("displayPlayfieldBorder", "Options_DisplayPlayfieldBorderTitle", "Options_DisplayPlayfieldBorderSummary", SettingsRowKind.Checkbox, false),
            ]),
            new("Options_CategoryHud", [
                new("hideInGameUI", "Options_HideInGameUiTitle", "Options_HideInGameUiSummary", SettingsRowKind.Checkbox, false),
                new("hideReplayMarquee", "Options_HideReplayMarqueeTitle", "Options_HideReplayMarqueeSummary", SettingsRowKind.Checkbox, false),
                new("fps", "Options_FpsTitle", "Options_FpsSummary", SettingsRowKind.Checkbox, false),
                new("displayScoreStatistics", "Options_DisplayScoreStatisticsTitle", "Options_DisplayScoreStatisticsSummary", SettingsRowKind.Checkbox, false),
            ]),
            new("Options_CategoryComboColors", [
                new("useCustomColors", "Options_ComboColorsTitle", "Options_ComboColorsSummary", SettingsRowKind.Checkbox, false),
            ]),
        ]),
        new(OptionsSection.Graphics, "Options_Graphics", UiMaterialIcon.MonitorDashboard, UiAction.OptionsSectionGraphics, [
            new("Options_CategorySkin", [
                new("hud_editor", "Options_HudEditorTitle", "Options_HudEditorSummary", SettingsRowKind.Button),
                new("skin", "Options_SkinTitle", "Options_SkinSummary", SettingsRowKind.Checkbox, true),
            ]),
            new("Options_CategoryCursor", [
                new("showcursor", "Options_ShowCursorTitle", "Options_ShowCursorSummary", SettingsRowKind.Checkbox, false),
                new("particles", "Options_ParticlesTitle", "Options_ParticlesSummary", SettingsRowKind.Checkbox, true),
            ]),
            new("Options_CategoryAnimations", [
                new("dimHitObjects", "Options_DimHitObjectsTitle", "Options_DimHitObjectsSummary", SettingsRowKind.Checkbox, false),
                new("comboburst", "Options_ComboBurstTitle", "Options_ComboBurstSummary", SettingsRowKind.Checkbox, true),
                new("images", "Options_LargeImagesTitle", "Options_LargeImagesSummary", SettingsRowKind.Checkbox, true),
                new("animateFollowCircle", "Options_AnimateFollowCircleTitle", "Options_AnimateFollowCircleSummary", SettingsRowKind.Checkbox, true),
                new("animateComboText", "Options_AnimateComboTextTitle", "Options_AnimateComboTextSummary", SettingsRowKind.Checkbox, true),
                new("snakingInSliders", "Options_SnakingInSlidersTitle", "Options_SnakingInSlidersSummary", SettingsRowKind.Checkbox, true),
                new("snakingOutSliders", "Options_SnakingOutSlidersTitle", "Options_SnakingOutSlidersSummary", SettingsRowKind.Checkbox, true),
                new("noChangeDimInBreaks", "Options_NoChangeDimInBreaksTitle", "Options_NoChangeDimInBreaksSummary", SettingsRowKind.Checkbox, true),
                new("bursts", "Options_BurstsTitle", "Options_BurstsSummary", SettingsRowKind.Checkbox, true),
                new("hitlighting", "Options_HitLightingTitle", "Options_HitLightingSummary", SettingsRowKind.Checkbox, true),
            ]),
        ]),
        new(OptionsSection.Audio, "Options_Audio", UiMaterialIcon.Headphones, UiAction.OptionsSectionAudio, [
            new("Options_CategoryVolume", [
                new("bgmvolume", "Options_BgmVolumeTitle", "Options_BgmVolumeSummary", SettingsRowKind.Slider, Min: 0, Max: 100, DefaultValue: 100),
                new("soundvolume", "Options_SoundVolumeTitle", "Options_SoundVolumeSummary", SettingsRowKind.Slider, Min: 0, Max: 100, DefaultValue: 100, IsBottom: true),
            ]),
            new("Options_CategoryOffset", [
                new("offset_calibration", "Options_OffsetCalibrationTitle", "Options_OffsetCalibrationSummary", SettingsRowKind.Button),
                new("gameAudioSynchronizationThreshold", "Options_GameAudioSynchronizationThresholdTitle", "Options_GameAudioSynchronizationThresholdSummary", SettingsRowKind.Slider, Min: 0, Max: 100, DefaultValue: 20, IsBottom: true),
            ]),
            new("Options_CategoryEffect", [
                new("metronomeswitch", "Options_MetronomeSwitchTitle", "Options_MetronomeSwitchSummary", SettingsRowKind.Select, ValueKey: "Options_MetronomeSwitchValue"),
                new("shiftPitchInRateChange", "Options_ShiftPitchTitle", "Options_ShiftPitchSummary", SettingsRowKind.Checkbox, false, Action: UiAction.OptionsToggleShiftPitch, IsBottom: true),
            ]),
            new("Options_CategoryMiscellaneous", [
                new("beatmapSounds", "Options_BeatmapSoundsTitle", "Options_BeatmapSoundsSummary", SettingsRowKind.Checkbox, true, Action: UiAction.OptionsToggleBeatmapSounds),
                new("musicpreview", "Options_MusicPreviewTitle", "Options_MusicPreviewSummary", SettingsRowKind.Checkbox, true, Action: UiAction.OptionsToggleMusicPreview, IsBottom: true),
            ]),
        ]),
        new(OptionsSection.Library, "Options_Library", UiMaterialIcon.LibraryMusic, UiAction.OptionsSectionLibrary, [
            new("Options_CategoryImport", [
                new("deleteosz", "Options_DeleteOszTitle", "Options_DeleteOszSummary", SettingsRowKind.Checkbox, false),
                new("scandownload", "Options_ScanDownloadTitle", "Options_ScanDownloadSummary", SettingsRowKind.Checkbox, true),
                new("deleteUnimportedBeatmaps", "Options_DeleteUnimportedTitle", "Options_DeleteUnimportedSummary", SettingsRowKind.Checkbox, false),
                new("deleteUnsupportedVideos", "Options_DeleteUnsupportedVideosTitle", "Options_DeleteUnsupportedVideosSummary", SettingsRowKind.Checkbox, false),
                new("preferNoVideoDownloads", "Options_PreferNoVideoDownloadsTitle", "Options_PreferNoVideoDownloadsSummary", SettingsRowKind.Checkbox, false),
                new("importReplay", "Options_ImportReplayTitle", "Options_ImportReplaySummary", SettingsRowKind.Button),
            ]),
            new("Options_CategoryMetadata", [
                new("forceromanized", "Options_ForceRomanizedTitle", "Options_ForceRomanizedSummary", SettingsRowKind.Checkbox, false),
            ]),
            new("Options_CategoryStorage", [
                new("clear_beatmap_cache", "Options_ClearBeatmapCacheTitle", "Options_ClearBeatmapCacheSummary", SettingsRowKind.Button),
                new("clear_properties", "Options_ClearPropertiesTitle", "Options_ClearPropertiesSummary", SettingsRowKind.Button),
            ]),
        ]),
        new(OptionsSection.Input, "Options_Input", UiMaterialIcon.GestureTapButton, UiAction.OptionsSectionInput, [
            new("Options_CategoryGameplay", [
                new("block_areas", "Options_BlockAreasTitle", "Options_BlockAreasSummary", SettingsRowKind.Button),
                new("highPrecisionInput", "Options_HighPrecisionInputTitle", "Options_HighPrecisionInputSummary", SettingsRowKind.Checkbox, false),
                new("removeSliderLock", "Options_RemoveSliderLockTitle", "Options_RemoveSliderLockSummary", SettingsRowKind.Checkbox, false),
            ]),
            new("Options_CategoryVibration", [
                new("vibrationCircle", "Options_VibrationCircleTitle", "Options_VibrationSummary", SettingsRowKind.Checkbox, false),
                new("vibrationSlider", "Options_VibrationSliderTitle", "Options_VibrationSummary", SettingsRowKind.Checkbox, false),
                new("vibrationSpinner", "Options_VibrationSpinnerTitle", "Options_VibrationSummary", SettingsRowKind.Checkbox, false),
            ]),
            new("Options_CategorySynchronization", [
                new("fixFrameOffset", "Options_FixFrameOffsetTitle", "Options_FixFrameOffsetSummary", SettingsRowKind.Checkbox, false),
            ]),
        ]),
        new(OptionsSection.Advanced, "Options_Advanced", UiMaterialIcon.Cogs, UiAction.OptionsSectionAdvanced, [
            new("Options_CategoryDirectories", [
                new("corePath", "Options_CorePathTitle", "Options_CorePathSummary", SettingsRowKind.Input),
                new("skinTopPath", "Options_SkinTopPathTitle", "Options_SkinTopPathSummary", SettingsRowKind.Input),
                new("directory", "Options_DirectoryTitle", "Options_DirectorySummary", SettingsRowKind.Input, ValueKey: "Options_DirectoryValue", IsBottom: true),
            ]),
            new("Options_CategoryMiscellaneous", [
                new("forceMaxRefreshRate", "Options_ForceMaxRefreshRateTitle", "Options_ForceMaxRefreshRateSummary", SettingsRowKind.Checkbox, false),
                new("safebeatmapbg", "Options_SafeBeatmapBgTitle", "Options_SafeBeatmapBgSummary", SettingsRowKind.Checkbox, true, IsBottom: true),
            ]),
        ]),
    ];

    private readonly Dictionary<string, bool> boolValues = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> intValues = new(StringComparer.Ordinal);
    private readonly GameLocalizer localizer;
    private OptionsSection activeSection;
    private float contentScrollOffset;
    private float sectionScrollOffset;

    public OptionsScene(GameLocalizer localizer)
    {
        this.localizer = localizer;
        foreach (var row in sections.SelectMany(section => section.Categories).SelectMany(category => category.Rows))
        {
            if (row.Kind == SettingsRowKind.Checkbox)
                boolValues[row.Key] = row.DefaultChecked;
            else if (row.Kind == SettingsRowKind.Slider)
                intValues[row.Key] = row.DefaultValue;
        }
    }

    public OptionsSection ActiveSection => activeSection;

    public float ScrollOffset => contentScrollOffset;

    public float ContentScrollOffset => contentScrollOffset;

    public float SectionScrollOffset => sectionScrollOffset;

    public IReadOnlyList<string> Sections => sections.Select(section => localizer.Get(section.Key)).ToArray();

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
        return new GameFrameSnapshot(
            "Options",
            localizer["Options_Title"],
            localizer["Options_Subtitle"],
            Sections,
            (int)activeSection,
            false,
            CreateUiFrame(viewport));
    }

    private SettingsSection ActiveSectionData => sections.Single(section => section.Section == activeSection);

    private void Toggle(string key)
    {
        if (boolValues.TryGetValue(key, out var value))
            boolValues[key] = !value;
    }

    private void ClampScroll(VirtualViewport viewport)
    {
        contentScrollOffset = Math.Clamp(contentScrollOffset, 0f, MaxActiveContentScrollOffset(viewport));
        sectionScrollOffset = Math.Clamp(sectionScrollOffset, 0f, MaxSectionScrollOffset(viewport));
    }

    private float MaxActiveContentScrollOffset(VirtualViewport viewport) => Math.Max(0f, CalculateContentHeight(ActiveSectionData.Categories) - VisibleContentHeight(viewport));

    private UiFrameSnapshot CreateUiFrame(VirtualViewport viewport)
    {
        var elements = new List<UiElementSnapshot>
        {
            Fill("options-root", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), rootBackground),
        };

        AddActiveList(elements, viewport);
        AddSections(elements, viewport);
        AddAppBar(elements, viewport);

        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private static void AddAppBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        elements.Add(Fill("options-appbar", new UiRect(0f, 0f, viewport.VirtualWidth, AppBarHeight), appBarBackground));
        elements.Add(Fill("options-back-hit", new UiRect(0f, 0f, AppBarHeight, AppBarHeight), selectedSection, 1f, UiAction.OptionsBack));
        elements.Add(MaterialIcon("options-back", UiMaterialIcon.ArrowBack, new UiRect(16f * AndroidDpScale, 16f * AndroidDpScale, SectionIconSize, SectionIconSize), white, 1f, UiAction.OptionsBack));
    }

    private void AddSections(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        for (var i = 0; i < sections.Length; i++)
        {
            var section = sections[i];
            var isSelected = section.Section == activeSection;
            var y = ContentTop + i * SectionStep - sectionScrollOffset;
            var bounds = new UiRect(ContentPaddingX, y, SectionRailWidth, SectionHeight);
            if (!IsVisible(bounds, viewport))
                continue;

            if (isSelected)
                elements.Add(Fill("options-section-selected", bounds, selectedSection, 1f, section.Action, AndroidSidebarRadius));
            else
                elements.Add(Fill($"options-section-{i}-hit", bounds, UiColor.Opaque(0, 0, 0), 0f, section.Action));

            var textColor = isSelected ? white : disabledWhite;
            var iconBounds = new UiRect(bounds.X + SectionPadding, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize);
            var textX = iconBounds.Right + SectionDrawablePadding;
            elements.Add(MaterialIcon($"options-section-{i}-icon", section.Icon, iconBounds, textColor, isSelected ? 1f : 0.9f, section.Action));
            elements.Add(Text($"options-section-{i}-text", localizer[section.Key], textX, bounds.Y + (bounds.Height - RowTitleSize) / 2f, bounds.Right - textX - SectionPadding, RowTitleSize + 4f, RowTitleSize, textColor, isSelected ? 1f : 0.9f, true, section.Action));
        }
    }

    private void AddActiveList(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var listX = ContentPaddingX + SectionRailWidth + ListGap;
        var listWidth = viewport.VirtualWidth - listX - ContentPaddingX;
        var y = ContentTop - contentScrollOffset;
        var rowIndex = 0;

        foreach (var category in ActiveSectionData.Categories)
        {
            y += CategoryTopMargin;
            var categoryTop = y;
            var categoryHeight = CalculateCategoryHeight(category);
            var categoryBounds = new UiRect(listX, categoryTop, listWidth, categoryHeight);

            if (IsVisible(categoryBounds, viewport))
            {
                var categoryHeaderBounds = new UiRect(listX, categoryTop, listWidth, CategoryHeaderHeight);
                elements.Add(Fill($"options-category-{rowIndex}-header", categoryHeaderBounds, rowBackground, 1f, UiAction.None, AndroidRoundedRectRadius, true, UiCornerMode.Top));
                elements.Add(Text($"options-category-{rowIndex}-title", localizer[category.TitleKey], listX + RowPadding, categoryTop + (CategoryHeaderHeight - RowSummarySize) / 2f, listWidth - RowPadding * 2f, RowSummarySize + 4f, RowSummarySize, secondaryText, 0.95f, true));
            }

            var rowY = categoryTop + CategoryHeaderHeight;
            foreach (var row in category.Rows)
            {
                var rowHeight = GetRowHeight(row);
                var rowBounds = new UiRect(listX, rowY, listWidth, rowHeight);
                if (IsVisible(rowBounds, viewport))
                    AddRow(elements, row, rowIndex, rowBounds);

                rowY += rowHeight;
                rowIndex++;
            }

            y += categoryHeight;
        }
    }

    private void AddRow(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var rowAlpha = row.IsEnabled ? 1f : 0.5f;
        var rowAction = row.IsEnabled ? row.Action : UiAction.None;
        var rowCornerRadius = row.IsBottom ? AndroidRoundedRectRadius : 0f;
        elements.Add(Fill($"options-row-{index}", bounds, rowBackground, rowAlpha, rowAction, rowCornerRadius, row.IsEnabled, row.IsBottom ? UiCornerMode.Bottom : UiCornerMode.None));

        if (row.Kind == SettingsRowKind.Slider)
        {
            AddSliderControl(elements, row, index, bounds);
            return;
        }

        var titleHeight = RowTitleSize + 4f;
        var summaryHeight = RowSummarySize + 4f;
        var textBlockHeight = titleHeight + summaryHeight + 6f * AndroidDpScale;
        var textTop = row.Kind is SettingsRowKind.Input or SettingsRowKind.Slider
            ? bounds.Y + RowPadding
            : bounds.Y + (bounds.Height - textBlockHeight) / 2f;
        var reservedControlWidth = row.Kind switch
        {
            SettingsRowKind.Slider => 0f,
            SettingsRowKind.Select => 150f * AndroidDpScale,
            _ => 96f * AndroidDpScale,
        };
        var textWidth = row.Kind == SettingsRowKind.Input
            ? bounds.Width - RowPadding * 2f
            : Math.Max(80f * AndroidDpScale, bounds.Width - RowPadding * 3f - reservedControlWidth);
        var textColor = row.IsEnabled ? disabledWhite : secondaryText;
        elements.Add(Text($"options-row-{index}-label", localizer[row.TitleKey], bounds.X + RowPadding, textTop, textWidth, titleHeight, RowTitleSize, textColor, rowAlpha * 0.94f, true, rowAction, row.IsEnabled));
        if (!string.IsNullOrEmpty(localizer[row.SummaryKey]))
            elements.Add(Text($"options-row-{index}-summary", localizer[row.SummaryKey], bounds.X + RowPadding, textTop + titleHeight + 6f * AndroidDpScale, textWidth, summaryHeight, RowSummarySize, secondaryText, rowAlpha * 0.86f, false, rowAction, row.IsEnabled));

        switch (row.Kind)
        {
            case SettingsRowKind.Checkbox:
                AddCheckbox(elements, row, index, bounds);
                break;

            case SettingsRowKind.Select:
                AddSelectControl(elements, row, index, bounds);
                break;

            case SettingsRowKind.Input:
                AddInputControl(elements, row, index, bounds);
                break;

            case SettingsRowKind.Button:
                elements.Add(MaterialIcon($"options-row-{index}-chevron", UiMaterialIcon.ChevronRight, new UiRect(bounds.Right - RowPadding - SectionIconSize, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize), secondaryText, rowAlpha * 0.8f, rowAction, row.IsEnabled));
                break;

        }
    }

    private void AddCheckbox(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var checkbox = new UiRect(bounds.Right - RowPadding - SectionIconSize, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize);
        var isChecked = GetBoolValue(row.Key);
        var rowAction = row.IsEnabled ? row.Action : UiAction.None;
        var alpha = row.IsEnabled ? 1f : 0.55f;
        if (isChecked)
        {
            elements.Add(Fill($"options-row-{index}-checkbox-box", checkbox, checkboxAccent, alpha, rowAction, 2f * AndroidDpScale, row.IsEnabled));
            elements.Add(MaterialIcon($"options-row-{index}-checkbox", UiMaterialIcon.Check, checkbox, UiColor.Opaque(32, 32, 46), alpha, rowAction, row.IsEnabled));
        }
        else
        {
            elements.Add(MaterialIcon($"options-row-{index}-checkbox", UiMaterialIcon.CheckboxBlankOutline, checkbox, secondaryText, alpha, rowAction, row.IsEnabled));
        }
    }

    private void AddSelectControl(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var chevron = new UiRect(bounds.Right - RowPadding - SectionIconSize, bounds.Y + (bounds.Height - SectionIconSize) / 2f, SectionIconSize, SectionIconSize);
        var alpha = row.IsEnabled ? 0.9f : 0.45f;
        var rowAction = row.IsEnabled ? row.Action : UiAction.None;
        if (row.ValueKey is not null)
        {
            var valueWidth = 86f * AndroidDpScale;
            elements.Add(Text($"options-row-{index}-value", localizer[row.ValueKey], chevron.X - 12f * AndroidDpScale - valueWidth, bounds.Y + (bounds.Height - RowTitleSize - 4f) / 2f, valueWidth, RowTitleSize + 4f, RowTitleSize, secondaryText, alpha, false, rowAction, row.IsEnabled));
        }

        elements.Add(MaterialIcon($"options-row-{index}-dropdown", UiMaterialIcon.ArrowDropDown, chevron, secondaryText, alpha, rowAction, row.IsEnabled));
    }

    private void AddInputControl(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var rowAction = row.IsEnabled ? row.Action : UiAction.None;
        var alpha = row.IsEnabled ? 1f : 0.5f;
        var inputBounds = new UiRect(bounds.X + RowPadding, bounds.Y + RowPadding + RowTitleSize + 4f + 6f * AndroidDpScale + RowSummarySize + 4f + InputGap, bounds.Width - RowPadding * 2f, InputHeight);
        elements.Add(Fill($"options-row-{index}-input", inputBounds, inputBackground, alpha, rowAction, AndroidRoundedRectRadius, row.IsEnabled));
        if (row.ValueKey is not null)
            elements.Add(Text($"options-row-{index}-input-value", localizer[row.ValueKey], inputBounds.X + 14f * AndroidDpScale, inputBounds.Y + 8f * AndroidDpScale, inputBounds.Width - 28f * AndroidDpScale, RowTitleSize + 4f, RowTitleSize, UiColor.Opaque(235, 235, 245), 0.85f * alpha, false, rowAction, row.IsEnabled));
    }

    private void AddSliderControl(List<UiElementSnapshot> elements, SettingsRow row, int index, UiRect bounds)
    {
        var value = GetIntValue(row.Key);
        var normalized = row.Max == row.Min ? 0f : Math.Clamp((value - row.Min) / (float)(row.Max - row.Min), 0f, 1f);
        var alpha = row.IsEnabled ? 1f : 0.5f;
        var titleHeight = RowTitleSize + 4f;
        var summaryHeight = RowSummarySize + 4f;
        var containerX = bounds.X + SeekbarContainerMarginX;
        var containerWidth = bounds.Width - SeekbarContainerMarginX * 2f;
        var valueWidth = 72f * AndroidDpScale;
        var controlWidth = Math.Min(ControlColumnWidth, Math.Max(96f * AndroidDpScale, containerWidth * 0.44f));
        var textWidth = Math.Max(80f * AndroidDpScale, containerWidth - controlWidth - ControlGap);
        var textTop = bounds.Y + RowPadding;
        var summaryTop = textTop + titleHeight + 6f * AndroidDpScale;
        var valueTop = textTop;
        var trackWidth = controlWidth;
        var trackX = bounds.Right - SeekbarContainerMarginX - trackWidth;
        var trackY = summaryTop + summaryHeight + SeekbarTopMargin + (SeekbarThumbSize - SeekbarTrackHeight) / 2f;
        var thumbX = trackX + trackWidth * normalized - SeekbarThumbSize / 2f;
        var thumbY = trackY + SeekbarTrackHeight / 2f - SeekbarThumbSize / 2f;

        elements.Add(Text($"options-row-{index}-label", localizer[row.TitleKey], containerX, textTop, textWidth, titleHeight, RowTitleSize, disabledWhite, alpha * 0.94f, true, UiAction.None, row.IsEnabled));
        if (!string.IsNullOrEmpty(localizer[row.SummaryKey]))
            elements.Add(Text($"options-row-{index}-summary", localizer[row.SummaryKey], containerX, summaryTop, textWidth, summaryHeight, RowSummarySize, secondaryText, alpha * 0.86f, false, UiAction.None, row.IsEnabled));

        elements.Add(Text($"options-row-{index}-value", value.ToString(CultureInfo.InvariantCulture), trackX + trackWidth - valueWidth, valueTop, valueWidth, titleHeight, RowTitleSize, secondaryText, 0.9f * alpha, false, UiAction.None, row.IsEnabled));
        elements.Add(Fill($"options-row-{index}-slider-track", new UiRect(trackX, trackY, trackWidth, SeekbarTrackHeight), sliderTrack, alpha, UiAction.None, 12f * AndroidDpScale, row.IsEnabled));
        elements.Add(Fill($"options-row-{index}-slider-fill", new UiRect(trackX, trackY, trackWidth * normalized, SeekbarTrackHeight), checkboxAccent, alpha, UiAction.None, 12f * AndroidDpScale, row.IsEnabled));
        elements.Add(Fill($"options-row-{index}-slider-thumb", new UiRect(thumbX, thumbY, SeekbarThumbSize, SeekbarThumbSize), white, alpha, UiAction.None, 12f * AndroidDpScale, row.IsEnabled));
    }

    private static float CalculateContentHeight(IReadOnlyList<SettingsCategory> categories) => categories.Sum(category => CategoryTopMargin + CalculateCategoryHeight(category));

    private static float CalculateSectionHeight() => 32f * AndroidDpScale + sections.Length * SectionStep + 32f * AndroidDpScale;

    private static float CalculateCategoryHeight(SettingsCategory category) => CategoryHeaderHeight + category.Rows.Sum(GetRowHeight);

    private static float GetRowHeight(SettingsRow row) => row.Kind switch
    {
        SettingsRowKind.Input => InputRowHeight,
        SettingsRowKind.Slider => SliderRowHeight,
        _ => RowHeight,
    };

    private static float VisibleContentHeight(VirtualViewport viewport) => Math.Max(0f, viewport.VirtualHeight - ContentTop);

    private static bool IsVisible(UiRect bounds, VirtualViewport viewport) => bounds.Bottom >= AppBarHeight && bounds.Y <= viewport.VirtualHeight;

    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float cornerRadius = 0f, bool enabled = true, UiCornerMode cornerMode = UiCornerMode.All) => new(
        id,
        UiElementKind.Fill,
        bounds,
        color,
        alpha,
        null,
        action,
        null,
        null,
        enabled,
        null,
        cornerRadius,
        null,
        cornerMode);

    private static UiElementSnapshot Sprite(string id, string assetName, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, bool enabled = true) => new(
        id,
        UiElementKind.Sprite,
        bounds,
        color,
        alpha,
        assetName,
        action,
        null,
        null,
        enabled);


    private static UiElementSnapshot MaterialIcon(string id, UiMaterialIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, bool enabled = true) => new(
        id,
        UiElementKind.MaterialIcon,
        bounds,
        color,
        alpha,
        null,
        action,
        null,
        null,
        enabled,
        null,
        0f,
        icon);

    private static UiElementSnapshot Icon(string id, UiIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, bool enabled = true) => new(
        id,
        UiElementKind.Icon,
        bounds,
        color,
        alpha,
        null,
        action,
        null,
        null,
        enabled,
        icon);

    private static UiElementSnapshot Text(
        string id,
        string value,
        float x,
        float y,
        float width,
        float height,
        float size,
        UiColor color,
        float alpha = 1f,
        bool bold = false,
        UiAction action = UiAction.None,
        bool enabled = true) => new(
            id,
            UiElementKind.Text,
            new UiRect(x, y, width, height),
            color,
            alpha,
            null,
            action,
            value,
            new UiTextStyle(size, bold),
            enabled);
}
