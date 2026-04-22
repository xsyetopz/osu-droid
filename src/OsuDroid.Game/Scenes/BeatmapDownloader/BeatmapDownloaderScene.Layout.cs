using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class BeatmapDownloaderScene
{
    private UiFrameSnapshot CreateFrame(VirtualViewport viewport)
    {
        var elements = new List<UiElementSnapshot>
        {
            Fill("downloader-background", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), Background),
        };

        AddCards(elements, viewport);

        DroidSceneChrome.AddAppBar(elements, "downloader", viewport.VirtualWidth, AppBar);
        DroidSceneChrome.AddBackButton(elements, "downloader", UiAction.DownloaderBack, AppBar, White);
        AddTopBar(elements, viewport);

        if (filtersOpen)
            AddFilterPanel(elements, viewport);

        if (mirrorsOpen)
            AddMirrorPanel(elements, viewport);

        if (selectedSetIndex is not null)
            AddDetails(elements, viewport);

        AddDownloadOverlay(elements, viewport);

        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private void AddTopBar(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var right = viewport.VirtualWidth - 12f * Dp;
        var mirrorWidth = 150f * Dp;
        var filtersWidth = 112f * Dp;
        var searchTrailingWidth = hasSearchError || isSearching ? 52f * Dp : 0f;
        var searchBounds = SearchBounds(viewport);
        var searchRight = searchBounds.Right;
        var currentMirror = MirrorDefinition(mirror);

        elements.Add(Fill("downloader-search", searchBounds, Field, 1f, UiAction.DownloaderSearchBox, Radius));
        if (isSearchFocused)
            elements.Add(Fill("downloader-search-focus", searchBounds, White, 0.16f, UiAction.DownloaderSearchBox, Radius));
        elements.Add(Text("downloader-search-text", string.IsNullOrWhiteSpace(query) ? "Search for..." : query, searchBounds.X + 14f * Dp, searchBounds.Y + 7f * Dp, searchBounds.Width - 56f * Dp, 22f * Dp, 14f * Dp, string.IsNullOrWhiteSpace(query) ? Muted : White, UiTextAlignment.Left, UiAction.DownloaderSearchBox));
        elements.Add(MaterialIcon("downloader-search-icon", UiMaterialIcon.Search, new UiRect(searchBounds.Right - 36f * Dp, searchBounds.Y + 6f * Dp, 24f * Dp, 24f * Dp), Muted, 1f, UiAction.DownloaderSearchBox));

        var filtersX = searchRight + 6f * Dp;
        if (isSearching)
        {
            elements.Add(Text("downloader-searching-indicator", "◌", filtersX, 8f * Dp, 52f * Dp, 38f * Dp, 22f * Dp, White, UiTextAlignment.Center));
            filtersX += 52f * Dp;
        }
        else if (hasSearchError)
        {
            elements.Add(MaterialIcon("downloader-refresh", UiMaterialIcon.Refresh, new UiRect(filtersX + 14f * Dp, 16f * Dp, 24f * Dp, 24f * Dp), White, 1f, UiAction.DownloaderRefresh));
            filtersX += 52f * Dp;
        }

        AddCompoundButton(elements, "downloader-filters", new UiRect(filtersX, 4f * Dp, filtersWidth, 48f * Dp), "Filters", UiAction.DownloaderFilters, UiMaterialIcon.Tune, null, filtersOpen ? new UiColor(242, 114, 114, 41) : AppBar, filtersOpen ? 15f * Dp : 0f);

        var mirrorX = filtersX + filtersWidth;
        AddCompoundSpriteButton(elements, "downloader-mirror", new UiRect(mirrorX, 4f * Dp, mirrorWidth, 48f * Dp), currentMirror.Description, currentMirror.LogoAssetName, UiAction.DownloaderMirror, UiMaterialIcon.ArrowDropDown);
    }
}
