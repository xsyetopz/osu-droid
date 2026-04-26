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
        UiActionGroups.TryGetSongSelectSetAction(visibleSlot, out UiAction action)
            ? action
            : UiAction.None;

    public static int SetIndex(UiAction action) =>
        UiActionGroups.TryGetSongSelectSetIndex(action, out int index) ? index : -1;

    public static UiAction DifficultyAction(int index) =>
        UiActionGroups.TryGetSongSelectDifficultyAction(index, out UiAction action)
            ? action
            : UiAction.None;

    public static int DifficultyIndex(UiAction action) =>
        UiActionGroups.TryGetSongSelectDifficultyIndex(action, out int index) ? index : -1;

    public static UiAction CollectionToggleAction(int index) =>
        UiActionGroups.TryGetSongSelectCollectionToggleAction(index, out UiAction action)
            ? action
            : UiAction.None;

    public static int CollectionToggleIndex(UiAction action) =>
        UiActionGroups.TryGetSongSelectCollectionToggleIndex(action, out int index) ? index : -1;

    public static UiAction CollectionDeleteAction(int index) =>
        UiActionGroups.TryGetSongSelectCollectionDeleteAction(index, out UiAction action)
            ? action
            : UiAction.None;

    public static int CollectionDeleteIndex(UiAction action) =>
        UiActionGroups.TryGetSongSelectCollectionDeleteIndex(action, out int index) ? index : -1;
}
