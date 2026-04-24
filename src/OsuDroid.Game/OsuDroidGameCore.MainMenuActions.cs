using OsuDroid.Game.Runtime;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    private bool HandleMainMenuUiAction(UiAction action)
    {
        switch (action)
        {
            case UiAction.MainMenuCookie:
                TapMainMenuCookie();
                return true;
            case UiAction.MainMenuFirst:
            case UiAction.MainMenuSecond:
            case UiAction.MainMenuThird:
                TapMainMenu(UiActionRouter.ToMainMenuSlot(action));
                return true;
            case UiAction.MainMenuVersionPill:
                _mainMenu.OpenAboutDialog();
                return true;
            case UiAction.MainMenuAboutClose:
                _mainMenu.CloseAboutDialog();
                return true;
            case UiAction.MainMenuAboutChangelog:
                PendingExternalUrl = "https://osudroid.moe/changelog/latest";
                _mainMenu.CloseAboutDialog();
                return true;
            case UiAction.MainMenuAboutOsuWebsite:
                PendingExternalUrl = "https://osu.ppy.sh";
                return true;
            case UiAction.MainMenuAboutOsuDroidWebsite:
                PendingExternalUrl = "https://osudroid.moe";
                return true;
            case UiAction.MainMenuAboutDiscord:
                PendingExternalUrl = "https://discord.gg/nyD92cE";
                return true;
            case UiAction.MainMenuExitDialogPanel:
                return true;
            case UiAction.MainMenuExitConfirm:
                _mainMenu.ConfirmExitDialog();
                return true;
            case UiAction.MainMenuExitCancel:
                _mainMenu.CancelExitDialog();
                return true;
            case UiAction.MainMenuBeatmapDownloader:
                PreserveDownloaderMusic();
                _activeScene = ActiveScene.BeatmapDownloader;
                _beatmapDownloader.Enter();
                return true;
            case UiAction.MainMenuMusicPrevious:
                return ExecuteMusicCommand(MenuMusicCommand.Previous);
            case UiAction.MainMenuMusicPlay:
                return ExecuteMusicCommand(MenuMusicCommand.Play);
            case UiAction.MainMenuMusicPause:
                return ExecuteMusicCommand(MenuMusicCommand.Pause);
            case UiAction.MainMenuMusicStop:
                return ExecuteMusicCommand(MenuMusicCommand.Stop);
            case UiAction.MainMenuMusicNext:
                return ExecuteMusicCommand(MenuMusicCommand.Next);
            case UiAction.None:
                break;
            case UiAction.OptionsBack:
                break;
            case UiAction.OptionsSectionGeneral:
                break;
            case UiAction.OptionsSectionGameplay:
                break;
            case UiAction.OptionsSectionGraphics:
                break;
            case UiAction.OptionsSectionAudio:
                break;
            case UiAction.OptionsSectionLibrary:
                break;
            case UiAction.OptionsSectionInput:
                break;
            case UiAction.OptionsSectionAdvanced:
                break;
            case UiAction.OptionsToggleServerConnection:
                break;
            case UiAction.OptionsToggleLoadAvatar:
                break;
            case UiAction.OptionsToggleAnnouncements:
                break;
            case UiAction.OptionsToggleMusicPreview:
                break;
            case UiAction.OptionsToggleShiftPitch:
                break;
            case UiAction.OptionsToggleBeatmapSounds:
                break;
            case UiAction.OptionsRow0:
                break;
            case UiAction.OptionsRow1:
                break;
            case UiAction.OptionsRow2:
                break;
            case UiAction.OptionsRow3:
                break;
            case UiAction.OptionsRow4:
                break;
            case UiAction.OptionsRow5:
                break;
            case UiAction.OptionsRow6:
                break;
            case UiAction.OptionsRow7:
                break;
            case UiAction.OptionsRow8:
                break;
            case UiAction.OptionsRow9:
                break;
            case UiAction.OptionsRow10:
                break;
            case UiAction.OptionsRow11:
                break;
            case UiAction.OptionsRow12:
                break;
            case UiAction.OptionsRow13:
                break;
            case UiAction.OptionsRow14:
                break;
            case UiAction.OptionsRow15:
                break;
            case UiAction.OptionsRow16:
                break;
            case UiAction.OptionsRow17:
                break;
            case UiAction.OptionsRow18:
                break;
            case UiAction.OptionsRow19:
                break;
            case UiAction.OptionsRow20:
                break;
            case UiAction.OptionsRow21:
                break;
            case UiAction.OptionsRow22:
                break;
            case UiAction.OptionsRow23:
                break;
            case UiAction.OptionsRow24:
                break;
            case UiAction.OptionsRow25:
                break;
            case UiAction.OptionsRow26:
                break;
            case UiAction.OptionsRow27:
                break;
            case UiAction.OptionsRow28:
                break;
            case UiAction.OptionsRow29:
                break;
            case UiAction.OptionsRow30:
                break;
            case UiAction.OptionsRow31:
                break;
            case UiAction.OptionsRow32:
                break;
            case UiAction.OptionsRow33:
                break;
            case UiAction.OptionsRow34:
                break;
            case UiAction.OptionsRow35:
                break;
            case UiAction.OptionsRow36:
                break;
            case UiAction.OptionsRow37:
                break;
            case UiAction.OptionsRow38:
                break;
            case UiAction.OptionsRow39:
                break;
            case UiAction.OptionsRow40:
                break;
            case UiAction.OptionsRow41:
                break;
            case UiAction.OptionsRow42:
                break;
            case UiAction.OptionsRow43:
                break;
            case UiAction.OptionsRow44:
                break;
            case UiAction.OptionsRow45:
                break;
            case UiAction.OptionsRow46:
                break;
            case UiAction.OptionsRow47:
                break;
            case UiAction.OptionsRow48:
                break;
            case UiAction.OptionsRow49:
                break;
            case UiAction.OptionsRow50:
                break;
            case UiAction.OptionsRow51:
                break;
            case UiAction.OptionsRow52:
                break;
            case UiAction.OptionsRow53:
                break;
            case UiAction.OptionsRow54:
                break;
            case UiAction.OptionsRow55:
                break;
            case UiAction.OptionsRow56:
                break;
            case UiAction.OptionsRow57:
                break;
            case UiAction.OptionsRow58:
                break;
            case UiAction.OptionsRow59:
                break;
            case UiAction.OptionsRow60:
                break;
            case UiAction.OptionsRow61:
                break;
            case UiAction.OptionsRow62:
                break;
            case UiAction.OptionsRow63:
                break;
            case UiAction.DownloaderBack:
                break;
            case UiAction.DownloaderSearchBox:
                break;
            case UiAction.DownloaderSearchSubmit:
                break;
            case UiAction.DownloaderRefresh:
                break;
            case UiAction.DownloaderFilters:
                break;
            case UiAction.DownloaderMirror:
                break;
            case UiAction.DownloaderMirrorOsuDirect:
                break;
            case UiAction.DownloaderMirrorCatboy:
                break;
            case UiAction.DownloaderSort:
                break;
            case UiAction.DownloaderSortTitle:
                break;
            case UiAction.DownloaderSortArtist:
                break;
            case UiAction.DownloaderSortBpm:
                break;
            case UiAction.DownloaderSortDifficultyRating:
                break;
            case UiAction.DownloaderSortHitLength:
                break;
            case UiAction.DownloaderSortPassCount:
                break;
            case UiAction.DownloaderSortPlayCount:
                break;
            case UiAction.DownloaderSortTotalLength:
                break;
            case UiAction.DownloaderSortFavouriteCount:
                break;
            case UiAction.DownloaderSortLastUpdated:
                break;
            case UiAction.DownloaderSortRankedDate:
                break;
            case UiAction.DownloaderSortSubmittedDate:
                break;
            case UiAction.DownloaderOrder:
                break;
            case UiAction.DownloaderStatus:
                break;
            case UiAction.DownloaderStatusAll:
                break;
            case UiAction.DownloaderStatusRanked:
                break;
            case UiAction.DownloaderStatusApproved:
                break;
            case UiAction.DownloaderStatusQualified:
                break;
            case UiAction.DownloaderStatusLoved:
                break;
            case UiAction.DownloaderStatusPending:
                break;
            case UiAction.DownloaderStatusWorkInProgress:
                break;
            case UiAction.DownloaderStatusGraveyard:
                break;
            case UiAction.DownloaderCard0:
                break;
            case UiAction.DownloaderCard1:
                break;
            case UiAction.DownloaderCard2:
                break;
            case UiAction.DownloaderCard3:
                break;
            case UiAction.DownloaderCard4:
                break;
            case UiAction.DownloaderCard5:
                break;
            case UiAction.DownloaderCard6:
                break;
            case UiAction.DownloaderCard7:
                break;
            case UiAction.DownloaderDetailsClose:
                break;
            case UiAction.DownloaderDetailsPanel:
                break;
            case UiAction.DownloaderDetailsPreview:
                break;
            case UiAction.DownloaderDetailsDownload:
                break;
            case UiAction.DownloaderDetailsDownloadNoVideo:
                break;
            case UiAction.DownloaderDetailsDifficulty0:
                break;
            case UiAction.DownloaderDetailsDifficulty1:
                break;
            case UiAction.DownloaderDetailsDifficulty2:
                break;
            case UiAction.DownloaderDetailsDifficulty3:
                break;
            case UiAction.DownloaderDetailsDifficulty4:
                break;
            case UiAction.DownloaderDetailsDifficulty5:
                break;
            case UiAction.DownloaderDetailsDifficulty6:
                break;
            case UiAction.DownloaderDetailsDifficulty7:
                break;
            case UiAction.DownloaderDetailsDifficulty8:
                break;
            case UiAction.DownloaderDetailsDifficulty9:
                break;
            case UiAction.DownloaderDetailsDifficulty10:
                break;
            case UiAction.DownloaderDetailsDifficulty11:
                break;
            case UiAction.DownloaderDetailsDifficulty12:
                break;
            case UiAction.DownloaderDetailsDifficulty13:
                break;
            case UiAction.DownloaderDetailsDifficulty14:
                break;
            case UiAction.DownloaderDetailsDifficulty15:
                break;
            case UiAction.DownloaderDownloadCancel:
                break;
            case UiAction.DownloaderDownloadFirst:
                break;
            case UiAction.DownloaderDownloadFirstNoVideo:
                break;
            case UiAction.DownloaderDownload0:
                break;
            case UiAction.DownloaderDownload1:
                break;
            case UiAction.DownloaderDownload2:
                break;
            case UiAction.DownloaderDownload3:
                break;
            case UiAction.DownloaderDownload4:
                break;
            case UiAction.DownloaderDownload5:
                break;
            case UiAction.DownloaderDownload6:
                break;
            case UiAction.DownloaderDownload7:
                break;
            case UiAction.DownloaderDownloadNoVideo0:
                break;
            case UiAction.DownloaderDownloadNoVideo1:
                break;
            case UiAction.DownloaderDownloadNoVideo2:
                break;
            case UiAction.DownloaderDownloadNoVideo3:
                break;
            case UiAction.DownloaderDownloadNoVideo4:
                break;
            case UiAction.DownloaderDownloadNoVideo5:
                break;
            case UiAction.DownloaderDownloadNoVideo6:
                break;
            case UiAction.DownloaderDownloadNoVideo7:
                break;
            case UiAction.DownloaderPreview0:
                break;
            case UiAction.DownloaderPreview1:
                break;
            case UiAction.DownloaderPreview2:
                break;
            case UiAction.DownloaderPreview3:
                break;
            case UiAction.DownloaderPreview4:
                break;
            case UiAction.DownloaderPreview5:
                break;
            case UiAction.DownloaderPreview6:
                break;
            case UiAction.DownloaderPreview7:
                break;
            case UiAction.SongSelectBack:
                break;
            case UiAction.SongSelectMods:
                break;
            case UiAction.SongSelectBeatmapOptions:
                break;
            case UiAction.SongSelectBeatmapOptionsSearch:
                break;
            case UiAction.SongSelectBeatmapOptionsFavorite:
                break;
            case UiAction.SongSelectBeatmapOptionsAlgorithm:
                break;
            case UiAction.SongSelectBeatmapOptionsSort:
                break;
            case UiAction.SongSelectBeatmapOptionsFolder:
                break;
            case UiAction.SongSelectRandom:
                break;
            case UiAction.SongSelectFirstSet:
                break;
            case UiAction.SongSelectSet0:
                break;
            case UiAction.SongSelectSet1:
                break;
            case UiAction.SongSelectSet2:
                break;
            case UiAction.SongSelectSet3:
                break;
            case UiAction.SongSelectSet4:
                break;
            case UiAction.SongSelectSet5:
                break;
            case UiAction.SongSelectSet6:
                break;
            case UiAction.SongSelectSet7:
                break;
            case UiAction.SongSelectDifficulty0:
                break;
            case UiAction.SongSelectDifficulty1:
                break;
            case UiAction.SongSelectDifficulty2:
                break;
            case UiAction.SongSelectDifficulty3:
                break;
            case UiAction.SongSelectDifficulty4:
                break;
            case UiAction.SongSelectDifficulty5:
                break;
            case UiAction.SongSelectDifficulty6:
                break;
            case UiAction.SongSelectDifficulty7:
                break;
            case UiAction.SongSelectDifficulty8:
                break;
            case UiAction.SongSelectDifficulty9:
                break;
            case UiAction.SongSelectDifficulty10:
                break;
            case UiAction.SongSelectDifficulty11:
                break;
            case UiAction.SongSelectDifficulty12:
                break;
            case UiAction.SongSelectDifficulty13:
                break;
            case UiAction.SongSelectDifficulty14:
                break;
            case UiAction.SongSelectDifficulty15:
                break;
            case UiAction.SongSelectPropertiesDismiss:
                break;
            case UiAction.SongSelectPropertiesPanel:
                break;
            case UiAction.SongSelectPropertiesOffsetInput:
                break;
            case UiAction.SongSelectPropertiesOffsetMinus:
                break;
            case UiAction.SongSelectPropertiesOffsetPlus:
                break;
            case UiAction.SongSelectPropertiesFavorite:
                break;
            case UiAction.SongSelectPropertiesManageCollections:
                break;
            case UiAction.SongSelectPropertiesDelete:
                break;
            case UiAction.SongSelectPropertiesDeleteConfirm:
                break;
            case UiAction.SongSelectPropertiesDeleteCancel:
                break;
            case UiAction.SongSelectCollectionsNewFolder:
                break;
            case UiAction.SongSelectCollectionsClose:
                break;
            case UiAction.SongSelectCollectionToggle0:
                break;
            case UiAction.SongSelectCollectionToggle1:
                break;
            case UiAction.SongSelectCollectionToggle2:
                break;
            case UiAction.SongSelectCollectionToggle3:
                break;
            case UiAction.SongSelectCollectionToggle4:
                break;
            case UiAction.SongSelectCollectionToggle5:
                break;
            case UiAction.SongSelectCollectionToggle6:
                break;
            case UiAction.SongSelectCollectionToggle7:
                break;
            case UiAction.SongSelectCollectionDelete0:
                break;
            case UiAction.SongSelectCollectionDelete1:
                break;
            case UiAction.SongSelectCollectionDelete2:
                break;
            case UiAction.SongSelectCollectionDelete3:
                break;
            case UiAction.SongSelectCollectionDelete4:
                break;
            case UiAction.SongSelectCollectionDelete5:
                break;
            case UiAction.SongSelectCollectionDelete6:
                break;
            case UiAction.SongSelectCollectionDelete7:
                break;
            case UiAction.SongSelectCollectionDeleteConfirm:
                break;
            case UiAction.SongSelectCollectionDeleteCancel:
                break;
            default:
                return false;
        }

        return false;
    }

    private bool ExecuteMusicCommand(MenuMusicCommand command)
    {
        _musicController.Execute(command);
        _mainMenu.SetNowPlaying(_musicController.State);
        return true;
    }
}
