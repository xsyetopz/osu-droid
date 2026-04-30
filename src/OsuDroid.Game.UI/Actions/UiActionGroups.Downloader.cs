namespace OsuDroid.Game.UI.Actions;

public static partial class UiActionGroups
{
    private static readonly UiIndexedActionMap s_downloaderResultCardSlots = new(
        UiAction.DownloaderResultCardSlot0,
        UiAction.DownloaderResultCardSlot7
    );
    private static readonly UiIndexedActionMap s_downloaderResultPreviewSlots = new(
        UiAction.DownloaderResultPreviewSlot0,
        UiAction.DownloaderResultPreviewSlot7
    );
    private static readonly UiIndexedActionMap s_downloaderDetailsDifficultySlots = new(
        UiAction.DownloaderDetailsDifficultySlot0,
        UiAction.DownloaderDetailsDifficultySlot15
    );

    private static readonly UiIndexedActionMap s_downloaderSortChoices = new(
        UiAction.DownloaderSortTitle,
        UiAction.DownloaderSortSubmittedDate
    );
    private static readonly UiIndexedActionMap s_downloaderStatusChoices = new(
        UiAction.DownloaderStatusAll,
        UiAction.DownloaderStatusGraveyard
    );

    public static bool TryGetDownloaderResultCardSlotIndex(UiAction action, out int index) =>
        s_downloaderResultCardSlots.TryGetIndex(action, out index);

    public static bool TryGetDownloaderResultCardSlotAction(int index, out UiAction action) =>
        s_downloaderResultCardSlots.TryGetAction(index, out action);

    public static bool TryGetDownloaderResultPreviewSlotIndex(UiAction action, out int index) =>
        s_downloaderResultPreviewSlots.TryGetIndex(action, out index);

    public static bool TryGetDownloaderResultPreviewSlotAction(int index, out UiAction action) =>
        s_downloaderResultPreviewSlots.TryGetAction(index, out action);

    public static bool TryGetDownloaderDetailsDifficultySlotIndex(UiAction action, out int index) =>
        s_downloaderDetailsDifficultySlots.TryGetIndex(action, out index);

    public static bool TryGetDownloaderDetailsDifficultySlotAction(
        int index,
        out UiAction action
    ) => s_downloaderDetailsDifficultySlots.TryGetAction(index, out action);

    public static bool IsDownloaderSortChoice(UiAction action) =>
        s_downloaderSortChoices.Contains(action);

    public static bool IsDownloaderStatusChoice(UiAction action) =>
        s_downloaderStatusChoices.Contains(action);
}
