using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
#pragma warning disable IDE0072 // Sort mode defaults to Title for unknown values.
    private void AddModal(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (!_propertiesOpen && !_beatmapOptionsOpen)
        {
            return;
        }

        elements.Add(
            Fill(
                "songselect-popup-shade",
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                s_modalShade,
                1f,
                UiAction.SongSelectPropertiesDismiss
            )
        );

        if (_collectionsOpen)
        {
            AddCollectionsPanel(elements, viewport);
        }
        else if (_beatmapOptionsOpen)
        {
            AddBeatmapOptionsPanel(elements, viewport);
        }
        else
        {
            AddPropertiesPanel(elements, viewport);
        }

        if (_deleteBeatmapConfirmOpen)
        {
            AddConfirmPanel(
                elements,
                viewport,
                "songselect-delete-beatmap",
                _localizer["SongSelect_DeleteBeatmapTitle"],
                _localizer["SongSelect_DeleteBeatmapMessage"],
                UiAction.SongSelectPropertiesDeleteConfirm,
                UiAction.SongSelectPropertiesDeleteCancel
            );
        }
        else if (_collectionPendingDelete is not null)
        {
            AddConfirmPanel(
                elements,
                viewport,
                "songselect-delete-collection",
                _localizer["SongSelect_RemoveCollectionTitle"],
                _localizer["SongSelect_DeleteBeatmapMessage"],
                UiAction.SongSelectCollectionDeleteConfirm,
                UiAction.SongSelectCollectionDeleteCancel
            );
        }
    }
}
