using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    public void HandleUiAction(UiAction action, VirtualViewport viewport)
    {
        var start = PerfDiagnostics.Start();
        PlayMenuSfxBeforeAction(action);

        if (HandleIndexedUiAction(action, viewport))
        {
            PlayPendingOptionsSfx();
            PerfDiagnostics.Log("core.handleUiAction", start, $"action={action} indexed=true scene={activeScene}");
            return;
        }

        _ = HandleMainMenuUiAction(action) ||
            HandleDownloaderUiAction(action, viewport) ||
            HandleSongSelectUiAction(action, viewport) ||
            HandleOptionsUiAction(action, viewport);

        PerfDiagnostics.Log("core.handleUiAction", start, $"action={action} scene={activeScene}");
    }

    private void PlayMenuSfxBeforeAction(UiAction action)
    {
        if (activeScene == ActiveScene.Options && IsOptionsAction(action))
            return;

        if (activeScene == ActiveScene.MainMenu && action == UiAction.MainMenuThird && !mainMenu.IsSecondMenu)
            activeMenuSfxPlayer.Play("seeya");
        else
            PlayMenuSfx(action);
    }

    private bool HandleIndexedUiAction(UiAction action, VirtualViewport viewport)
    {
        if (UiActionGroups.TryGetDownloaderCardIndex(action, out var downloaderCardIndex))
        {
            beatmapDownloader.SelectCard(downloaderCardIndex);
            return true;
        }

        if (UiActionGroups.TryGetDownloaderPreviewIndex(action, out var downloaderPreviewIndex))
        {
            beatmapDownloader.PreviewCard(downloaderPreviewIndex);
            return true;
        }

        if (UiActionGroups.TryGetDownloaderDetailsDifficultyIndex(action, out var downloaderDetailsDifficultyIndex))
        {
            beatmapDownloader.SelectDetailsDifficulty(downloaderDetailsDifficultyIndex);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectSetIndex(action, out var songSelectSetIndex))
        {
            songSelect.SelectSet(songSelectSetIndex);
            mainMenu.SetNowPlaying(musicController.State);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectDifficultyIndex(action, out var songSelectDifficultyIndex))
        {
            songSelect.SelectDifficulty(songSelectDifficultyIndex);
            mainMenu.SetNowPlaying(musicController.State);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectCollectionToggleIndex(action, out var songSelectCollectionToggleIndex))
        {
            songSelect.HandleCollectionPrimaryAction(songSelectCollectionToggleIndex);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectCollectionDeleteIndex(action, out var songSelectCollectionDeleteIndex))
        {
            songSelect.RequestDeleteCollection(songSelectCollectionDeleteIndex);
            return true;
        }

        if (UiActionGroups.TryGetOptionsRowIndex(action, out _) && activeScene == ActiveScene.Options)
        {
            options.HandleAction(action, viewport);
            ApplyChangedOptionsSetting(options.ConsumeChangedSettingKey());
            return true;
        }

        return false;
    }

    private void PlayPendingOptionsSfx()
    {
        var key = options.ConsumePendingSfxKey();
        if (key is not null)
            activeMenuSfxPlayer.Play(key);
    }
}
