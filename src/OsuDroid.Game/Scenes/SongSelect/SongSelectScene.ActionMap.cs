using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Frames;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) =>
        new(
            "SongSelect",
            "Song Select",
            string.Empty,
            Array.Empty<string>(),
            0,
            false,
            CreateFrame(viewport)
        );

    public static UiAction SetAction(int visibleSlot) =>
        UiActionGroups.TryGetSongSelectVisibleSetSlotAction(visibleSlot, out UiAction action)
            ? action
            : UiAction.None;

    public static int SetIndex(UiAction action) =>
        UiActionGroups.TryGetSongSelectVisibleSetSlotIndex(action, out int index) ? index : -1;

    public static UiAction DifficultyAction(int index) =>
        UiActionGroups.TryGetSongSelectVisibleDifficultySlotAction(index, out UiAction action)
            ? action
            : UiAction.None;

    public static int DifficultyIndex(UiAction action) =>
        UiActionGroups.TryGetSongSelectVisibleDifficultySlotIndex(action, out int index)
            ? index
            : -1;

    public static UiAction CollectionToggleAction(int index) =>
        UiActionGroups.TryGetSongSelectCollectionToggleSlotAction(index, out UiAction action)
            ? action
            : UiAction.None;

    public static int CollectionToggleIndex(UiAction action) =>
        UiActionGroups.TryGetSongSelectCollectionToggleSlotIndex(action, out int index)
            ? index
            : -1;

    public static UiAction CollectionDeleteAction(int index) =>
        UiActionGroups.TryGetSongSelectCollectionDeleteSlotAction(index, out UiAction action)
            ? action
            : UiAction.None;

    public static int CollectionDeleteIndex(UiAction action) =>
        UiActionGroups.TryGetSongSelectCollectionDeleteSlotIndex(action, out int index)
            ? index
            : -1;
}
