namespace OsuDroid.Game.Scenes.ModSelect;

public sealed record ModCatalogEntry(
    string Acronym,
    string Name,
    string Description,
    string SectionKey,
    string AssetName,
    float ScoreMultiplier = 1f,
    bool HasCustomization = false,
    bool IsRanked = false,
    IReadOnlyList<string>? IncompatibleAcronyms = null);

public static class ModCatalog
{
    public static IReadOnlyList<ModCatalogEntry> Entries { get; } =
    [
        new("EZ", "Easy", "Larger circles, more forgiving HP drain, less accuracy required, and three lives!", "LegacyLanguagePack_mod_section_difficulty_reduction", DroidAssets.ModEasy, 0.5f, IsRanked: true, IncompatibleAcronyms: ["HR"]),
        new("HT", "Half Time", "Less zoom...", "LegacyLanguagePack_mod_section_difficulty_reduction", DroidAssets.ModHalfTime, 0.3f, IsRanked: true, IncompatibleAcronyms: ["DT", "NC"]),
        new("NF", "No Fail", "You can't fail, no matter what.", "LegacyLanguagePack_mod_section_difficulty_reduction", DroidAssets.ModNoFail, 0.5f, IsRanked: true, IncompatibleAcronyms: ["PF", "SD", "AP", "RX"]),
        new("RE", "Really Easy", "Everything just got easier...", "LegacyLanguagePack_mod_section_difficulty_reduction", DroidAssets.ModReallyEasy, 0.5f),

        new("DT", "Double Time", "Zoooooooooom...", "LegacyLanguagePack_mod_section_difficulty_increase", DroidAssets.ModDoubleTime, 1.12f, IsRanked: true, IncompatibleAcronyms: ["NC", "HT"]),
        new("FL", "Flashlight", "Restricted view area.", "LegacyLanguagePack_mod_section_difficulty_increase", DroidAssets.ModFlashlight, 1.12f, IsRanked: true),
        new("HR", "Hard Rock", "Everything just got a bit harder...", "LegacyLanguagePack_mod_section_difficulty_increase", DroidAssets.ModHardRock, 1.06f, IsRanked: true, IncompatibleAcronyms: ["EZ", "MR"]),
        new("HD", "Hidden", "Play with no approach circles and fading circles/sliders.", "LegacyLanguagePack_mod_section_difficulty_increase", DroidAssets.ModHidden, 1.06f, IsRanked: true, IncompatibleAcronyms: ["AD", "TC", "FR"]),
        new("NC", "Nightcore", "Uguuuuuuuu...", "LegacyLanguagePack_mod_section_difficulty_increase", DroidAssets.ModNightcore, 1.12f, IsRanked: true, IncompatibleAcronyms: ["DT", "HT"]),
        new("PF", "Perfect", "SS or quit.", "LegacyLanguagePack_mod_section_difficulty_increase", DroidAssets.ModPerfect, IsRanked: true, IncompatibleAcronyms: ["NF", "SD", "AT"]),
        new("PR", "Precise", "Ultimate rhythm gamer timing.", "LegacyLanguagePack_mod_section_difficulty_increase", DroidAssets.ModPrecise, 1.06f, IsRanked: true),
        new("SC", "Small Circle", "Who put ants in my beatmaps?", "LegacyLanguagePack_mod_section_difficulty_increase", DroidAssets.ModSmallCircle, HasCustomization: true),
        new("SD", "Sudden Death", "Miss and fail.", "LegacyLanguagePack_mod_section_difficulty_increase", DroidAssets.ModSuddenDeath, IsRanked: true, IncompatibleAcronyms: ["NF", "PF", "AT"]),
        new("TC", "Traceable", "Put your faith in the approach circles...", "LegacyLanguagePack_mod_section_difficulty_increase", DroidAssets.ModTraceable, 1.06f, IncompatibleAcronyms: ["HD"]),

        new("AT", "Autoplay", "Watch a perfect automated play through the song.", "LegacyLanguagePack_mod_section_difficulty_automation", DroidAssets.ModAutoplay, IncompatibleAcronyms: ["RX", "AP", "PF", "SD"]),
        new("AP", "Autopilot", "Automatic cursor movement - just follow the rhythm.", "LegacyLanguagePack_mod_section_difficulty_automation", DroidAssets.ModAutopilot, 0.001f, IncompatibleAcronyms: ["RX", "AT", "NF"]),
        new("RX", "Relax", "You don't need to tap. Give your tapping fingers a break from the heat of things.", "LegacyLanguagePack_mod_section_difficulty_automation", DroidAssets.ModRelax, 0.001f, IncompatibleAcronyms: ["AT", "NF", "AP"]),

        new("CS", "Custom Speed", "Play at any speed you want - slow or fast.", "LegacyLanguagePack_mod_section_difficulty_conversion", DroidAssets.ModCustomSpeed, HasCustomization: true, IsRanked: true),
        new("DA", "Difficulty Adjust", "Override a beatmap's difficulty settings.", "LegacyLanguagePack_mod_section_difficulty_conversion", DroidAssets.ModDifficultyAdjust, HasCustomization: true),
        new("MR", "Mirror", "Flip objects on the chosen axes.", "LegacyLanguagePack_mod_section_difficulty_conversion", DroidAssets.ModMirror, HasCustomization: true, IncompatibleAcronyms: ["HR"]),
        new("RD", "Random", "It never gets boring!", "LegacyLanguagePack_mod_section_difficulty_conversion", DroidAssets.ModRandom),
        new("V2", "Score V2", "A different scoring mode from what you have known.", "LegacyLanguagePack_mod_section_difficulty_conversion", DroidAssets.ModScoreV2),

        new("AD", "Approach Different", "Never trust the approach circles...", "LegacyLanguagePack_mod_section_fun", DroidAssets.ModApproachDifferent, HasCustomization: true, IncompatibleAcronyms: ["HD", "FR"]),
        new("FR", "Freeze Frame", "Burn the notes into your memory.", "LegacyLanguagePack_mod_section_fun", DroidAssets.ModFreezeFrame, HasCustomization: true, IncompatibleAcronyms: ["AD", "HD"]),
        new("MU", "Muted", "Can you still feel the rhythm without music?", "LegacyLanguagePack_mod_section_fun", DroidAssets.ModMuted),
        new("SY", "Synesthesia", "Colors hit objects based on the rhythm.", "LegacyLanguagePack_mod_section_fun", DroidAssets.ModSynesthesia, 0.8f),
        new("WD", "Wind Down", "Sloooow doooown...", "LegacyLanguagePack_mod_section_fun", DroidAssets.ModWindDown),
        new("WU", "Wind Up", "Can you keep up?", "LegacyLanguagePack_mod_section_fun", DroidAssets.ModWindUp),
    ];
}
