using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private UiFrameSnapshot CreateFrame(VirtualViewport viewport)
    {
        var elements = new List<UiElementSnapshot>
        {
            Fill("downloader-background", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), s_background),
        };

        AddCards(elements, viewport);

        DroidSceneChrome.AddAppBar(elements, "downloader", viewport.VirtualWidth, s_appBar);
        DroidSceneChrome.AddBackButton(elements, "downloader", UiAction.DownloaderBack, s_appBar, s_white);
        AddTopBar(elements, viewport);

        if (_filtersOpen)
        {
            AddFilterPanel(elements, viewport);
        }

        if (_mirrorsOpen)
        {
            AddMirrorPanel(elements, viewport);
        }

        if (_selectedSetIndex is not null)
        {
            AddDetails(elements, viewport);
        }

        AddDownloadOverlay(elements, viewport);

        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private void AddTopBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float right = viewport.VirtualWidth - 12f * Dp;
        float mirrorWidth = 150f * Dp;
        float filtersWidth = 112f * Dp;
        float searchTrailingWidth = _hasSearchError || _isSearching ? 52f * Dp : 0f;
        UiRect searchBounds = SearchBounds(viewport);
        float searchRight = searchBounds.Right;
        BeatmapMirrorDefinition currentMirror = MirrorDefinition(_mirror);

        elements.Add(Fill("downloader-search", searchBounds, s_field, 1f, UiAction.DownloaderSearchBox, Radius));
        if (_isSearchFocused)
        {
            elements.Add(Fill("downloader-search-focus", searchBounds, s_white, 0.16f, UiAction.DownloaderSearchBox, Radius));
        }

        elements.Add(Text("downloader-search-text", string.IsNullOrWhiteSpace(_query) ? _localizer["BeatmapDownloader_SearchPlaceholder"] : _query, searchBounds.X + 14f * Dp, searchBounds.Y + 7f * Dp, searchBounds.Width - 56f * Dp, 22f * Dp, 14f * Dp, string.IsNullOrWhiteSpace(_query) ? s_muted : s_white, UiTextAlignment.Left, UiAction.DownloaderSearchBox));
        elements.Add(MaterialIcon("downloader-search-icon", UiMaterialIcon.Search, new UiRect(searchBounds.Right - 36f * Dp, searchBounds.Y + 6f * Dp, 24f * Dp, 24f * Dp), s_muted, 1f, UiAction.DownloaderSearchBox));

        float filtersX = searchRight + 6f * Dp;
        if (_isSearching)
        {
            elements.Add(Text("downloader-searching-indicator", "◌", filtersX, 8f * Dp, 52f * Dp, 38f * Dp, 22f * Dp, s_white, UiTextAlignment.Center));
            filtersX += 52f * Dp;
        }
        else if (_hasSearchError)
        {
            elements.Add(MaterialIcon("downloader-refresh", UiMaterialIcon.Refresh, new UiRect(filtersX + 14f * Dp, 16f * Dp, 24f * Dp, 24f * Dp), s_white, 1f, UiAction.DownloaderRefresh));
            filtersX += 52f * Dp;
        }

        AddCompoundButton(elements, "downloader-filters", new UiRect(filtersX, 4f * Dp, filtersWidth, 48f * Dp), _localizer["BeatmapDownloader_Filters"], UiAction.DownloaderFilters, UiMaterialIcon.Tune, null, _filtersOpen ? new UiColor(242, 114, 114, 41) : s_appBar, _filtersOpen ? 15f * Dp : 0f);

        float mirrorX = filtersX + filtersWidth;
        AddCompoundSpriteButton(elements, "downloader-mirror", new UiRect(mirrorX, 4f * Dp, mirrorWidth, 48f * Dp), currentMirror.Description, currentMirror.LogoAssetName, UiAction.DownloaderMirror, UiMaterialIcon.ArrowDropDown);
    }
}
