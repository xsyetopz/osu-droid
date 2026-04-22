using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    private bool HandleSongSelectUiAction(UiAction action, VirtualViewport viewport) => action switch
    {
        UiAction.SongSelectBack => Do(() => BackToMainMenu(MainMenuReturnTransition.SongSelectBack)),
        UiAction.SongSelectBeatmapOptions => Do(songSelect.OpenBeatmapOptions),
        UiAction.SongSelectBeatmapOptionsSearch => Do(() => songSelect.FocusBeatmapOptionsSearch(viewport)),
        UiAction.SongSelectBeatmapOptionsFavorite => Do(songSelect.ToggleBeatmapOptionsFavoriteOnly),
        UiAction.SongSelectBeatmapOptionsAlgorithm => Do(songSelect.ToggleBeatmapOptionsAlgorithm),
        UiAction.SongSelectBeatmapOptionsSort => Do(songSelect.CycleBeatmapOptionsSort),
        UiAction.SongSelectBeatmapOptionsFolder => Do(songSelect.ToggleCollectionFilterPicker),
        UiAction.SongSelectRandom => Do(songSelect.SelectRandomSet),
        UiAction.SongSelectPropertiesDismiss => Do(songSelect.ClosePopups),
        UiAction.SongSelectPropertiesPanel => true,
        UiAction.SongSelectPropertiesOffsetInput => Do(() => songSelect.FocusOffsetInput(viewport)),
        UiAction.SongSelectPropertiesOffsetMinus => Do(() => songSelect.AdjustOffset(-1)),
        UiAction.SongSelectPropertiesOffsetPlus => Do(() => songSelect.AdjustOffset(1)),
        UiAction.SongSelectPropertiesFavorite => Do(songSelect.ToggleFavorite),
        UiAction.SongSelectPropertiesManageCollections => Do(songSelect.OpenCollections),
        UiAction.SongSelectPropertiesDelete => Do(songSelect.RequestDeleteBeatmap),
        UiAction.SongSelectPropertiesDeleteConfirm => Do(songSelect.ConfirmDeleteBeatmap),
        UiAction.SongSelectPropertiesDeleteCancel => Do(songSelect.CancelDeleteBeatmap),
        UiAction.SongSelectCollectionsNewFolder => Do(() => songSelect.FocusNewCollectionInput(viewport)),
        UiAction.SongSelectCollectionsClose => Do(songSelect.CloseCollections),
        UiAction.SongSelectCollectionDeleteConfirm => Do(songSelect.ConfirmDeleteCollection),
        UiAction.SongSelectCollectionDeleteCancel => Do(songSelect.CancelDeleteCollection),
        _ => false,
    };
}
