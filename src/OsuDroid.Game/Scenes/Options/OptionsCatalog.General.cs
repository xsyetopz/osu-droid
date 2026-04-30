using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;

namespace OsuDroid.Game.Scenes.Options;

internal static partial class OptionsCatalog
{
    private static SettingsCategory[] CreateGeneralCategories() =>
        [
            new(
                "Options_CategoryOnline",
                [
                    new(
                        "stayOnline",
                        "Options_ServerConnectionTitle",
                        "Options_ServerConnectionSummary",
                        SettingsRowKind.Checkbox,
                        false,
                        Action: UiAction.OptionsToggleServerConnection
                    ),
                    new(
                        "loadAvatar",
                        "Options_LoadAvatarTitle",
                        "Options_LoadAvatarSummary",
                        SettingsRowKind.Checkbox,
                        false,
                        Action: UiAction.OptionsToggleLoadAvatar
                    ),
                    new(
                        "difficultyAlgorithm",
                        "Options_DifficultyAlgorithmTitle",
                        "Options_DifficultyAlgorithmSummary",
                        SettingsRowKind.Select,
                        IsBottom: true,
                        ValueKeys:
                        [
                            "Options_DifficultyAlgorithmDroid",
                            "Options_DifficultyAlgorithmStandard",
                        ]
                    ),
                ]
            ),
            new(
                "Options_CategoryAccount",
                [
                    new(
                        "onlineUsername",
                        "Options_LoginTitle",
                        "Options_LoginSummary",
                        SettingsRowKind.Input
                    ),
                    new(
                        "onlinePassword",
                        "Options_PasswordTitle",
                        "Options_PasswordSummary",
                        SettingsRowKind.Input
                    ),
                    new(
                        "registerAcc",
                        "Options_RegisterTitle",
                        "Options_RegisterSummary",
                        SettingsRowKind.Button,
                        IsBottom: true
                    ),
                ]
            ),
            new(
                "Options_CategoryCommunity",
                [
                    new(
                        "receiveAnnouncements",
                        "Options_ReceiveAnnouncementsTitle",
                        "Options_ReceiveAnnouncementsSummary",
                        SettingsRowKind.Checkbox,
                        true,
                        Action: UiAction.OptionsToggleAnnouncements,
                        IsBottom: true
                    ),
                ]
            ),
            new(
                "Options_CategoryUpdates",
                [
                    new(
                        "update",
                        "Options_UpdateTitle",
                        "Options_UpdateSummary",
                        SettingsRowKind.Button,
                        IsBottom: true
                    ),
                ]
            ),
            new(
                "Options_CategoryConfigBackup",
                [
                    new(
                        "backup",
                        "Options_BackupTitle",
                        "Options_BackupSummary",
                        SettingsRowKind.Button
                    ),
                    new(
                        "restore",
                        "Options_RestoreTitle",
                        "Options_RestoreSummary",
                        SettingsRowKind.Button,
                        IsBottom: true
                    ),
                ]
            ),
            new(
                "Options_CategoryLocalization",
                [
                    new(
                        "appLanguage",
                        "Options_LanguageTitle",
                        "Options_LanguageSummary",
                        SettingsRowKind.Select,
                        IsBottom: true,
                        ValueKeys: ["Options_LanguageSystemDefault"]
                    ),
                ]
            ),
        ];

    private static SettingsSection CreateGeneralSection() =>
        new(
            OptionsSection.General,
            "Options_General",
            UiMaterialIcon.ViewGridOutline,
            UiAction.OptionsSectionGeneral,
            OptionsCatalog.GeneralCategories
        );
}
