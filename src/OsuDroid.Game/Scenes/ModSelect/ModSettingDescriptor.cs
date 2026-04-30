namespace OsuDroid.Game.Scenes.ModSelect;

public enum ModSettingKind
{
    Slider,
    OptionalSlider,
    WholeNumber,
    OptionalWholeNumber,
    Toggle,
    Choice,
}

public sealed record ModSettingDescriptor(
    string Key,
    string Name,
    ModSettingKind Kind,
    double DefaultValue = 0,
    double MinValue = 0,
    double MaxValue = 1,
    double Step = 1,
    IReadOnlyList<string>? EnumValues = null,
    bool DefaultBoolean = false,
    bool IsNullable = false,
    int Precision = 2,
    bool UseManualInput = false
);

public sealed record ModCatalogEntry(
    string Acronym,
    string Name,
    string Description,
    string SectionKey,
    string AssetName,
    float ScoreMultiplier = 1f,
    bool IsRanked = false,
    IReadOnlyList<string>? IncompatibleAcronyms = null,
    IReadOnlyList<ModSettingDescriptor>? Settings = null
)
{
    public bool HasCustomization => Settings?.Count > 0;
}
