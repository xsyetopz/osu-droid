using OsuDroid.Game.Scenes.MainMenu;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    public void TapMainMenuCookie() => _mainMenu.ToggleCookie();

    public void BackToMainMenu() => BackToMainMenu(MainMenuReturnTransition.None);

    public void BackToMainMenu(MainMenuReturnTransition transition)
    {
        string? returnBackgroundPath =
            _activeScene == ActiveScene.SongSelect
            && transition == MainMenuReturnTransition.SongSelectBack
                ? _songSelect.SelectedBackgroundPath
                : null;

        if (_activeScene == ActiveScene.SongSelect)
        {
            _songSelect.Leave();
        }
        else if (_activeScene == ActiveScene.BeatmapDownloader)
        {
            _beatmapDownloader.Leave();
            RestoreDownloaderMusic();
        }

        _activeScene = ActiveScene.MainMenu;

        if (transition == MainMenuReturnTransition.SongSelectBack)
        {
            _mainMenu.StartReturnTransition(returnBackgroundPath);
        }
    }

    public MainMenuRoute HandleMainMenu(MainMenuAction action)
    {
        if (_activeScene != ActiveScene.MainMenu)
        {
            return MainMenuRoute.None;
        }

        LastRoute = _mainMenu.Handle(action);
        ApplyRoute(LastRoute);
        return LastRoute;
    }

    public MainMenuRoute TapMainMenu(MainMenuButtonSlot slot)
    {
        if (_activeScene != ActiveScene.MainMenu)
        {
            return MainMenuRoute.None;
        }

        LastRoute = _mainMenu.Tap(slot);
        ApplyRoute(LastRoute);
        return LastRoute;
    }

    public bool HandleUiLongPress(UiAction action, VirtualViewport _)
    {
        if (
            _activeScene == ActiveScene.ModSelect
            && UiActionGroups.TryGetModSelectPresetIndex(action, out int presetIndex)
        )
        {
            bool opened = _modSelect.OpenPresetDeleteDialog(presetIndex);
            if (opened)
            {
                PlayMenuSfx(UiAction.SongSelectBeatmapOptions);
            }

            return opened;
        }

        if (
            _activeScene != ActiveScene.SongSelect
            || !UiActionGroups.TryGetSongSelectDifficultyIndex(action, out int index)
        )
        {
            return false;
        }

        PlayMenuSfx(UiAction.SongSelectBeatmapOptions);
        _songSelect.OpenPropertiesForDifficulty(index);
        return true;
    }

    public string? ConsumePendingExternalUrl()
    {
        string? pendingUrl = PendingExternalUrl;
        PendingExternalUrl = null;
        return pendingUrl;
    }
}
