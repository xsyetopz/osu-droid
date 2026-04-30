using OsuDroid.Game.UI.Actions;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    public static int DownloadIndex(UiAction action)
    {
        if (action is UiAction.DownloaderDownloadFirst or UiAction.DownloaderDownloadFirstNoVideo)
        {
            return 0;
        }

        int index = IndexedActionOffset(action, UiAction.DownloaderResultDownloadSlot0, 8);
        return index >= 0
            ? index
            : IndexedActionOffset(action, UiAction.DownloaderResultDownloadWithoutVideoSlot0, 8);
    }

    public static bool IsNoVideoAction(UiAction action) =>
        action
            is UiAction.DownloaderDownloadFirstNoVideo
                or UiAction.DownloaderResultDownloadWithoutVideoSlot0
                or UiAction.DownloaderResultDownloadWithoutVideoSlot1
                or UiAction.DownloaderResultDownloadWithoutVideoSlot2
                or UiAction.DownloaderResultDownloadWithoutVideoSlot3
                or UiAction.DownloaderResultDownloadWithoutVideoSlot4
                or UiAction.DownloaderResultDownloadWithoutVideoSlot5
                or UiAction.DownloaderResultDownloadWithoutVideoSlot6
                or UiAction.DownloaderResultDownloadWithoutVideoSlot7;

    public static int CardIndex(UiAction action) =>
        IndexedActionOffset(action, UiAction.DownloaderResultCardSlot0, 8);

    public static int PreviewIndex(UiAction action) =>
        IndexedActionOffset(action, UiAction.DownloaderResultPreviewSlot0, 8);

    public static int DifficultyIndex(UiAction action) =>
        IndexedActionOffset(action, UiAction.DownloaderDetailsDifficultySlot0, 16);

    private static UiAction CardAction(int index) =>
        IndexedAction(UiAction.DownloaderResultCardSlot0, index, 8);

    private static UiAction PreviewAction(int index) =>
        IndexedAction(UiAction.DownloaderResultPreviewSlot0, index, 8);

    private static UiAction DownloadAction(int index) =>
        IndexedAction(UiAction.DownloaderResultDownloadSlot0, index, 8);

    private static UiAction NoVideoAction(int index) =>
        IndexedAction(UiAction.DownloaderResultDownloadWithoutVideoSlot0, index, 8);

    private static UiAction DifficultyAction(int index) =>
        IndexedAction(UiAction.DownloaderDetailsDifficultySlot0, index, 16);

    private static int IndexedActionOffset(UiAction action, UiAction first, int count)
    {
        int index = (int)action - (int)first;
        return (uint)index < (uint)count ? index : -1;
    }

    private static UiAction IndexedAction(UiAction first, int index, int count) =>
        (uint)index < (uint)count ? (UiAction)((int)first + index) : UiAction.None;
}
