using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;

namespace OsuDroid.Game.Scenes.Options;

internal static partial class OptionsCatalog
{
    private static SettingsSection CreateAudioSection() =>
        new(
            OptionsSection.Audio,
            "Options_Audio",
            UiMaterialIcon.Headphones,
            UiAction.OptionsSectionAudio,
            [
                new(
                    "Options_CategoryVolume",
                    [
                        new(
                            "bgmvolume",
                            "Options_BgmVolumeTitle",
                            "Options_BgmVolumeSummary",
                            SettingsRowKind.Slider,
                            Min: 0,
                            Max: 100,
                            DefaultValue: 100
                        ),
                        new(
                            "soundvolume",
                            "Options_SoundVolumeTitle",
                            "Options_SoundVolumeSummary",
                            SettingsRowKind.Slider,
                            Min: 0,
                            Max: 100,
                            DefaultValue: 100,
                            IsBottom: true
                        ),
                    ]
                ),
                new(
                    "Options_CategoryOffset",
                    [
                        new(
                            "offset_calibration",
                            "Options_OffsetCalibrationTitle",
                            "Options_OffsetCalibrationSummary",
                            SettingsRowKind.Button,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "gameAudioSynchronizationThreshold",
                            "Options_GameAudioSynchronizationThresholdTitle",
                            "Options_GameAudioSynchronizationThresholdSummary",
                            SettingsRowKind.Slider,
                            Min: 0,
                            Max: 100,
                            DefaultValue: 20,
                            IsBottom: true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
                new(
                    "Options_CategoryEffect",
                    [
                        new(
                            "metronomeswitch",
                            "Options_MetronomeSwitchTitle",
                            "Options_MetronomeSwitchSummary",
                            SettingsRowKind.Select,
                            ValueKey: "Options_MetronomeSwitchValue",
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "shiftPitchInRateChange",
                            "Options_ShiftPitchTitle",
                            "Options_ShiftPitchSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Action: UiAction.OptionsToggleShiftPitch,
                            IsBottom: true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
                new(
                    "Options_CategoryMiscellaneous",
                    [
                        new(
                            "beatmapSounds",
                            "Options_BeatmapSoundsTitle",
                            "Options_BeatmapSoundsSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Action: UiAction.OptionsToggleBeatmapSounds,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "musicpreview",
                            "Options_MusicPreviewTitle",
                            "Options_MusicPreviewSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Action: UiAction.OptionsToggleMusicPreview,
                            IsBottom: true
                        ),
                    ]
                ),
            ]
        );
}
