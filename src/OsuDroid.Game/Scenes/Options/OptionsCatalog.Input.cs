using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;

namespace OsuDroid.Game.Scenes.Options;

internal static partial class OptionsCatalog
{
    private static SettingsSection CreateInputSection() =>
        new(
            OptionsSection.Input,
            "Options_Input",
            UiMaterialIcon.GestureTapButton,
            UiAction.OptionsSectionInput,
            [
                new(
                    "Options_CategoryGameplay",
                    [
                        new(
                            "block_areas",
                            "Options_BlockAreasTitle",
                            "Options_BlockAreasSummary",
                            SettingsRowKind.Button,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "highPrecisionInput",
                            "Options_HighPrecisionInputTitle",
                            "Options_HighPrecisionInputSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "back_button_press_time",
                            "Options_BackButtonPressTimeTitle",
                            "Options_BackButtonPressTimeSummary",
                            SettingsRowKind.Slider,
                            Min: 0,
                            Max: 300,
                            DefaultValue: 300,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "removeSliderLock",
                            "Options_RemoveSliderLockTitle",
                            "Options_RemoveSliderLockSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            IsBottom: true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
                new(
                    "Options_CategoryVibration",
                    [
                        new(
                            "vibrationCircle",
                            "Options_VibrationCircleTitle",
                            "Options_EmptySummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "vibrationSlider",
                            "Options_VibrationSliderTitle",
                            "Options_EmptySummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "vibrationSpinner",
                            "Options_VibrationSpinnerTitle",
                            "Options_EmptySummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "seekBarVibrateIntensity",
                            "Options_SeekBarVibrateIntensityTitle",
                            "Options_SeekBarVibrateIntensitySummary",
                            SettingsRowKind.Slider,
                            Min: 1,
                            Max: 255,
                            DefaultValue: 127,
                            IsBottom: true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
                new(
                    "Options_CategorySynchronization",
                    [
                        new(
                            "fixFrameOffset",
                            "Options_FixFrameOffsetTitle",
                            "Options_FixFrameOffsetSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
            ]
        );
}
