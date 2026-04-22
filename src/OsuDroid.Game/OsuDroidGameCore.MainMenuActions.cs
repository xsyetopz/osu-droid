using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

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
                mainMenu.OpenAboutDialog();
                return true;
            case UiAction.MainMenuAboutClose:
                mainMenu.CloseAboutDialog();
                return true;
            case UiAction.MainMenuAboutChangelog:
                PendingExternalUrl = "https://osudroid.moe/changelog/latest";
                mainMenu.CloseAboutDialog();
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
            case UiAction.MainMenuBeatmapDownloader:
                PreserveDownloaderMusic();
                activeScene = ActiveScene.BeatmapDownloader;
                beatmapDownloader.Enter();
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
            default:
                return false;
        }
    }

    private bool ExecuteMusicCommand(MenuMusicCommand command)
    {
        musicController.Execute(command);
        mainMenu.SetNowPlaying(musicController.State);
        return true;
    }
}
