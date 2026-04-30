namespace OsuDroid.Game.Scenes.Options;

internal static partial class OptionsCatalog
{
    private static readonly SettingsCategory[] s_generalCategories = CreateGeneralCategories();

    private static readonly SettingsSection[] s_sections =
    [
        CreateGeneralSection(),
        CreateGameplaySection(),
        CreateGraphicsSection(),
        CreateAudioSection(),
        CreateLibrarySection(),
        CreateInputSection(),
        CreateAdvancedSection(),
    ];

    internal static SettingsCategory[] GeneralCategories => s_generalCategories;

    internal static SettingsSection[] Sections => s_sections;
}
