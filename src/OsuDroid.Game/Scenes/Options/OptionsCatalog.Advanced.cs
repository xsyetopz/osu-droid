using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;

namespace OsuDroid.Game.Scenes.Options;

internal static partial class OptionsCatalog
{
    private static SettingsSection CreateAdvancedSection() =>
        new(
            OptionsSection.Advanced,
            "Options_Advanced",
            UiMaterialIcon.Cogs,
            UiAction.OptionsSectionAdvanced,
            [
                new(
                    "Options_CategoryDirectories",
                    [
                        new(
                            "corePath",
                            "Options_CorePathTitle",
                            "Options_CorePathSummary",
                            SettingsRowKind.Input
                        ),
                        new(
                            "skinTopPath",
                            "Options_SkinTopPathTitle",
                            "Options_SkinTopPathSummary",
                            SettingsRowKind.Input
                        ),
                        new(
                            "directory",
                            "Options_DirectoryTitle",
                            "Options_DirectorySummary",
                            SettingsRowKind.Input,
                            IsBottom: true
                        ),
                    ]
                ),
                new(
                    "Options_CategoryMiscellaneous",
                    [
                        new(
                            "forceMaxRefreshRate",
                            "Options_ForceMaxRefreshRateTitle",
                            "Options_ForceMaxRefreshRateSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "safebeatmapbg",
                            "Options_SafeBeatmapBgTitle",
                            "Options_SafeBeatmapBgSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            IsBottom: true,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
            ]
        );
}
