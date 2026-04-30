using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;

namespace OsuDroid.Game.Scenes.Options;

internal static partial class OptionsCatalog
{
    private static SettingsSection CreateLibrarySection() =>
        new(
            OptionsSection.Library,
            "Options_Library",
            UiMaterialIcon.LibraryMusic,
            UiAction.OptionsSectionLibrary,
            [
                new(
                    "Options_CategoryImport",
                    [
                        new(
                            "deleteosz",
                            "Options_DeleteOszTitle",
                            "Options_DeleteOszSummary",
                            SettingsRowKind.Checkbox,
                            true
                        ),
                        new(
                            "scandownload",
                            "Options_ScanDownloadTitle",
                            "Options_ScanDownloadSummary",
                            SettingsRowKind.Checkbox,
                            false
                        ),
                        new(
                            "deleteUnimportedBeatmaps",
                            "Options_DeleteUnimportedTitle",
                            "Options_DeleteUnimportedSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "deleteUnsupportedVideos",
                            "Options_DeleteUnsupportedVideosTitle",
                            "Options_DeleteUnsupportedVideosSummary",
                            SettingsRowKind.Checkbox,
                            false,
                            Availability: SettingsRowAvailability.Locked
                        ),
                        new(
                            "preferNoVideoDownloads",
                            "Options_PreferNoVideoDownloadsTitle",
                            "Options_PreferNoVideoDownloadsSummary",
                            SettingsRowKind.Checkbox,
                            false
                        ),
                        new(
                            "importReplay",
                            "Options_ImportReplayTitle",
                            "Options_ImportReplaySummary",
                            SettingsRowKind.Button,
                            Availability: SettingsRowAvailability.Locked
                        ),
                    ]
                ),
                new(
                    "Options_CategoryMetadata",
                    [
                        new(
                            "forceromanized",
                            "Options_ForceRomanizedTitle",
                            "Options_ForceRomanizedSummary",
                            SettingsRowKind.Checkbox,
                            false
                        ),
                    ]
                ),
                new(
                    "Options_CategoryStorage",
                    [
                        new(
                            "clear_beatmap_cache",
                            "Options_ClearBeatmapCacheTitle",
                            "Options_ClearBeatmapCacheSummary",
                            SettingsRowKind.Button
                        ),
                        new(
                            "clear_properties",
                            "Options_ClearPropertiesTitle",
                            "Options_ClearPropertiesSummary",
                            SettingsRowKind.Button
                        ),
                    ]
                ),
            ]
        );
}
