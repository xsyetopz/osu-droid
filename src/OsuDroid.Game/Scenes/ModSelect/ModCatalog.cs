namespace OsuDroid.Game.Scenes.ModSelect;

public sealed record ModCatalogEntry(
    string Acronym,
    string Name,
    string Description,
    string SectionKey,
    float ScoreMultiplier = 1f,
    bool HasCustomization = false);

public static class ModCatalog
{
    public static IReadOnlyList<ModCatalogEntry> Entries { get; } =
    [
        new("NF", "No Fail", "You can't fail, no matter what.", "LegacyLanguagePack_mod_section_difficulty_reduction", 0.5f),
        new("EZ", "Easy", "Larger circles, more forgiving HP drain, less accuracy required, and three lives!", "LegacyLanguagePack_mod_section_difficulty_reduction", 0.5f),
        new("RE", "Really Easy", "Everything just got easier...", "LegacyLanguagePack_mod_section_difficulty_reduction", 0.5f),
        new("HT", "Half Time", "Less zoom...", "LegacyLanguagePack_mod_section_difficulty_reduction"),

        new("HR", "Hard Rock", "Everything just got a bit harder...", "LegacyLanguagePack_mod_section_difficulty_increase", 1.06f),
        new("SD", "Sudden Death", "Miss and fail.", "LegacyLanguagePack_mod_section_difficulty_increase"),
        new("PF", "Perfect", "SS or quit.", "LegacyLanguagePack_mod_section_difficulty_increase"),
        new("HD", "Hidden", "Play with no approach circles and fading circles/sliders.", "LegacyLanguagePack_mod_section_difficulty_increase"),
        new("DT", "Double Time", "Zoooooooooom...", "LegacyLanguagePack_mod_section_difficulty_increase"),
        new("NC", "Nightcore", "Uguuuuuuuu...", "LegacyLanguagePack_mod_section_difficulty_increase"),
        new("FL", "Flashlight", "Restricted view area.", "LegacyLanguagePack_mod_section_difficulty_increase"),
        new("PR", "Precise", "Ultimate rhythm gamer timing.", "LegacyLanguagePack_mod_section_difficulty_increase", 1.06f),
        new("TC", "Traceable", "Put your faith in the approach circles...", "LegacyLanguagePack_mod_section_difficulty_increase", 1.06f),
        new("SC", "Small Circle", "Who put ants in my beatmaps?", "LegacyLanguagePack_mod_section_difficulty_increase", HasCustomization: true),

        new("RX", "Relax", "You don't need to tap. Give your tapping fingers a break from the heat of things.", "LegacyLanguagePack_mod_section_difficulty_automation", 0.001f),
        new("AP", "Autopilot", "Automatic cursor movement - just follow the rhythm.", "LegacyLanguagePack_mod_section_difficulty_automation", 0.001f),
        new("AT", "Autoplay", "Watch a perfect automated play through the song.", "LegacyLanguagePack_mod_section_difficulty_automation"),

        new("CS", "Custom Speed", "Play at any speed you want - slow or fast.", "LegacyLanguagePack_mod_section_difficulty_conversion", HasCustomization: true),
        new("DA", "Difficulty Adjust", "Override a beatmap's difficulty settings.", "LegacyLanguagePack_mod_section_difficulty_conversion", HasCustomization: true),
        new("MR", "Mirror", "Flip objects on the chosen axes.", "LegacyLanguagePack_mod_section_difficulty_conversion", HasCustomization: true),
        new("RD", "Random", "It never gets boring!", "LegacyLanguagePack_mod_section_difficulty_conversion"),
        new("V2", "Score V2", "A different scoring mode from what you have known.", "LegacyLanguagePack_mod_section_difficulty_conversion"),

        new("MU", "Muted", "Can you still feel the rhythm without music?", "LegacyLanguagePack_mod_section_fun"),
        new("FR", "Freeze Frame", "Burn the notes into your memory.", "LegacyLanguagePack_mod_section_fun", HasCustomization: true),
        new("WU", "Wind Up", "Can you keep up?", "LegacyLanguagePack_mod_section_fun"),
        new("WD", "Wind Down", "Sloooow doooown...", "LegacyLanguagePack_mod_section_fun"),
        new("AD", "Approach Different", "Never trust the approach circles...", "LegacyLanguagePack_mod_section_fun", HasCustomization: true),
        new("SY", "Synesthesia", "Colors hit objects based on the rhythm.", "LegacyLanguagePack_mod_section_fun", 0.8f),
    ];
}
