namespace OsuDroid.Game.UI;

public static class UiActionGroups
{
    public static bool TryGetDownloaderCardIndex(UiAction action, out int index) => TryGetContiguousIndex(action, UiAction.DownloaderCard0, UiAction.DownloaderCard7, out index);

    public static bool TryGetDownloaderPreviewIndex(UiAction action, out int index) => TryGetContiguousIndex(action, UiAction.DownloaderPreview0, UiAction.DownloaderPreview7, out index);

    public static bool TryGetDownloaderDetailsDifficultyIndex(UiAction action, out int index) => TryGetContiguousIndex(action, UiAction.DownloaderDetailsDifficulty0, UiAction.DownloaderDetailsDifficulty15, out index);

    public static bool TryGetSongSelectSetIndex(UiAction action, out int index)
    {
        if (action == UiAction.SongSelectFirstSet)
        {
            index = 0;
            return true;
        }

        return TryGetContiguousIndex(action, UiAction.SongSelectSet0, UiAction.SongSelectSet7, out index);
    }

    public static bool TryGetSongSelectDifficultyIndex(UiAction action, out int index) => TryGetContiguousIndex(action, UiAction.SongSelectDifficulty0, UiAction.SongSelectDifficulty15, out index);

    public static bool TryGetSongSelectCollectionToggleIndex(UiAction action, out int index) => TryGetContiguousIndex(action, UiAction.SongSelectCollectionToggle0, UiAction.SongSelectCollectionToggle7, out index);

    public static bool TryGetSongSelectCollectionDeleteIndex(UiAction action, out int index) => TryGetContiguousIndex(action, UiAction.SongSelectCollectionDelete0, UiAction.SongSelectCollectionDelete7, out index);

    public static bool IsOptionsSection(UiAction action) => IsContiguous(action, UiAction.OptionsSectionGeneral, UiAction.OptionsSectionAdvanced);

    public static bool IsOptionsToggle(UiAction action) => IsContiguous(action, UiAction.OptionsToggleServerConnection, UiAction.OptionsToggleBeatmapSounds);

    public static bool IsDownloaderSortChoice(UiAction action) => IsContiguous(action, UiAction.DownloaderSortTitle, UiAction.DownloaderSortSubmittedDate);

    public static bool IsDownloaderStatusChoice(UiAction action) => IsContiguous(action, UiAction.DownloaderStatusAll, UiAction.DownloaderStatusGraveyard);

    private static bool IsContiguous(UiAction action, UiAction first, UiAction last) => action >= first && action <= last;

    private static bool TryGetContiguousIndex(UiAction action, UiAction first, UiAction last, out int index)
    {
        if (!IsContiguous(action, first, last))
        {
            index = -1;
            return false;
        }

        index = (int)action - (int)first;
        return true;
    }
}
