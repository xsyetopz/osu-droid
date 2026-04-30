using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;

namespace OsuDroid.Game.Scenes.Options;

internal static partial class OptionsCatalog
{
    private static SettingsSection CreateGraphicsSection() =>
        new(
            OptionsSection.Graphics,
            "Options_Graphics",
            UiMaterialIcon.MonitorDashboard,
            UiAction.OptionsSectionGraphics,
            [
                new(
                    "Options_CategorySkin",
                    [
                        new(
                            "skinPath",
                            "OsuDroidLanguagePack_opt_skinpath_title",
                            "OsuDroidLanguagePack_opt_skinpath_summary",
                            SettingsRowKind.Select,
                            ValueKeys: ["OsuDroidLanguagePack_placeholder_array"],
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "hud_editor",
                            "Options_HudEditorTitle",
                            "Options_HudEditorSummary",
                            SettingsRowKind.Button,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "spinnerstyle",
                            "OsuDroidLanguagePack_opt_spinner_style_title",
                            "OsuDroidLanguagePack_opt_spinner_style_summary",
                            SettingsRowKind.Select,
                            DefaultValue: 0,
                            ValueKeys:
                            [
                                "Options_SpinnerStyleModern",
                                "Options_SpinnerStyleClassical",
                            ],
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "skin",
                            "Options_SkinTitle",
                            "Options_SkinSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
                new(
                    "Options_CategoryCursor",
                    [
                        new(
                            "showcursor",
                            "Options_ShowCursorTitle",
                            "Options_ShowCursorSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "cursorSize",
                            "OsuDroidLanguagePack_opt_cursor_size",
                            "OsuDroidLanguagePack_opt_cursor_size_summary",
                            SettingsRowKind.Slider,
                            Min: 25,
                            Max: 300,
                            DefaultValue: 50,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "particles",
                            "Options_ParticlesTitle",
                            "Options_ParticlesSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
                new(
                    "Options_CategoryAnimations",
                    [
                        new(
                            "dimHitObjects",
                            "Options_DimHitObjectsTitle",
                            "Options_DimHitObjectsSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "comboburst",
                            "Options_ComboBurstTitle",
                            "Options_ComboBurstSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "images",
                            "Options_LargeImagesTitle",
                            "Options_LargeImagesSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "animateFollowCircle",
                            "Options_AnimateFollowCircleTitle",
                            "Options_AnimateFollowCircleSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "animateComboText",
                            "Options_AnimateComboTextTitle",
                            "Options_AnimateComboTextSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "snakingInSliders",
                            "Options_SnakingInSlidersTitle",
                            "Options_SnakingInSlidersSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "snakingOutSliders",
                            "Options_SnakingOutSlidersTitle",
                            "Options_SnakingOutSlidersSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "noChangeDimInBreaks",
                            "Options_NoChangeDimInBreaksTitle",
                            "Options_NoChangeDimInBreaksSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "bursts",
                            "Options_BurstsTitle",
                            "Options_BurstsSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "hitlighting",
                            "Options_HitLightingTitle",
                            "Options_HitLightingSummary",
                            SettingsRowKind.Checkbox,
                            true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
            ]
        );
}
