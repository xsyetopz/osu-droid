using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.UI.Actions;

namespace OsuDroid.Game;

#pragma warning disable IDE0072

public sealed partial class OsuDroidGameCore
{
    private bool HandleMainMenuUiAction(UiAction action) =>
        action switch
        {
            UiAction.MainMenuCookie => Execute(TapMainMenuCookie),
            UiAction.MainMenuPrimaryButton
            or UiAction.MainMenuSecondaryButton
            or UiAction.MainMenuTertiaryButton => Execute(() =>
                TapMainMenu(UiActionRouter.ToMainMenuSlot(action))
            ),
            UiAction.MainMenuVersionPill => Execute(_mainMenu.OpenAboutDialog),
            UiAction.MainMenuAboutClose => Execute(_mainMenu.CloseAboutDialog),
            UiAction.MainMenuAboutChangelog => Execute(() =>
            {
                PendingExternalUrl = "https://osudroid.moe/changelog/latest";
                _mainMenu.CloseAboutDialog();
            }),
            UiAction.MainMenuAboutOsuWebsite => Execute(() =>
                PendingExternalUrl = "https://osu.ppy.sh"
            ),
            UiAction.MainMenuAboutOsuDroidWebsite => Execute(() =>
                PendingExternalUrl = "https://osudroid.moe"
            ),
            UiAction.MainMenuAboutDiscord => Execute(() =>
                PendingExternalUrl = "https://discord.gg/nyD92cE"
            ),
            UiAction.MainMenuExitDialogPanel => true,
            UiAction.MainMenuExitConfirm => ConfirmMainMenuExit(),
            UiAction.MainMenuExitCancel => Execute(_mainMenu.CancelExitDialog),
            UiAction.MainMenuBeatmapDownloader => Execute(() =>
            {
                PreserveDownloaderMusic();
                _activeScene = ActiveScene.BeatmapDownloader;
                _beatmapDownloader.Enter();
            }),
            UiAction.MainMenuMusicPrevious => ExecuteMusicCommand(MenuMusicCommand.Previous),
            UiAction.MainMenuMusicPlay => ExecuteMusicCommand(MenuMusicCommand.Play),
            UiAction.MainMenuMusicPause => ExecuteMusicCommand(MenuMusicCommand.Pause),
            UiAction.MainMenuMusicStop => ExecuteMusicCommand(MenuMusicCommand.Stop),
            UiAction.MainMenuMusicNext => ExecuteMusicCommand(MenuMusicCommand.Next),
            _ => false,
        };

    private static bool Execute(Action action)
    {
        action();
        return true;
    }

    private bool ExecuteMusicCommand(MenuMusicCommand command)
    {
        _musicController.Execute(command);
        _mainMenu.SetNowPlaying(_musicController.State);
        return true;
    }

    private bool ConfirmMainMenuExit()
    {
        if (!_mainMenu.ConfirmExitDialog())
        {
            return false;
        }

        _musicController.Execute(MenuMusicCommand.Stop);
        _mainMenu.SetNowPlaying(_musicController.State);
        _activeMenuSfxPlayer.Play("seeya");
        return true;
    }
}
#pragma warning restore IDE0072
