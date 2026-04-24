namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    public void HandleUiAction(UiAction action, VirtualViewport viewport)
    {
        long start = PerfDiagnostics.Start();
        PlayMenuSfxBeforeAction(action);

        if (HandleIndexedUiAction(action, viewport))
        {
            PlayPendingOptionsSfx();
            PerfDiagnostics.Log("core.handleUiAction", start, $"action={action} indexed=true scene={_activeScene}");
            return;
        }

        _ = HandleMainMenuUiAction(action) ||
            HandleDownloaderUiAction(action, viewport) ||
            HandleSongSelectUiAction(action, viewport) ||
            HandleModSelectUiAction(action, viewport) ||
            HandleOptionsUiAction(action, viewport);

        PerfDiagnostics.Log("core.handleUiAction", start, $"action={action} scene={_activeScene}");
    }

    private void PlayMenuSfxBeforeAction(UiAction action)
    {
        if (_activeScene == ActiveScene.Options && IsOptionsAction(action))
        {
            return;
        }

        if (_activeScene == ActiveScene.MainMenu && action == UiAction.MainMenuThird && !_mainMenu.IsSecondMenu)
        {
            _activeMenuSfxPlayer.Play("seeya");
        }
        else
        {
            PlayMenuSfx(action);
        }
    }

    private bool HandleIndexedUiAction(UiAction action, VirtualViewport viewport)
    {
        if (UiActionGroups.TryGetDownloaderCardIndex(action, out int downloaderCardIndex))
        {
            _beatmapDownloader.SelectCard(downloaderCardIndex);
            return true;
        }

        if (UiActionGroups.TryGetDownloaderPreviewIndex(action, out int downloaderPreviewIndex))
        {
            _beatmapDownloader.PreviewCard(downloaderPreviewIndex);
            return true;
        }

        if (UiActionGroups.TryGetDownloaderDetailsDifficultyIndex(action, out int downloaderDetailsDifficultyIndex))
        {
            _beatmapDownloader.SelectDetailsDifficulty(downloaderDetailsDifficultyIndex);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectSetIndex(action, out int songSelectSetIndex))
        {
            _songSelect.SelectSet(songSelectSetIndex);
            _mainMenu.SetNowPlaying(_musicController.State);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectDifficultyIndex(action, out int songSelectDifficultyIndex))
        {
            _songSelect.SelectDifficulty(songSelectDifficultyIndex);
            _mainMenu.SetNowPlaying(_musicController.State);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectCollectionToggleIndex(action, out int songSelectCollectionToggleIndex))
        {
            _songSelect.HandleCollectionPrimaryAction(songSelectCollectionToggleIndex);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectCollectionDeleteIndex(action, out int songSelectCollectionDeleteIndex))
        {
            _songSelect.RequestDeleteCollection(songSelectCollectionDeleteIndex);
            return true;
        }

        if (UiActionGroups.TryGetOptionsRowIndex(action, out _) && _activeScene == ActiveScene.Options)
        {
            _options.HandleAction(action, viewport);
            ApplyChangedOptionsSetting(_options.ConsumeChangedSettingKey());
            return true;
        }

        if (UiActionGroups.TryGetModSelectToggleIndex(action, out int modSelectToggleIndex) && _activeScene == ActiveScene.ModSelect)
        {
            _activeMenuSfxPlayer.Play(_modSelect.ToggleMod(modSelectToggleIndex) ? "check-on" : "check-off");
            return true;
        }

        return false;
    }

    private void PlayPendingOptionsSfx()
    {
        string? key = _options.ConsumePendingSfxKey();
        if (key is not null)
        {
            _activeMenuSfxPlayer.Play(key);
        }
    }
}
