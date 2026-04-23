namespace OsuDroid.Game.UI.Actions;

public static class UiActionGroups
{
    private static readonly UiIndexedActionMap s_downloaderCards = new(UiAction.DownloaderCard0, UiAction.DownloaderCard7);
    private static readonly UiIndexedActionMap s_downloaderPreviews = new(UiAction.DownloaderPreview0, UiAction.DownloaderPreview7);
    private static readonly UiIndexedActionMap s_downloaderDetailsDifficulties = new(UiAction.DownloaderDetailsDifficulty0, UiAction.DownloaderDetailsDifficulty15);
    private static readonly UiIndexedActionMap s_songSelectSets = new(UiAction.SongSelectSet0, UiAction.SongSelectSet7, (UiAction.SongSelectFirstSet, 0));
    private static readonly UiIndexedActionMap s_songSelectDifficulties = new(UiAction.SongSelectDifficulty0, UiAction.SongSelectDifficulty15);
    private static readonly UiIndexedActionMap s_songSelectCollectionToggles = new(UiAction.SongSelectCollectionToggle0, UiAction.SongSelectCollectionToggle7);
    private static readonly UiIndexedActionMap s_songSelectCollectionDeletes = new(UiAction.SongSelectCollectionDelete0, UiAction.SongSelectCollectionDelete7);
    private static readonly UiIndexedActionMap s_optionsRows = new(UiAction.OptionsRow0, UiAction.OptionsRow63);
    private static readonly UiIndexedActionMap s_optionsSections = new(UiAction.OptionsSectionGeneral, UiAction.OptionsSectionAdvanced);
    private static readonly UiIndexedActionMap s_optionsToggles = new(UiAction.OptionsToggleServerConnection, UiAction.OptionsToggleBeatmapSounds);
    private static readonly UiIndexedActionMap s_downloaderSortChoices = new(UiAction.DownloaderSortTitle, UiAction.DownloaderSortSubmittedDate);
    private static readonly UiIndexedActionMap s_downloaderStatusChoices = new(UiAction.DownloaderStatusAll, UiAction.DownloaderStatusGraveyard);

    public static bool TryGetDownloaderCardIndex(UiAction action, out int index) => s_downloaderCards.TryGetIndex(action, out index);

    public static bool TryGetDownloaderCardAction(int index, out UiAction action) => s_downloaderCards.TryGetAction(index, out action);

    public static bool TryGetDownloaderPreviewIndex(UiAction action, out int index) => s_downloaderPreviews.TryGetIndex(action, out index);

    public static bool TryGetDownloaderPreviewAction(int index, out UiAction action) => s_downloaderPreviews.TryGetAction(index, out action);

    public static bool TryGetDownloaderDetailsDifficultyIndex(UiAction action, out int index) => s_downloaderDetailsDifficulties.TryGetIndex(action, out index);

    public static bool TryGetDownloaderDetailsDifficultyAction(int index, out UiAction action) => s_downloaderDetailsDifficulties.TryGetAction(index, out action);

    public static bool TryGetSongSelectSetIndex(UiAction action, out int index) => s_songSelectSets.TryGetIndex(action, out index);

    public static bool TryGetSongSelectSetAction(int index, out UiAction action) => s_songSelectSets.TryGetAction(index, out action);

    public static bool TryGetSongSelectDifficultyIndex(UiAction action, out int index) => s_songSelectDifficulties.TryGetIndex(action, out index);

    public static bool TryGetSongSelectDifficultyAction(int index, out UiAction action) => s_songSelectDifficulties.TryGetAction(index, out action);

    public static bool TryGetSongSelectCollectionToggleIndex(UiAction action, out int index) => s_songSelectCollectionToggles.TryGetIndex(action, out index);

    public static bool TryGetSongSelectCollectionToggleAction(int index, out UiAction action) => s_songSelectCollectionToggles.TryGetAction(index, out action);

    public static bool TryGetSongSelectCollectionDeleteIndex(UiAction action, out int index) => s_songSelectCollectionDeletes.TryGetIndex(action, out index);

    public static bool TryGetSongSelectCollectionDeleteAction(int index, out UiAction action) => s_songSelectCollectionDeletes.TryGetAction(index, out action);

    public static bool TryGetOptionsRowIndex(UiAction action, out int index) => s_optionsRows.TryGetIndex(action, out index);

    public static bool TryGetOptionsRowAction(int index, out UiAction action) => s_optionsRows.TryGetAction(index, out action);

    public static bool IsOptionsSection(UiAction action) => s_optionsSections.Contains(action);

    public static bool IsOptionsToggle(UiAction action) => s_optionsToggles.Contains(action);

    public static bool IsOptionsRow(UiAction action) => s_optionsRows.Contains(action);

    public static bool IsDownloaderSortChoice(UiAction action) => s_downloaderSortChoices.Contains(action);

    public static bool IsDownloaderStatusChoice(UiAction action) => s_downloaderStatusChoices.Contains(action);
}
