namespace OsuDroid.Game.UI;

public static class UiActionGroups
{
    private static readonly UiIndexedActionMap downloaderCards = new(UiAction.DownloaderCard0, UiAction.DownloaderCard7);
    private static readonly UiIndexedActionMap downloaderPreviews = new(UiAction.DownloaderPreview0, UiAction.DownloaderPreview7);
    private static readonly UiIndexedActionMap downloaderDetailsDifficulties = new(UiAction.DownloaderDetailsDifficulty0, UiAction.DownloaderDetailsDifficulty15);
    private static readonly UiIndexedActionMap songSelectSets = new(UiAction.SongSelectSet0, UiAction.SongSelectSet7, (UiAction.SongSelectFirstSet, 0));
    private static readonly UiIndexedActionMap songSelectDifficulties = new(UiAction.SongSelectDifficulty0, UiAction.SongSelectDifficulty15);
    private static readonly UiIndexedActionMap songSelectCollectionToggles = new(UiAction.SongSelectCollectionToggle0, UiAction.SongSelectCollectionToggle7);
    private static readonly UiIndexedActionMap songSelectCollectionDeletes = new(UiAction.SongSelectCollectionDelete0, UiAction.SongSelectCollectionDelete7);
    private static readonly UiIndexedActionMap optionsRows = new(UiAction.OptionsRow0, UiAction.OptionsRow63);
    private static readonly UiIndexedActionMap optionsSections = new(UiAction.OptionsSectionGeneral, UiAction.OptionsSectionAdvanced);
    private static readonly UiIndexedActionMap optionsToggles = new(UiAction.OptionsToggleServerConnection, UiAction.OptionsToggleBeatmapSounds);
    private static readonly UiIndexedActionMap downloaderSortChoices = new(UiAction.DownloaderSortTitle, UiAction.DownloaderSortSubmittedDate);
    private static readonly UiIndexedActionMap downloaderStatusChoices = new(UiAction.DownloaderStatusAll, UiAction.DownloaderStatusGraveyard);

    public static bool TryGetDownloaderCardIndex(UiAction action, out int index) => downloaderCards.TryGetIndex(action, out index);

    public static bool TryGetDownloaderCardAction(int index, out UiAction action) => downloaderCards.TryGetAction(index, out action);

    public static bool TryGetDownloaderPreviewIndex(UiAction action, out int index) => downloaderPreviews.TryGetIndex(action, out index);

    public static bool TryGetDownloaderPreviewAction(int index, out UiAction action) => downloaderPreviews.TryGetAction(index, out action);

    public static bool TryGetDownloaderDetailsDifficultyIndex(UiAction action, out int index) => downloaderDetailsDifficulties.TryGetIndex(action, out index);

    public static bool TryGetDownloaderDetailsDifficultyAction(int index, out UiAction action) => downloaderDetailsDifficulties.TryGetAction(index, out action);

    public static bool TryGetSongSelectSetIndex(UiAction action, out int index) => songSelectSets.TryGetIndex(action, out index);

    public static bool TryGetSongSelectSetAction(int index, out UiAction action) => songSelectSets.TryGetAction(index, out action);

    public static bool TryGetSongSelectDifficultyIndex(UiAction action, out int index) => songSelectDifficulties.TryGetIndex(action, out index);

    public static bool TryGetSongSelectDifficultyAction(int index, out UiAction action) => songSelectDifficulties.TryGetAction(index, out action);

    public static bool TryGetSongSelectCollectionToggleIndex(UiAction action, out int index) => songSelectCollectionToggles.TryGetIndex(action, out index);

    public static bool TryGetSongSelectCollectionToggleAction(int index, out UiAction action) => songSelectCollectionToggles.TryGetAction(index, out action);

    public static bool TryGetSongSelectCollectionDeleteIndex(UiAction action, out int index) => songSelectCollectionDeletes.TryGetIndex(action, out index);

    public static bool TryGetSongSelectCollectionDeleteAction(int index, out UiAction action) => songSelectCollectionDeletes.TryGetAction(index, out action);

    public static bool TryGetOptionsRowIndex(UiAction action, out int index) => optionsRows.TryGetIndex(action, out index);

    public static bool TryGetOptionsRowAction(int index, out UiAction action) => optionsRows.TryGetAction(index, out action);

    public static bool IsOptionsSection(UiAction action) => optionsSections.Contains(action);

    public static bool IsOptionsToggle(UiAction action) => optionsToggles.Contains(action);

    public static bool IsOptionsRow(UiAction action) => optionsRows.Contains(action);

    public static bool IsDownloaderSortChoice(UiAction action) => downloaderSortChoices.Contains(action);

    public static bool IsDownloaderStatusChoice(UiAction action) => downloaderStatusChoices.Contains(action);
}
