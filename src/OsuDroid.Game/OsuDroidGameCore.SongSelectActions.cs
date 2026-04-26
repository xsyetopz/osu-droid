using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Scenes.MainMenu;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
#pragma warning disable IDE0072 // UiAction has cross-scene members; Song Select handles only its own actions.
    private bool HandleSongSelectUiAction(UiAction action, VirtualViewport viewport) =>
        action switch
        {
            UiAction.SongSelectBack => Do(() =>
                BackToMainMenu(MainMenuReturnTransition.SongSelectBack)
            ),
            UiAction.SongSelectMods => Do(OpenModSelect),
            UiAction.SongSelectBeatmapOptions => Do(_songSelect.OpenBeatmapOptions),
            UiAction.SongSelectBeatmapOptionsSearch => Do(() =>
                _songSelect.FocusBeatmapOptionsSearch(viewport)
            ),
            UiAction.SongSelectBeatmapOptionsFavorite => Do(
                _songSelect.ToggleBeatmapOptionsFavoriteOnly
            ),
            UiAction.SongSelectBeatmapOptionsAlgorithm => Do(ToggleSongSelectDifficultyAlgorithm),
            UiAction.SongSelectBeatmapOptionsSort => Do(_songSelect.CycleBeatmapOptionsSort),
            UiAction.SongSelectBeatmapOptionsFolder => Do(_songSelect.ToggleCollectionFilterPicker),
            UiAction.SongSelectRandom => Do(_songSelect.SelectRandomSet),
            UiAction.SongSelectPropertiesDismiss => Do(_songSelect.ClosePopups),
            UiAction.SongSelectPropertiesPanel => true,
            UiAction.SongSelectPropertiesOffsetInput => Do(() =>
                _songSelect.FocusOffsetInput(viewport)
            ),
            UiAction.SongSelectPropertiesOffsetMinus => Do(() => _songSelect.AdjustOffset(-1)),
            UiAction.SongSelectPropertiesOffsetPlus => Do(() => _songSelect.AdjustOffset(1)),
            UiAction.SongSelectPropertiesFavorite => Do(_songSelect.ToggleFavorite),
            UiAction.SongSelectPropertiesManageCollections => Do(_songSelect.OpenCollections),
            UiAction.SongSelectPropertiesDelete => Do(_songSelect.RequestDeleteBeatmap),
            UiAction.SongSelectPropertiesDeleteConfirm => Do(_songSelect.ConfirmDeleteBeatmap),
            UiAction.SongSelectPropertiesDeleteCancel => Do(_songSelect.CancelDeleteBeatmap),
            UiAction.SongSelectCollectionsNewFolder => Do(() =>
                _songSelect.FocusNewCollectionInput(viewport)
            ),
            UiAction.SongSelectCollectionsClose => Do(_songSelect.CloseCollections),
            UiAction.SongSelectCollectionDeleteConfirm => Do(_songSelect.ConfirmDeleteCollection),
            UiAction.SongSelectCollectionDeleteCancel => Do(_songSelect.CancelDeleteCollection),
            _ => false,
        };

    private void ToggleSongSelectDifficultyAlgorithm()
    {
        DifficultyAlgorithm algorithm = _songSelect.ToggleBeatmapOptionsAlgorithm();
        int storedValue = algorithm == Beatmaps.Difficulty.DifficultyAlgorithm.Standard ? 1 : 0;
        _settingsStore.SetInt("difficultyAlgorithm", storedValue);
        _options.SetIntValue("difficultyAlgorithm", storedValue);
    }

    private void OpenModSelect()
    {
        _textInputService.HideTextInput();
        _modSelect.SetSelectedBeatmap(_songSelect.SelectedBeatmap);
        _activeScene = ActiveScene.ModSelect;
    }
}
