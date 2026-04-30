using OsuDroid.Game.UI.Assets;
using static OsuDroid.Game.Scenes.ModSelect.ModSettingPresets;

namespace OsuDroid.Game.Scenes.ModSelect;

public static class ModCatalog
{
    private const string Reduction = "OsuDroidLanguagePack_mod_section_difficulty_reduction";
    private const string Increase = "OsuDroidLanguagePack_mod_section_difficulty_increase";
    private const string Automation = "OsuDroidLanguagePack_mod_section_difficulty_automation";
    private const string Conversion = "OsuDroidLanguagePack_mod_section_difficulty_conversion";
    private const string Fun = "OsuDroidLanguagePack_mod_section_fun";

    public static IReadOnlyList<ModCatalogEntry> Entries { get; } =
    [
        new(
            "EZ",
            "Easy",
            "Larger circles, more forgiving HP drain, less accuracy required, and three lives!",
            Reduction,
            DroidAssets.ModEasy,
            0.5f,
            IsRanked: true,
            IncompatibleAcronyms: ["HR"]
        ),
        new(
            "HT",
            "Half Time",
            "Less zoom...",
            Reduction,
            DroidAssets.ModHalfTime,
            IsRanked: true,
            IncompatibleAcronyms: ["DT", "NC"]
        ),
        new(
            "NF",
            "No Fail",
            "You can't fail, no matter what.",
            Reduction,
            DroidAssets.ModNoFail,
            0.5f,
            IsRanked: true,
            IncompatibleAcronyms: ["PF", "SD", "AP", "RX"]
        ),
        new(
            "RE",
            "Really Easy",
            "Everything just got easier...",
            Reduction,
            DroidAssets.ModReallyEasy,
            0.5f
        ),
        new(
            "DT",
            "Double Time",
            "Zoooooooooom...",
            Increase,
            DroidAssets.ModDoubleTime,
            IsRanked: true,
            IncompatibleAcronyms: ["NC", "HT"]
        ),
        new(
            "FL",
            "Flashlight",
            "Restricted view area.",
            Increase,
            DroidAssets.ModFlashlight,
            1.12f,
            IsRanked: true,
            Settings: FlashlightSettings()
        ),
        new(
            "HR",
            "Hard Rock",
            "Everything just got a bit harder...",
            Increase,
            DroidAssets.ModHardRock,
            1.06f,
            IsRanked: true,
            IncompatibleAcronyms: ["EZ"]
        ),
        new(
            "HD",
            "Hidden",
            "Play with no approach circles and fading circles/sliders.",
            Increase,
            DroidAssets.ModHidden,
            1.06f,
            IsRanked: true,
            Settings: HiddenSettings(),
            IncompatibleAcronyms: ["AD", "TC", "FR"]
        ),
        new(
            "NC",
            "Nightcore",
            "Uguuuuuuuu...",
            Increase,
            DroidAssets.ModNightcore,
            IsRanked: true,
            IncompatibleAcronyms: ["DT", "HT"]
        ),
        new(
            "PF",
            "Perfect",
            "SS or quit.",
            Increase,
            DroidAssets.ModPerfect,
            IsRanked: true,
            IncompatibleAcronyms: ["SD", "NF", "AT", "AP", "RX"]
        ),
        new(
            "PR",
            "Precise",
            "Ultimate rhythm gamer timing.",
            Increase,
            DroidAssets.ModPrecise,
            1.06f,
            IsRanked: true
        ),
        new(
            "SD",
            "Sudden Death",
            "Miss and fail.",
            Increase,
            DroidAssets.ModSuddenDeath,
            IsRanked: true,
            IncompatibleAcronyms: ["PF", "NF", "AT", "AP", "RX"]
        ),
        new(
            "TC",
            "Traceable",
            "Put your faith in the approach circles...",
            Increase,
            DroidAssets.ModTraceable,
            1.06f,
            IsRanked: true
        ),
        new(
            "AT",
            "Autoplay",
            "Watch a perfect automated play through the song.",
            Automation,
            DroidAssets.ModAutoplay,
            IncompatibleAcronyms: ["RX", "AP", "PF", "SD"]
        ),
        new(
            "AP",
            "Autopilot",
            "Automatic cursor movement - just follow the rhythm.",
            Automation,
            DroidAssets.ModAutopilot,
            0.001f,
            IncompatibleAcronyms: ["AT", "RX", "PF", "SD", "NF"]
        ),
        new(
            "RX",
            "Relax",
            "You don't need to tap. Give your tapping fingers a break from the heat of things.",
            Automation,
            DroidAssets.ModRelax,
            0.001f,
            IncompatibleAcronyms: ["AT", "AP", "PF", "SD", "NF"]
        ),
        new(
            "CS",
            "Custom Speed",
            "Play at any speed you want - slow or fast.",
            Conversion,
            DroidAssets.ModCustomSpeed,
            IsRanked: true,
            Settings: RateSettings(1f)
        ),
        new(
            "DA",
            "Difficulty Adjust",
            "Override a beatmap's difficulty settings.",
            Conversion,
            DroidAssets.ModDifficultyAdjust,
            Settings: DifficultyAdjustSettings()
        ),
        new(
            "MR",
            "Mirror",
            "Flip objects on the chosen axes.",
            Conversion,
            DroidAssets.ModMirror,
            Settings: MirrorSettings(),
            IncompatibleAcronyms: ["HR"]
        ),
        new(
            "RD",
            "Random",
            "It never gets boring!",
            Conversion,
            DroidAssets.ModRandom,
            Settings: RandomSettings()
        ),
        new(
            "V2",
            "Score V2",
            "A different scoring mode from what you have known.",
            Conversion,
            DroidAssets.ModScoreV2
        ),
        new(
            "AD",
            "Approach Different",
            "Never trust the approach circles...",
            Fun,
            DroidAssets.ModApproachDifferent,
            Settings: ApproachDifferentSettings(),
            IncompatibleAcronyms: ["HD", "FR"]
        ),
        new(
            "FR",
            "Freeze Frame",
            "Burn the notes into your memory.",
            Fun,
            DroidAssets.ModFreezeFrame,
            IncompatibleAcronyms: ["AD", "HD"]
        ),
        new(
            "MU",
            "Muted",
            "Can you still feel the rhythm without music?",
            Fun,
            DroidAssets.ModMuted,
            Settings: MutedSettings()
        ),
        new(
            "SY",
            "Synesthesia",
            "Colors hit objects based on the rhythm.",
            Fun,
            DroidAssets.ModSynesthesia,
            0.8f
        ),
        new(
            "WD",
            "Wind Down",
            "Sloooow doooown...",
            Fun,
            DroidAssets.ModWindDown,
            Settings: WindDownSettings()
        ),
        new(
            "WU",
            "Wind Up",
            "Can you keep up?",
            Fun,
            DroidAssets.ModWindUp,
            Settings: WindUpSettings()
        ),
    ];
}
