namespace OsuDroid.Game.Scenes.ModSelect;

internal static class ModSettingPresets
{
    internal static ModSettingDescriptor[] RateSettings(float defaultValue) =>
        [
            new(
                "rateMultiplier",
                "Track rate multiplier",
                ModSettingKind.Slider,
                defaultValue,
                0.5,
                2,
                0.05,
                Precision: 2
            ),
        ];

    internal static ModSettingDescriptor[] ApproachDifferentSettings() =>
        [
            new("scale", "Initial size", ModSettingKind.Slider, 3, 1.5, 10, 0.1, Precision: 1),
            new(
                "style",
                "Animation style",
                ModSettingKind.Choice,
                EnumValues:
                [
                    "Linear",
                    "Gravity",
                    "InOut1",
                    "InOut2",
                    "Accelerate1",
                    "Accelerate2",
                    "Accelerate3",
                    "Decelerate1",
                    "Decelerate2",
                    "Decelerate3",
                    "BounceIn",
                    "BounceOut",
                    "BounceInOut",
                ]
            ),
        ];

    internal static ModSettingDescriptor[] DifficultyAdjustSettings() =>
        [
            new(
                "cs",
                "Circle size",
                ModSettingKind.OptionalSlider,
                MinValue: 0,
                MaxValue: 15,
                Step: 0.1,
                IsNullable: true,
                Precision: 1
            ),
            new(
                "ar",
                "Approach rate",
                ModSettingKind.OptionalSlider,
                MinValue: 0,
                MaxValue: 12.5,
                Step: 0.1,
                IsNullable: true,
                Precision: 1
            ),
            new(
                "od",
                "Overall difficulty",
                ModSettingKind.OptionalSlider,
                MinValue: 0,
                MaxValue: 11,
                Step: 0.1,
                IsNullable: true,
                Precision: 1
            ),
            new(
                "hp",
                "Health drain",
                ModSettingKind.OptionalSlider,
                MinValue: 0,
                MaxValue: 11,
                Step: 0.1,
                IsNullable: true,
                Precision: 1
            ),
        ];

    internal static ModSettingDescriptor[] FlashlightSettings() =>
        [
            new(
                "areaFollowDelay",
                "Flashlight follow delay",
                ModSettingKind.Slider,
                0.12,
                0.12,
                1.2,
                0.12,
                Precision: 2
            ),
            new(
                "sizeMultiplier",
                "Flashlight size",
                ModSettingKind.Slider,
                1,
                0.5,
                2,
                0.1,
                Precision: 1
            ),
            new(
                "comboBasedSize",
                "Change size based on combo",
                ModSettingKind.Toggle,
                DefaultBoolean: true
            ),
        ];

    internal static ModSettingDescriptor[] HiddenSettings() =>
        [new("onlyFadeApproachCircles", "Only fade approach circles", ModSettingKind.Toggle)];

    internal static ModSettingDescriptor[] MirrorSettings() =>
        [
            new(
                "flippedAxes",
                "Flipped axes",
                ModSettingKind.Choice,
                EnumValues: ["Horizontal", "Vertical", "Both"]
            ),
        ];

    internal static ModSettingDescriptor[] MutedSettings() =>
        [
            new("inverseMuting", "Start muted", ModSettingKind.Toggle),
            new("enableMetronome", "Enable metronome", ModSettingKind.Toggle, DefaultBoolean: true),
            new(
                "muteComboCount",
                "Final volume at combo",
                ModSettingKind.WholeNumber,
                100,
                0,
                500,
                1
            ),
            new("affectsHitSounds", "Mute hit sounds", ModSettingKind.Toggle, DefaultBoolean: true),
        ];

    internal static ModSettingDescriptor[] RandomSettings() =>
        [
            new(
                "seed",
                "Seed",
                ModSettingKind.OptionalWholeNumber,
                MinValue: 0,
                MaxValue: int.MaxValue,
                Step: 1,
                IsNullable: true,
                UseManualInput: true
            ),
            new(
                "angleSharpness",
                "Angle sharpness",
                ModSettingKind.Slider,
                7,
                1,
                10,
                0.1,
                Precision: 1
            ),
        ];

    internal static ModSettingDescriptor[] WindDownSettings() =>
        [
            new(
                "initialRate",
                "Initial rate",
                ModSettingKind.Slider,
                1,
                0.55,
                2,
                0.05,
                Precision: 2
            ),
            new(
                "finalRate",
                "Final rate",
                ModSettingKind.Slider,
                0.75,
                0.5,
                1.95,
                0.05,
                Precision: 2
            ),
        ];

    internal static ModSettingDescriptor[] WindUpSettings() =>
        [
            new(
                "initialRate",
                "Initial rate",
                ModSettingKind.Slider,
                1,
                0.5,
                1.95,
                0.05,
                Precision: 2
            ),
            new("finalRate", "Final rate", ModSettingKind.Slider, 1.5, 0.55, 2, 0.05, Precision: 2),
        ];
}
