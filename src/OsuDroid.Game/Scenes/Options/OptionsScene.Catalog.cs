using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class OptionsScene
{
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

}
