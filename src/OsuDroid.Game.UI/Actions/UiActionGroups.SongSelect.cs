namespace OsuDroid.Game.UI.Actions;

public static partial class UiActionGroups
{
    private static readonly UiIndexedActionMap s_songSelectVisibleSetSlots = new(
        UiAction.SongSelectVisibleSetSlot0,
        UiAction.SongSelectVisibleSetSlot7,
        (UiAction.SongSelectFirstSet, 0)
    );
    private static readonly UiIndexedActionMap s_songSelectVisibleDifficultySlots = new(
        UiAction.SongSelectVisibleDifficultySlot0,
        UiAction.SongSelectVisibleDifficultySlot15
    );
    private static readonly UiIndexedActionMap s_songSelectCollectionToggleSlots = new(
        UiAction.SongSelectCollectionToggleSlot0,
        UiAction.SongSelectCollectionToggleSlot7
    );
    private static readonly UiIndexedActionMap s_songSelectCollectionDeleteSlots = new(
        UiAction.SongSelectCollectionDeleteSlot0,
        UiAction.SongSelectCollectionDeleteSlot7
    );

    public static bool TryGetSongSelectVisibleSetSlotIndex(UiAction action, out int index) =>
        s_songSelectVisibleSetSlots.TryGetIndex(action, out index);

    public static bool TryGetSongSelectVisibleSetSlotAction(int index, out UiAction action) =>
        s_songSelectVisibleSetSlots.TryGetAction(index, out action);

    public static bool TryGetSongSelectVisibleDifficultySlotIndex(UiAction action, out int index) =>
        s_songSelectVisibleDifficultySlots.TryGetIndex(action, out index);

    public static bool TryGetSongSelectVisibleDifficultySlotAction(
        int index,
        out UiAction action
    ) => s_songSelectVisibleDifficultySlots.TryGetAction(index, out action);

    public static bool TryGetSongSelectCollectionToggleSlotIndex(UiAction action, out int index) =>
        s_songSelectCollectionToggleSlots.TryGetIndex(action, out index);

    public static bool TryGetSongSelectCollectionToggleSlotAction(int index, out UiAction action) =>
        s_songSelectCollectionToggleSlots.TryGetAction(index, out action);

    public static bool TryGetSongSelectCollectionDeleteSlotIndex(UiAction action, out int index) =>
        s_songSelectCollectionDeleteSlots.TryGetIndex(action, out index);

    public static bool TryGetSongSelectCollectionDeleteSlotAction(int index, out UiAction action) =>
        s_songSelectCollectionDeleteSlots.TryGetAction(index, out action);
}
