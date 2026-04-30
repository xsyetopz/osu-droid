using OsuDroid.Game.Runtime.Diagnostics;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;

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
            PerfDiagnostics.Log(
                "core.handleUiAction",
                start,
                $"action={action} indexed=true scene={_activeScene}"
            );
            return;
        }

        _ =
            HandleMainMenuUiAction(action)
            || HandleDownloaderUiAction(action, viewport)
            || HandleSongSelectUiAction(action, viewport)
            || HandleModSelectUiAction(action, viewport)
            || HandleOptionsUiAction(action, viewport);

        PerfDiagnostics.Log("core.handleUiAction", start, $"action={action} scene={_activeScene}");
    }

    private void PlayMenuSfxBeforeAction(UiAction action)
    {
        if (_activeScene == ActiveScene.Options && IsOptionsAction(action))
        {
            return;
        }

        if (_activeScene == ActiveScene.MainMenu && action == UiAction.MainMenuExitConfirm)
        {
            return;
        }

        PlayMenuSfx(action);
    }

    private bool HandleIndexedUiAction(UiAction action, VirtualViewport viewport)
    {
        if (UiActionGroups.TryGetDownloaderResultCardSlotIndex(action, out int downloaderCardIndex))
        {
            _beatmapDownloader.SelectCard(downloaderCardIndex);
            return true;
        }

        if (
            UiActionGroups.TryGetDownloaderResultPreviewSlotIndex(
                action,
                out int downloaderPreviewIndex
            )
        )
        {
            _beatmapDownloader.PreviewCard(downloaderPreviewIndex);
            return true;
        }

        if (
            UiActionGroups.TryGetDownloaderDetailsDifficultySlotIndex(
                action,
                out int downloaderDetailsDifficultyIndex
            )
        )
        {
            _beatmapDownloader.SelectDetailsDifficulty(downloaderDetailsDifficultyIndex);
            return true;
        }

        if (UiActionGroups.TryGetSongSelectVisibleSetSlotIndex(action, out int songSelectSetIndex))
        {
            _songSelect.SelectSet(songSelectSetIndex);
            _mainMenu.SetNowPlaying(_musicController.State);
            return true;
        }

        if (
            UiActionGroups.TryGetSongSelectVisibleDifficultySlotIndex(
                action,
                out int songSelectDifficultyIndex
            )
        )
        {
            _songSelect.SelectDifficulty(songSelectDifficultyIndex);
            _mainMenu.SetNowPlaying(_musicController.State);
            return true;
        }

        if (
            UiActionGroups.TryGetSongSelectCollectionToggleSlotIndex(
                action,
                out int songSelectCollectionToggleIndex
            )
        )
        {
            _songSelect.HandleCollectionPrimaryAction(songSelectCollectionToggleIndex);
            return true;
        }

        if (
            UiActionGroups.TryGetSongSelectCollectionDeleteSlotIndex(
                action,
                out int songSelectCollectionDeleteIndex
            )
        )
        {
            _songSelect.RequestDeleteCollection(songSelectCollectionDeleteIndex);
            return true;
        }

        if (
            UiActionGroups.TryGetOptionsActiveRowIndex(action, out _)
            && _activeScene == ActiveScene.Options
        )
        {
            _options.HandleAction(action, viewport);
            ApplyChangedOptionsSetting(_options.ConsumeChangedSettingKey());
            return true;
        }

        if (
            UiActionGroups.TryGetModSelectCatalogModToggleIndex(
                action,
                out int modSelectToggleIndex
            )
            && _activeScene == ActiveScene.ModSelect
        )
        {
            _activeMenuSfxPlayer.Play(
                _modSelect.ToggleMod(modSelectToggleIndex) ? "check-on" : "check-off"
            );
            _songSelect.SetSelectedModState(_modSelect.CreateSelectionState());
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
