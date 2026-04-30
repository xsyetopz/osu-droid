using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;

namespace OsuDroid.Game.Scenes.Options;

internal static partial class OptionsCatalog
{
    private static SettingsSection CreateGameplaySection() =>
        new(
            OptionsSection.Gameplay,
            "Options_Gameplay",
            UiMaterialIcon.GamepadVariantOutline,
            UiAction.OptionsSectionGameplay,
            [
                new(
                    "Options_CategoryHitObjects",
                    [
                        new(
                            "showfirstapproachcircle",
                            "Options_ShowFirstApproachCircleTitle",
                            "Options_ShowFirstApproachCircleSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
                new(
                    "Options_CategoryBackground",
                    [
                        new(
                            "bgbrightness",
                            "OsuDroidLanguagePack_opt_bgbrightness_title",
                            "OsuDroidLanguagePack_opt_bgbrightness_summary",
                            SettingsRowKind.Slider,
                            Min: 0,
                            Max: 100,
                            DefaultValue: 25,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "keepBackgroundAspectRatio",
                            "Options_KeepBackgroundAspectRatioTitle",
                            "Options_KeepBackgroundAspectRatioSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "enableStoryboard",
                            "Options_EnableStoryboardTitle",
                            "Options_EnableStoryboardSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "enableVideo",
                            "Options_EnableVideoTitle",
                            "Options_EnableVideoSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
                new(
                    "Options_CategoryPlayfield",
                    [
                        new(
                            "playfieldSize",
                            "OsuDroidLanguagePack_opt_setplayfield_title",
                            "OsuDroidLanguagePack_opt_setplayfield_summary",
                            SettingsRowKind.Slider,
                            Min: 50,
                            Max: 100,
                            DefaultValue: 100,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "playfieldHorizontalPosition",
                            "OsuDroidLanguagePack_opt_playfieldHorizontalPosition_title",
                            "OsuDroidLanguagePack_opt_playfieldHorizontalPosition_summary",
                            SettingsRowKind.Slider,
                            Min: 0,
                            Max: 100,
                            DefaultValue: 50,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "playfieldVerticalPosition",
                            "OsuDroidLanguagePack_opt_playfieldVerticalPosition_title",
                            "OsuDroidLanguagePack_opt_playfieldVerticalPosition_summary",
                            SettingsRowKind.Slider,
                            Min: 0,
                            Max: 100,
                            DefaultValue: 50,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "displayPlayfieldBorder",
                            "Options_DisplayPlayfieldBorderTitle",
                            "Options_DisplayPlayfieldBorderSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
                new(
                    "Options_CategoryHud",
                    [
                        new(
                            "hideInGameUI",
                            "Options_HideInGameUiTitle",
                            "Options_HideInGameUiSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "hideReplayMarquee",
                            "Options_HideReplayMarqueeTitle",
                            "Options_HideReplayMarqueeSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "fps",
                            "Options_FpsTitle",
                            "Options_FpsSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "displayScoreStatistics",
                            "Options_DisplayScoreStatisticsTitle",
                            "Options_DisplayScoreStatisticsSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
                new(
                    "Options_CategoryComboColors",
                    [
                        new(
                            "useCustomColors",
                            "Options_ComboColorsTitle",
                            "Options_ComboColorsSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "combo1",
                            "Options_Combo1Title",
                            "Options_EmptySummary",
                            SettingsRowKind.Button,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "combo2",
                            "Options_Combo2Title",
                            "Options_EmptySummary",
                            SettingsRowKind.Button,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "combo3",
                            "Options_Combo3Title",
                            "Options_EmptySummary",
                            SettingsRowKind.Button,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "combo4",
                            "Options_Combo4Title",
                            "Options_EmptySummary",
                            SettingsRowKind.Button,
                            IsBottom: true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
            ]
        );
}
