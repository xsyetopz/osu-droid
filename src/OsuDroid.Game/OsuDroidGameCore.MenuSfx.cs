using OsuDroid.Game.UI.Actions;
namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
#pragma warning disable IDE0072 // UiAction has cross-scene members; only sound-producing actions map to keys.
    private void PlayMenuSfx(UiAction action)
    {
        string? key = MenuSfxKeyFor(action);

        if (key is not null)
        {
            _activeMenuSfxPlayer.Play(key);
        }
    }

    private static string? MenuSfxKeyFor(UiAction action)
    {
        return UiActionGroups.IsOptionsSection(action) ||
            UiActionGroups.IsDownloaderSortChoice(action) ||
            UiActionGroups.IsDownloaderStatusChoice(action) ||
            UiActionGroups.TryGetDownloaderDetailsDifficultyIndex(action, out _) ||
            UiActionGroups.TryGetSongSelectDifficultyIndex(action, out _) ||
            UiActionGroups.TryGetSongSelectCollectionToggleIndex(action, out _)
            ? "menuclick"
            : UiActionGroups.IsOptionsToggle(action) ||
            UiActionGroups.TryGetDownloaderCardIndex(action, out _) ||
            UiActionGroups.TryGetDownloaderPreviewIndex(action, out _) ||
            UiActionGroups.TryGetSongSelectCollectionDeleteIndex(action, out _) ||
            IsBetween(action, UiAction.DownloaderDownload0, UiAction.DownloaderDownloadNoVideo7) ||
            UiActionGroups.TryGetSongSelectSetIndex(action, out _)
            ? "menuhit"
            : action switch
            {
                UiAction.MainMenuCookie or UiAction.MainMenuFirst or UiAction.MainMenuSecond or UiAction.MainMenuThird => "menuhit",
                UiAction.MainMenuBeatmapDownloader or UiAction.DownloaderDetailsPanel => "menuhit",
                UiAction.DownloaderDetailsPreview or UiAction.DownloaderDetailsDownload or UiAction.DownloaderDetailsDownloadNoVideo => "menuhit",
                UiAction.DownloaderDownloadFirst or UiAction.DownloaderDownloadFirstNoVideo => "menuhit",
                UiAction.SongSelectPropertiesOffsetMinus or UiAction.SongSelectPropertiesOffsetPlus => "menuhit",
                UiAction.SongSelectPropertiesFavorite or UiAction.SongSelectPropertiesManageCollections or UiAction.SongSelectPropertiesDelete => "menuclick",
                UiAction.SongSelectPropertiesOffsetInput or UiAction.SongSelectCollectionsNewFolder => "menuclick",
                UiAction.SongSelectPropertiesDeleteConfirm or UiAction.SongSelectCollectionDeleteConfirm => "menuhit",
                UiAction.SongSelectPropertiesDeleteCancel or UiAction.SongSelectCollectionDeleteCancel => "menuback",
                UiAction.SongSelectPropertiesDismiss or UiAction.SongSelectCollectionsClose => "menuback",
                UiAction.MainMenuExitConfirm => "menuhit",
                UiAction.MainMenuExitCancel => "menuback",
                UiAction.MainMenuVersionPill or UiAction.MainMenuAboutClose or UiAction.MainMenuAboutChangelog => "menuclick",
                UiAction.MainMenuAboutOsuWebsite or UiAction.MainMenuAboutOsuDroidWebsite or UiAction.MainMenuAboutDiscord => "menuclick",
                UiAction.MainMenuMusicPrevious or UiAction.MainMenuMusicPlay or UiAction.MainMenuMusicPause => "menuclick",
                UiAction.MainMenuMusicStop or UiAction.MainMenuMusicNext or UiAction.DownloaderSearchBox => "menuclick",
                UiAction.SongSelectMods or UiAction.SongSelectBeatmapOptions or UiAction.SongSelectBeatmapOptionsSearch or UiAction.SongSelectBeatmapOptionsFavorite or UiAction.SongSelectBeatmapOptionsAlgorithm or UiAction.SongSelectBeatmapOptionsSort or UiAction.SongSelectBeatmapOptionsFolder or UiAction.SongSelectRandom => "menuclick",
                UiAction.DownloaderSearchSubmit or UiAction.DownloaderRefresh or UiAction.DownloaderMirrorOsuDirect or UiAction.DownloaderMirrorCatboy => "menuclick",
                UiAction.DownloaderFilters or UiAction.DownloaderMirror or UiAction.DownloaderSort or UiAction.DownloaderOrder or UiAction.DownloaderStatus => "menuhit",
                UiAction.ModSelectClear or UiAction.ModSelectCustomize or UiAction.ModSelectSearchBox or UiAction.ModSelectPresetAdd => "menuclick",
                UiAction.OptionsBack or UiAction.DownloaderBack or UiAction.SongSelectBack or UiAction.ModSelectBack => "menuback",
                UiAction.DownloaderDetailsClose or UiAction.DownloaderDownloadCancel => "menuback",
                _ => null,
            };
    }

    private static bool IsBetween(UiAction action, UiAction first, UiAction last) => action >= first && action <= last;

    private static bool IsOptionsAction(UiAction action) =>
        UiActionGroups.IsOptionsSection(action) ||
        UiActionGroups.IsOptionsToggle(action) ||
        UiActionGroups.IsOptionsRow(action);
}
