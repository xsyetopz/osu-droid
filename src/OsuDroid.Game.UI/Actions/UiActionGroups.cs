namespace OsuDroid.Game.UI.Actions;

public static class UiActionGroups
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
    private static readonly UiIndexedActionMap s_optionsActiveRows = new(
        UiAction.OptionsActiveRow0,
        UiAction.OptionsActiveRow63
    );
    private static readonly UiIndexedActionMap s_optionsSections = new(
        UiAction.OptionsSectionGeneral,
        UiAction.OptionsSectionAdvanced
    );
    private static readonly UiIndexedActionMap s_optionsToggles = new(
        UiAction.OptionsToggleServerConnection,
        UiAction.OptionsToggleBeatmapSounds
    );
    private static readonly UiIndexedActionMap s_downloaderSortChoices = new(
        UiAction.DownloaderSortTitle,
        UiAction.DownloaderSortSubmittedDate
    );
    private static readonly UiIndexedActionMap s_downloaderStatusChoices = new(
        UiAction.DownloaderStatusAll,
        UiAction.DownloaderStatusGraveyard
    );
    private static readonly UiIndexedActionMap s_modSelectPresetSlots = new(
        UiAction.ModSelectPresetSlot0,
        UiAction.ModSelectPresetSlot15
    );
    private static readonly UiIndexedActionMap s_modSelectCatalogModToggles = new(
        UiAction.ModSelectCatalogModToggle0,
        UiAction.ModSelectCatalogModToggle31
    );
    private static readonly UiIndexedActionMap s_modSelectCustomizeSettingDecreases = new(
        UiAction.ModSelectCustomizeSettingDecrease0,
        UiAction.ModSelectCustomizeSettingDecrease31
    );
    private static readonly UiIndexedActionMap s_modSelectCustomizeSettingIncreases = new(
        UiAction.ModSelectCustomizeSettingIncrease0,
        UiAction.ModSelectCustomizeSettingIncrease31
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

    public static bool TryGetOptionsActiveRowIndex(UiAction action, out int index) =>
        s_optionsActiveRows.TryGetIndex(action, out index);

    public static bool TryGetOptionsActiveRowAction(int index, out UiAction action) =>
        s_optionsActiveRows.TryGetAction(index, out action);

    public static bool IsOptionsSection(UiAction action) => s_optionsSections.Contains(action);

    public static bool IsOptionsToggle(UiAction action) => s_optionsToggles.Contains(action);

    public static bool IsOptionsActiveRow(UiAction action) => s_optionsActiveRows.Contains(action);

    public static bool IsDownloaderSortChoice(UiAction action) =>
        s_downloaderSortChoices.Contains(action);

    public static bool IsDownloaderStatusChoice(UiAction action) =>
        s_downloaderStatusChoices.Contains(action);

    public static bool TryGetModSelectPresetSlotIndex(UiAction action, out int index) =>
        s_modSelectPresetSlots.TryGetIndex(action, out index);

    public static bool TryGetModSelectPresetSlotAction(int index, out UiAction action) =>
        s_modSelectPresetSlots.TryGetAction(index, out action);

    public static bool TryGetModSelectCatalogModToggleIndex(UiAction action, out int index) =>
        s_modSelectCatalogModToggles.TryGetIndex(action, out index);

    public static bool TryGetModSelectCatalogModToggleAction(int index, out UiAction action) =>
        s_modSelectCatalogModToggles.TryGetAction(index, out action);

    public static bool TryGetModSelectCustomizeSettingDecreaseIndex(
        UiAction action,
        out int index
    ) => s_modSelectCustomizeSettingDecreases.TryGetIndex(action, out index);

    public static bool TryGetModSelectCustomizeSettingDecreaseAction(
        int index,
        out UiAction action
    ) => s_modSelectCustomizeSettingDecreases.TryGetAction(index, out action);

    public static bool TryGetModSelectCustomizeSettingIncreaseIndex(
        UiAction action,
        out int index
    ) => s_modSelectCustomizeSettingIncreases.TryGetIndex(action, out index);

    public static bool TryGetModSelectCustomizeSettingIncreaseAction(
        int index,
        out UiAction action
    ) => s_modSelectCustomizeSettingIncreases.TryGetAction(index, out action);
}
