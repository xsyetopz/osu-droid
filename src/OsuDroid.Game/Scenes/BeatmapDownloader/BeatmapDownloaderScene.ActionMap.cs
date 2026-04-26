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

        int index = IndexedActionOffset(action, UiAction.DownloaderDownload0, 8);
        return index >= 0
            ? index
            : IndexedActionOffset(action, UiAction.DownloaderDownloadNoVideo0, 8);
    }

    public static bool IsNoVideoAction(UiAction action) =>
        action
            is UiAction.DownloaderDownloadFirstNoVideo
                or UiAction.DownloaderDownloadNoVideo0
                or UiAction.DownloaderDownloadNoVideo1
                or UiAction.DownloaderDownloadNoVideo2
                or UiAction.DownloaderDownloadNoVideo3
                or UiAction.DownloaderDownloadNoVideo4
                or UiAction.DownloaderDownloadNoVideo5
                or UiAction.DownloaderDownloadNoVideo6
                or UiAction.DownloaderDownloadNoVideo7;

    public static int CardIndex(UiAction action) =>
        IndexedActionOffset(action, UiAction.DownloaderCard0, 8);

    public static int PreviewIndex(UiAction action) =>
        IndexedActionOffset(action, UiAction.DownloaderPreview0, 8);

    public static int DifficultyIndex(UiAction action) =>
        IndexedActionOffset(action, UiAction.DownloaderDetailsDifficulty0, 16);

    private static UiAction CardAction(int index) =>
        IndexedAction(UiAction.DownloaderCard0, index, 8);

    private static UiAction PreviewAction(int index) =>
        IndexedAction(UiAction.DownloaderPreview0, index, 8);

    private static UiAction DownloadAction(int index) =>
        IndexedAction(UiAction.DownloaderDownload0, index, 8);

    private static UiAction NoVideoAction(int index) =>
        IndexedAction(UiAction.DownloaderDownloadNoVideo0, index, 8);

    private static UiAction DifficultyAction(int index) =>
        IndexedAction(UiAction.DownloaderDetailsDifficulty0, index, 16);

    private static int IndexedActionOffset(UiAction action, UiAction first, int count)
    {
        int index = (int)action - (int)first;
        return (uint)index < (uint)count ? index : -1;
    }

    private static UiAction IndexedAction(UiAction first, int index, int count) =>
        (uint)index < (uint)count ? (UiAction)((int)first + index) : UiAction.None;
}
