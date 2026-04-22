using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed class BeatmapDownloaderScene
{
    private const int PageSize = 50;
    private const int VisibleSlots = 8;
    private const float Dp = DroidUiMetrics.DpScale;
    private const float BarHeight = 56f * Dp;
    private const float CardWidth = 395f * Dp;
    private const float CardMargin = 8f * Dp;
    private const float CardTopHeight = 100f * Dp;
    private const float CardDifficultyHeight = 38f * Dp;
    private const float DifficultyGlyphRowHeight = 34f * Dp;
    private const float CardFooterHeight = 104f * Dp;
    private const float CardHeight = CardTopHeight + CardDifficultyHeight + CardFooterHeight;
    private const float Radius = 14f * Dp;

    private static readonly UiColor Background = UiColor.Opaque(19, 19, 26);
    private static readonly UiColor AppBar = UiColor.Opaque(30, 30, 46);
    private static readonly UiColor Panel = UiColor.Opaque(22, 22, 34);
    private static readonly UiColor Footer = UiColor.Opaque(30, 30, 46);
    private static readonly UiColor Field = UiColor.Opaque(54, 54, 83);
    private static readonly UiColor Accent = UiColor.Opaque(243, 115, 115);
    private static readonly UiColor White = UiColor.Opaque(255, 255, 255);
    private static readonly UiColor Secondary = UiColor.Opaque(178, 178, 204);
    private static readonly UiColor Muted = UiColor.Opaque(130, 130, 168);
    private static readonly UiColor CoverFallback = UiColor.Opaque(75, 75, 128);
    private static readonly UiColor FilterPanel = UiColor.Opaque(33, 33, 51);
    private static readonly UiColor ModalShade = new(19, 19, 26, 190);

    private static readonly HttpClient CoverClient = new();

    private readonly IBeatmapMirrorClient mirrorClient;
    private readonly IBeatmapDownloadService downloadService;
    private IBeatmapPreviewPlayer previewPlayer;
    private readonly string coverCacheDirectory;
    private ITextInputService textInputService;
    private CancellationTokenSource searchCancellation = new();
    private IReadOnlyList<BeatmapMirrorSet> sets = [];
    private readonly HashSet<string> coverDownloads = new(StringComparer.Ordinal);
    private bool isSearching;
    private bool hasMore = true;
    private bool hasSearchError;
    private bool isSearchFocused;
    private bool filtersOpen;
    private bool sortDropdownOpen;
    private bool statusDropdownOpen;
    private bool mirrorsOpen;
    private bool downloading;
    private float sortDropdownScroll;
    private float statusDropdownScroll;
    private int previewPlayCount;
    private DateTime lastPreviewStartedUtc;
    private int? previewingSetIndex;
    private bool ownsPreviewPlayback;
    private int offset;
    private int visibleStartIndex;
    private int? selectedSetIndex;
    private int selectedDifficultyIndex;
    private float scrollOffset;
    private string query = string.Empty;
    private string? message = "Search beatmaps";
    private string? importedSetDirectory;
    private BeatmapMirrorKind mirror = BeatmapMirrorKind.OsuDirect;
    private BeatmapMirrorSort sort = BeatmapMirrorSort.RankedDate;
    private BeatmapMirrorOrder order = BeatmapMirrorOrder.Descending;
    private BeatmapRankedStatus? status;

    public BeatmapDownloaderScene(
        IBeatmapMirrorClient mirrorClient,
        IBeatmapDownloadService downloadService,
        ITextInputService textInputService,
        IBeatmapPreviewPlayer previewPlayer,
        string coverCacheDirectory)
    {
        this.mirrorClient = mirrorClient;
        this.downloadService = downloadService;
        this.textInputService = textInputService;
        this.previewPlayer = previewPlayer;
        this.coverCacheDirectory = coverCacheDirectory;
        Directory.CreateDirectory(coverCacheDirectory);
    }

    public string Query => query;

    public BeatmapMirrorKind Mirror => mirror;

    public string? ConsumeImportedSetDirectory()
    {
        var directory = importedSetDirectory;
        importedSetDirectory = null;
        return directory;
    }

    public void Enter()
    {
        if (sets.Count == 0 && !isSearching)
            _ = SearchAsync(false);
    }

    public void Leave()
    {
        HideSearchInput();
        if (ownsPreviewPlayback)
            previewPlayer.StopPreview();
        previewingSetIndex = null;
        ownsPreviewPlayback = false;
        searchCancellation.Cancel();
        filtersOpen = false;
        sortDropdownOpen = false;
        statusDropdownOpen = false;
        mirrorsOpen = false;
        selectedSetIndex = null;
    }

    public void SetTextInputService(ITextInputService service) => textInputService = service;

    public void SetPreviewPlayer(IBeatmapPreviewPlayer player)
    {
        if (ReferenceEquals(previewPlayer, player))
            return;

        if (ownsPreviewPlayback)
            previewPlayer.StopPreview();
        previewPlayer = player;
        previewingSetIndex = null;
        ownsPreviewPlayback = false;
    }

    public void FocusSearch(VirtualViewport viewport)
    {
        isSearchFocused = true;
        textInputService.RequestTextInput(new TextInputRequest(
            query,
            text => query = text,
            SubmitSearch,
            viewport.ToSurface(SearchBounds(viewport)),
            () => isSearchFocused = false));
    }

    public void SubmitSearch(string text)
    {
        query = text;
        isSearchFocused = false;
        _ = SearchAsync(false);
    }

    public void Refresh() => _ = SearchAsync(false);

    public void ToggleFilters()
    {
        HideSearchInput();
        filtersOpen = !filtersOpen;
        sortDropdownOpen = false;
        statusDropdownOpen = false;
        mirrorsOpen = false;
    }

    public void ToggleMirrorSelector()
    {
        HideSearchInput();
        mirrorsOpen = !mirrorsOpen;
        filtersOpen = false;
        sortDropdownOpen = false;
        statusDropdownOpen = false;
    }

    public void SelectMirror(BeatmapMirrorKind nextMirror)
    {
        if (mirror == nextMirror)
        {
            mirrorsOpen = false;
            return;
        }

        mirror = nextMirror;
        mirrorsOpen = false;
        _ = SearchAsync(false);
    }

    public void ToggleSortDropdown()
    {
        HideSearchInput();
        sortDropdownOpen = !sortDropdownOpen;
        statusDropdownOpen = false;
        sortDropdownScroll = 0f;
    }

    public void SetSort(BeatmapMirrorSort nextSort)
    {
        sort = nextSort;
        sortDropdownOpen = false;
        _ = SearchAsync(false);
    }

    public void ToggleOrder()
    {
        order = order == BeatmapMirrorOrder.Ascending ? BeatmapMirrorOrder.Descending : BeatmapMirrorOrder.Ascending;
        _ = SearchAsync(false);
    }

    public void ToggleStatusDropdown()
    {
        HideSearchInput();
        statusDropdownOpen = !statusDropdownOpen;
        sortDropdownOpen = false;
        statusDropdownScroll = 0f;
    }

    public void SetStatus(BeatmapRankedStatus? nextStatus)
    {
        status = nextStatus;
        statusDropdownOpen = false;
        _ = SearchAsync(false);
    }

    public void SelectDetailsDifficulty(int index)
    {
        if (selectedSetIndex is not int setIndex || setIndex < 0 || setIndex >= sets.Count)
            return;

        if (index < 0 || index >= sets[setIndex].Beatmaps.Count)
            return;

        selectedDifficultyIndex = index;
    }

    public void SelectCard(int visibleSlot)
    {
        HideSearchInput();
        var index = visibleStartIndex + visibleSlot;
        if (index < 0 || index >= sets.Count)
            return;

        selectedSetIndex = index;
        selectedDifficultyIndex = 0;
    }

    public void CloseDetails() => selectedSetIndex = null;

    public void PreviewCard(int visibleSlot) => Preview(visibleStartIndex + visibleSlot);

    public void PreviewDetails()
    {
        if (selectedSetIndex is int index)
            Preview(index);
    }

    public void Download(int index, bool withVideo) => _ = DownloadAsync(index, withVideo);

    public void DownloadVisible(int visibleSlot, bool withVideo) => Download(visibleStartIndex + visibleSlot, withVideo);

    public void DownloadDetails(bool withVideo)
    {
        if (selectedSetIndex is int index)
            Download(index, withVideo);
    }

    public void CancelDownload() => downloadService.CancelActiveDownload();

    private void HideSearchInput()
    {
        isSearchFocused = false;
        textInputService.HideTextInput();
    }

    public void Scroll(float deltaY, VirtualViewport viewport)
    {
        if (sortDropdownOpen)
        {
            sortDropdownScroll = Math.Clamp(sortDropdownScroll + deltaY, 0f, MaxDropdownScroll(SortOptions().Length, viewport));
            return;
        }

        if (statusDropdownOpen)
        {
            statusDropdownScroll = Math.Clamp(statusDropdownScroll + deltaY, 0f, MaxDropdownScroll(StatusOptions().Length, viewport));
            return;
        }

        if (selectedSetIndex is not null || filtersOpen || mirrorsOpen)
            return;

        scrollOffset = Math.Clamp(scrollOffset + deltaY, 0f, MaxScrollOffset(viewport));
        if (hasMore && !isSearching && scrollOffset >= MaxScrollOffset(viewport) - 40f * Dp)
            _ = SearchAsync(true);
    }

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new("BeatmapDownloader", "Beatmap Downloader", string.Empty, Array.Empty<string>(), 0, false, CreateFrame(viewport));

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

    private void AddCards(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (sets.Count == 0)
        {
            var statusText = isSearching ? $"Searching {MirrorDefinition(mirror).Description}..." : message ?? "No beatmaps found";
            elements.Add(Text("downloader-status", statusText, 70f * Dp, BarHeight + 42f * Dp, viewport.VirtualWidth - 140f * Dp, 40f * Dp, 20f * Dp, White, UiTextAlignment.Center));
            return;
        }

        var columns = Math.Max(1, (int)MathF.Floor(viewport.VirtualWidth / (CardWidth + CardMargin * 2f)));
        var gridWidth = columns * (CardWidth + CardMargin * 2f);
        var gridX = MathF.Max(0f, (viewport.VirtualWidth - gridWidth) / 2f);
        var top = BarHeight + 18f * Dp;
        var rowStep = CardHeight + CardMargin * 2f;
        var firstRow = Math.Max(0, (int)MathF.Floor(scrollOffset / rowStep));
        visibleStartIndex = firstRow * columns;
        var count = Math.Min(VisibleSlots, Math.Max(0, sets.Count - visibleStartIndex));

        for (var slot = 0; slot < count; slot++)
        {
            var index = visibleStartIndex + slot;
            var column = index % columns;
            var row = index / columns;
            var x = gridX + column * (CardWidth + CardMargin * 2f) + CardMargin;
            var y = top + row * rowStep - scrollOffset;
            AddCard(elements, sets[index], slot, index, x, y);
        }

        if (isSearching)
            elements.Add(Text("downloader-loading-more", "Loading more...", 0f, viewport.VirtualHeight - 38f * Dp, viewport.VirtualWidth, 24f * Dp, 16f * Dp, Secondary, UiTextAlignment.Center));
    }

    private void AddCard(List<UiElementSnapshot> elements, BeatmapMirrorSet set, int slot, int absoluteIndex, float x, float y)
    {
        var cardAction = CardAction(slot);
        elements.Add(Fill($"downloader-card-{slot}", new UiRect(x, y, CardWidth, CardHeight), Panel, 1f, cardAction, Radius));
        AddCover(elements, $"downloader-card-{slot}-cover", set, new UiRect(x, y, CardWidth, CardTopHeight), cardAction);
        elements.Add(Fill($"downloader-card-{slot}-cover-dim", new UiRect(x, y, CardWidth, CardTopHeight), CoverFallback, 0.2f, cardAction, Radius));
        AddStatusPill(elements, $"downloader-card-{slot}-status", set.Status, x + 12f * Dp, y + 12f * Dp, cardAction);
        elements.Add(Text($"downloader-card-{slot}-title", set.DisplayTitle, x + 16f * Dp, y + 32f * Dp, CardWidth - 32f * Dp, 30f * Dp, 17f * Dp, White, UiTextAlignment.Center, cardAction));
        elements.Add(Text($"downloader-card-{slot}-artist", set.DisplayArtist, x + 16f * Dp, y + 60f * Dp, CardWidth - 32f * Dp, 24f * Dp, 14f * Dp, Secondary, UiTextAlignment.Center, cardAction));
        AddDifficultyDots(elements, $"downloader-card-{slot}-diff", set.Beatmaps, x + 8f * Dp, y + CardTopHeight + (CardDifficultyHeight - DifficultyGlyphRowHeight) / 2f, CardWidth - 16f * Dp, UiAction.None, null, true, cardAction);

        var footerY = y + CardTopHeight + CardDifficultyHeight;
        elements.Add(Fill($"downloader-card-{slot}-footer", new UiRect(x, footerY, CardWidth, CardFooterHeight), Footer, 1f, cardAction));
        var buttonY = footerY + 12f * Dp;
        var previewAction = PreviewAction(slot);
        var downloadAction = DownloadAction(slot);
        var noVideoAction = NoVideoAction(slot);
        var noVideoVisible = MirrorDefinition(set.Mirror).SupportsNoVideoDownloads && set.HasVideo;
        var buttons = new List<DownloaderButtonSpec>
        {
            new($"downloader-card-{slot}-preview", previewingSetIndex == absoluteIndex ? UiMaterialIcon.Pause : UiMaterialIcon.PlayArrow, string.Empty, 52f * Dp, previewAction),
            new($"downloader-card-{slot}-download", UiMaterialIcon.Download, "Download", 116f * Dp, downloadAction),
        };
        if (noVideoVisible)
            buttons.Add(new DownloaderButtonSpec($"downloader-card-{slot}-no-video", UiMaterialIcon.Download, "Download (no video)", 174f * Dp, noVideoAction));
        AddButtonGroup(elements, buttons, x + CardWidth / 2f, buttonY);
        elements.Add(Text($"downloader-card-{slot}-creator", $"Mapped by {set.Creator}", x + 16f * Dp, footerY + 66f * Dp, CardWidth - 32f * Dp, 24f * Dp, 13f * Dp, Muted, UiTextAlignment.Center, cardAction));
    }

    private void AddFilterPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var y = BarHeight;
        elements.Add(Fill("downloader-filter-dismiss", new UiRect(0f, y, viewport.VirtualWidth, viewport.VirtualHeight - y), Background, 0f, UiAction.DownloaderFilters));
        elements.Add(Fill("downloader-filter-panel", new UiRect(0f, y, viewport.VirtualWidth, 64f * Dp), FilterPanel));
        var x = 12f * Dp;
        elements.Add(Text("downloader-filter-sort-label", "Sort by", x, y + 18f * Dp, 58f * Dp, 22f * Dp, 13f * Dp, Secondary));
        AddDropdownButton(elements, "downloader-filter-sort", new UiRect(x + 62f * Dp, y + 8f * Dp, 170f * Dp, 42f * Dp), SortText(sort), UiAction.DownloaderSort);
        elements.Add(Text("downloader-filter-order-label", "Order", x + 250f * Dp, y + 18f * Dp, 52f * Dp, 22f * Dp, 13f * Dp, Secondary));
        AddButton(elements, "downloader-filter-order", new UiRect(x + 306f * Dp, y + 8f * Dp, 126f * Dp, 42f * Dp), order == BeatmapMirrorOrder.Ascending ? "Ascending" : "Descending", UiAction.DownloaderOrder);
        elements.Add(Text("downloader-filter-status-label", "Status", x + 450f * Dp, y + 18f * Dp, 52f * Dp, 22f * Dp, 13f * Dp, Secondary));
        AddDropdownButton(elements, "downloader-filter-status", new UiRect(x + 506f * Dp, y + 8f * Dp, 150f * Dp, 42f * Dp), status is null ? "All" : RankedStatusText(status.Value), UiAction.DownloaderStatus);
        if (sortDropdownOpen)
            AddDropdownOptions(elements, "downloader-sort-option", x + 62f * Dp, y + 54f * Dp, 176f * Dp, SortOptions(), sortDropdownScroll, viewport);
        if (statusDropdownOpen)
            AddDropdownOptions(elements, "downloader-status-option", x + 506f * Dp, y + 54f * Dp, 150f * Dp, StatusOptions(), statusDropdownScroll, viewport);
    }

    private static (string Text, UiAction Action)[] SortOptions() =>
    [
        ("Title", UiAction.DownloaderSortTitle),
        ("Artist", UiAction.DownloaderSortArtist),
        ("BPM", UiAction.DownloaderSortBpm),
        ("Difficulty rating", UiAction.DownloaderSortDifficultyRating),
        ("Hit length", UiAction.DownloaderSortHitLength),
        ("Pass count", UiAction.DownloaderSortPassCount),
        ("Play count", UiAction.DownloaderSortPlayCount),
        ("Total length", UiAction.DownloaderSortTotalLength),
        ("Favourite count", UiAction.DownloaderSortFavouriteCount),
        ("Last updated", UiAction.DownloaderSortLastUpdated),
        ("Ranked date", UiAction.DownloaderSortRankedDate),
        ("Submitted date", UiAction.DownloaderSortSubmittedDate),
    ];

    private static (string Text, UiAction Action)[] StatusOptions() =>
    [
        ("All", UiAction.DownloaderStatusAll),
        ("Ranked", UiAction.DownloaderStatusRanked),
        ("Approved", UiAction.DownloaderStatusApproved),
        ("Qualified", UiAction.DownloaderStatusQualified),
        ("Loved", UiAction.DownloaderStatusLoved),
        ("Pending", UiAction.DownloaderStatusPending),
        ("WIP", UiAction.DownloaderStatusWorkInProgress),
        ("Graveyard", UiAction.DownloaderStatusGraveyard),
    ];

    private static void AddDropdownOptions(List<UiElementSnapshot> elements, string id, float x, float y, float width, (string Text, UiAction Action)[] actions, float scroll, VirtualViewport viewport)
    {
        var rowHeight = 42f * Dp;
        var padding = 8f * Dp;
        var availableHeight = Math.Max(rowHeight, viewport.VirtualHeight - y);
        var height = Math.Min(actions.Length * (rowHeight + 4f * Dp) + padding * 2f, availableHeight);
        elements.Add(Fill(id + "-panel", new UiRect(x, y, width, height), UiColor.Opaque(40, 40, 61), 1f, UiAction.None, Radius));
        var first = Math.Max(0, (int)MathF.Floor(scroll / (rowHeight + 4f * Dp)));
        var offsetY = padding;
        for (var i = first; i < actions.Length; i++)
        {
            var rowY = y + offsetY + (i - first) * (rowHeight + 4f * Dp);
            if (rowY > y + height - padding)
                break;

            AddButton(elements, $"{id}-{i}", new UiRect(x + padding, rowY, width - padding * 2f, rowHeight), actions[i].Text, actions[i].Action);
        }
    }

    private void AddMirrorPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var width = 210f * Dp;
        var x = viewport.VirtualWidth - width - 12f * Dp;
        var y = BarHeight + 4f * Dp;
        elements.Add(Fill("downloader-mirror-panel", new UiRect(x, y, width, 98f * Dp), Panel, 1f, UiAction.None, Radius));
        AddMirrorOption(elements, BeatmapMirrorKind.OsuDirect, x + 8f * Dp, y + 8f * Dp, UiAction.DownloaderMirrorOsuDirect);
        AddMirrorOption(elements, BeatmapMirrorKind.Catboy, x + 8f * Dp, y + 52f * Dp, UiAction.DownloaderMirrorCatboy);
    }

    private void AddMirrorOption(List<UiElementSnapshot> elements, BeatmapMirrorKind kind, float x, float y, UiAction action)
    {
        var definition = MirrorDefinition(kind);
        elements.Add(Fill($"downloader-mirror-{kind}", new UiRect(x, y, 194f * Dp, 38f * Dp), kind == mirror ? Field : Panel, 1f, action, Radius));
        elements.Add(new UiElementSnapshot($"downloader-mirror-{kind}-logo", UiElementKind.Sprite, new UiRect(x + 10f * Dp, y + 7f * Dp, 24f * Dp, 24f * Dp), White, 1f, definition.LogoAssetName, action));
        elements.Add(Text($"downloader-mirror-{kind}-text", definition.Description, x + 44f * Dp, y + 8f * Dp, 120f * Dp, 22f * Dp, 14f * Dp, White, UiTextAlignment.Left, action));
    }

    private void AddDetails(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (selectedSetIndex is not int index || index < 0 || index >= sets.Count)
            return;

        var set = sets[index];
        var beatmaps = set.Beatmaps;
        if (beatmaps.Count == 0)
            return;

        selectedDifficultyIndex = Math.Clamp(selectedDifficultyIndex, 0, beatmaps.Count - 1);
        var beatmap = beatmaps[selectedDifficultyIndex];
        elements.Add(Fill("downloader-details-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), ModalShade, 1f, UiAction.DownloaderDetailsClose));
        var panelWidth = Math.Min(500f * Dp, viewport.VirtualWidth - 80f * Dp);
        var x = (viewport.VirtualWidth - panelWidth) / 2f;
        var y = Math.Max(20f * Dp, (viewport.VirtualHeight - 348f * Dp) / 2f);
        elements.Add(Fill("downloader-details-panel", new UiRect(x, y, panelWidth, 348f * Dp), Panel, 1f, UiAction.DownloaderDetailsPanel, Radius));
        AddCover(elements, "downloader-details-cover", set, new UiRect(x, y, panelWidth, 100f * Dp), UiAction.DownloaderDetailsPanel);
        elements.Add(Fill("downloader-details-cover-dim", new UiRect(x, y, panelWidth, 100f * Dp), CoverFallback, 0.2f, UiAction.DownloaderDetailsPanel, Radius));
        elements.Add(Text("downloader-details-title", set.DisplayTitle, x + 16f * Dp, y + 18f * Dp, panelWidth - 120f * Dp, 26f * Dp, 17f * Dp, White));
        elements.Add(Text("downloader-details-artist", set.DisplayArtist, x + 16f * Dp, y + 44f * Dp, panelWidth - 120f * Dp, 22f * Dp, 14f * Dp, Secondary));
        elements.Add(Text("downloader-details-creator", $"Mapped by {set.Creator}", x + 16f * Dp, y + 68f * Dp, panelWidth - 120f * Dp, 20f * Dp, 13f * Dp, Muted));
        AddStatusPill(elements, "downloader-details-status", set.Status, x + panelWidth - StatusPillWidth(set.Status) - 16f * Dp, y + 16f * Dp, UiAction.DownloaderDetailsPanel);
        AddDifficultyDots(elements, "downloader-details-diff", beatmaps, x + 12f * Dp, y + 108f * Dp, panelWidth - 24f * Dp, UiAction.None, selectedDifficultyIndex, true, UiAction.None);
        var bodyY = y + 146f * Dp;
        elements.Add(Fill("downloader-details-body", new UiRect(x, bodyY, panelWidth, 202f * Dp), Footer, 1f, UiAction.DownloaderDetailsPanel));
        elements.Add(Text("downloader-details-version", beatmap.Version, x + 16f * Dp, bodyY + 12f * Dp, panelWidth - 32f * Dp, 24f * Dp, 15f * Dp, White));
        elements.Add(Text("downloader-details-stats", $"Star rating: {beatmap.StarRating:0.##}\nAR: {beatmap.ApproachRate:0.##} - OD: {beatmap.OverallDifficulty:0.##} - CS: {beatmap.CircleSize:0.##} - HP: {beatmap.HpDrainRate:0.##}\nCircles: {beatmap.CircleCount} - Sliders: {beatmap.SliderCount} - Spinners: {beatmap.SpinnerCount}\nLength: {TimeSpan.FromSeconds(beatmap.HitLength):m\\:ss} - BPM: {beatmap.Bpm:0.##}", x + 16f * Dp, bodyY + 38f * Dp, panelWidth - 32f * Dp, 92f * Dp, 13f * Dp, Secondary));
        var buttons = new List<DownloaderButtonSpec>
        {
            new("downloader-details-preview", previewingSetIndex == index ? UiMaterialIcon.Pause : UiMaterialIcon.PlayArrow, string.Empty, 52f * Dp, UiAction.DownloaderDetailsPreview),
            new("downloader-details-download", UiMaterialIcon.Download, "Download", 112f * Dp, UiAction.DownloaderDetailsDownload),
        };
        if (MirrorDefinition(set.Mirror).SupportsNoVideoDownloads && set.HasVideo)
            buttons.Add(new DownloaderButtonSpec("downloader-details-no-video", UiMaterialIcon.Download, "No video", 128f * Dp, UiAction.DownloaderDetailsDownloadNoVideo));
        AddButtonGroup(elements, buttons, x + panelWidth - 180f * Dp, bodyY + 138f * Dp);
    }

    private void AddDownloadOverlay(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var state = downloadService.State;
        if (!state.IsActive && !downloading)
            return;

        var progress = state.Progress;
        var text = progress?.State == "Importing"
            ? $"Importing {state.Filename}"
            : $"Downloading {state.Filename ?? "beatmap"}{FormatDownloadInfo(progress)}";
        var textWidth = Math.Min(400f * Dp, EstimateTextWidth(text, 13f * Dp));
        var width = Math.Clamp(textWidth + 110f * Dp, 300f * Dp, Math.Min(500f * Dp, viewport.VirtualWidth - 80f * Dp));
        var titleHeight = 44f * Dp;
        var bodyHeight = 58f * Dp;
        var cancelHeight = 42f * Dp;
        var height = titleHeight + bodyHeight + cancelHeight;
        var x = (viewport.VirtualWidth - width) / 2f;
        var y = (viewport.VirtualHeight - height) / 2f;
        elements.Add(Fill("downloader-download-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), new UiColor(0, 0, 0, 128)));
        elements.Add(Fill("downloader-download-panel", new UiRect(x, y, width, height), AppBar, 1f, UiAction.None, Radius));
        elements.Add(TextMiddle("downloader-download-title", "Download", x, y, width, titleHeight, 14f * Dp, Secondary, UiTextAlignment.Center));
        elements.Add(Fill("downloader-download-divider-title", new UiRect(x, y + titleHeight, width, 1f * Dp), Background, 0.45f));

        var bodyY = y + titleHeight;
        var contentWidth = Math.Min(width - 32f * Dp, 42f * Dp + textWidth + 12f * Dp);
        var contentX = x + (width - contentWidth) / 2f;
        elements.Add(TextMiddle("downloader-download-spinner", "◌", contentX, bodyY, 34f * Dp, bodyHeight, 30f * Dp, Accent, UiTextAlignment.Center));
        elements.Add(TextMiddle("downloader-download-text", text, contentX + 46f * Dp, bodyY, contentWidth - 46f * Dp, bodyHeight, 13f * Dp, White, UiTextAlignment.Left));
        elements.Add(Fill("downloader-download-divider-cancel", new UiRect(x, bodyY + bodyHeight, width, 1f * Dp), Background, 0.45f));

        var cancelY = y + titleHeight + bodyHeight;
        elements.Add(Fill("downloader-download-cancel-hit", new UiRect(x, cancelY, width, cancelHeight), Field, 1f, UiAction.DownloaderDownloadCancel, Radius));
        elements.Add(TextMiddle("downloader-download-cancel-text", "× Cancel", x, cancelY, width, cancelHeight, 14f * Dp, UiColor.Opaque(255, 191, 191), UiTextAlignment.Center, UiAction.DownloaderDownloadCancel));
    }

    private void AddCover(List<UiElementSnapshot> elements, string id, BeatmapMirrorSet set, UiRect bounds, UiAction action)
    {
        elements.Add(Fill(id + "-fallback", bounds, CoverFallback, 0.55f, action, Radius));
        var path = GetCoverPath(set);
        if (path is not null && File.Exists(path))
        {
            elements.Add(new UiElementSnapshot(id, UiElementKind.Sprite, bounds, White, 0.55f, Action: action, ExternalAssetPath: path, SpriteFit: UiSpriteFit.Cover));
            return;
        }

        StartCoverDownload(set);
    }

    private void StartCoverDownload(BeatmapMirrorSet set)
    {
        if (string.IsNullOrWhiteSpace(set.CoverUrl))
            return;

        var path = GetCoverPath(set);
        if (path is null || File.Exists(path) || !coverDownloads.Add(path))
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                using var response = await CoverClient.GetAsync(set.CoverUrl).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                using var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var destination = File.Create(path);
                await source.CopyToAsync(destination).ConfigureAwait(false);
            }
            catch (Exception)
            {
                TryDelete(path);
            }
            finally
            {
                coverDownloads.Remove(path);
            }
        });
    }

    private string? GetCoverPath(BeatmapMirrorSet set)
    {
        if (string.IsNullOrWhiteSpace(set.CoverUrl))
            return null;

        var extension = Path.GetExtension(new Uri(set.CoverUrl).AbsolutePath);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";
        return Path.Combine(coverCacheDirectory, BeatmapImportService.SanitizeArchiveName($"{set.Mirror}-{set.Id}") + extension);
    }

    private async Task SearchAsync(bool append)
    {
        if (isSearching)
            return;

        if (!append)
        {
            searchCancellation.Cancel();
            searchCancellation.Dispose();
            searchCancellation = new CancellationTokenSource();
            offset = 0;
            scrollOffset = 0f;
            sets = [];
            hasMore = true;
        }

        if (!hasMore)
            return;

        try
        {
            isSearching = true;
            hasSearchError = false;
            message = null;
            var request = new BeatmapMirrorSearchRequest(query, offset, PageSize, sort, order, status, mirror);
            var result = await mirrorClient.SearchAsync(request, searchCancellation.Token).ConfigureAwait(false);
            sets = append ? sets.Concat(result).ToArray() : result;
            offset += result.Count;
            hasMore = result.Count >= PageSize;
            message = sets.Count == 0 ? "No beatmaps found" : null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            message = "Failed to connect to server, please check your internet connection.";
            hasSearchError = true;
            hasMore = false;
        }
        finally
        {
            isSearching = false;
        }
    }

    private async Task DownloadAsync(int index, bool withVideo)
    {
        if (index < 0 || index >= sets.Count)
            return;

        downloading = true;
        var importResult = await downloadService.DownloadAndImportAsync(sets[index], withVideo, CancellationToken.None).ConfigureAwait(false);
        downloading = false;
        if (importResult.IsSuccess)
        {
            selectedSetIndex = null;
            importedSetDirectory = importResult.SetDirectory;
            message = "Beatmap imported";
        }
        else
        {
            message = importResult.ErrorMessage;
        }
    }

    private void Preview(int index)
    {
        if (index < 0 || index >= sets.Count || sets[index].Beatmaps.Count == 0)
            return;

        if (previewingSetIndex == index)
        {
            if (ownsPreviewPlayback)
                previewPlayer.StopPreview();
            previewingSetIndex = null;
            ownsPreviewPlayback = false;
            return;
        }

        if (previewPlayCount >= 2 && DateTime.UtcNow - lastPreviewStartedUtc < TimeSpan.FromSeconds(5))
            return;

        if (DateTime.UtcNow - lastPreviewStartedUtc >= TimeSpan.FromSeconds(5))
            previewPlayCount = 0;

        if (ownsPreviewPlayback)
            previewPlayer.StopPreview();
        previewPlayer.Play(mirrorClient.CreatePreviewUri(sets[index].Mirror, sets[index].Beatmaps[0].Id));
        previewingSetIndex = index;
        ownsPreviewPlayback = true;
        previewPlayCount++;
        lastPreviewStartedUtc = DateTime.UtcNow;
    }

    private float MaxScrollOffset(VirtualViewport viewport)
    {
        var columns = Math.Max(1, (int)MathF.Floor(viewport.VirtualWidth / (CardWidth + CardMargin * 2f)));
        var rows = (int)MathF.Ceiling(sets.Count / (float)columns);
        var contentHeight = rows * (CardHeight + CardMargin * 2f) + 32f * Dp;
        return Math.Max(0f, contentHeight - (viewport.VirtualHeight - BarHeight));
    }

    private BeatmapMirrorDefinition MirrorDefinition(BeatmapMirrorKind kind) => mirrorClient.Mirrors.First(m => m.Kind == kind);

    private static UiRect SearchBounds(VirtualViewport viewport)
    {
        var left = DroidUiMetrics.AppBarHeight + 6f * Dp;
        var right = viewport.VirtualWidth - 12f * Dp;
        var mirrorWidth = 150f * Dp;
        var filtersWidth = 112f * Dp;
        var searchTrailingWidth = 0f;
        var searchRight = right - mirrorWidth - filtersWidth - searchTrailingWidth - 18f * Dp;
        return new UiRect(left, 10f * Dp, Math.Max(200f * Dp, searchRight - left), 36f * Dp);
    }

    private static float MaxDropdownScroll(int optionCount, VirtualViewport viewport)
    {
        var rowHeight = 46f * Dp;
        var contentHeight = optionCount * rowHeight + 16f * Dp;
        var availableHeight = Math.Max(rowHeight, viewport.VirtualHeight - (BarHeight + 54f * Dp));
        return Math.Max(0f, contentHeight - availableHeight);
    }

    private static void AddCompoundButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action, UiMaterialIcon leadingIcon, UiMaterialIcon? trailingIcon, UiColor background, float radius)
    {
        elements.Add(Fill(id + "-hit", bounds, background, 1f, action, radius));
        var textWidth = EstimateTextWidth(text, 14f * Dp);
        var trailingWidth = trailingIcon is null ? 0f : 8f * Dp + 24f * Dp;
        var contentWidth = 24f * Dp + 8f * Dp + textWidth + trailingWidth;
        var x = bounds.X + (bounds.Width - contentWidth) / 2f;
        var iconY = bounds.Y + (bounds.Height - 24f * Dp) / 2f;
        elements.Add(MaterialIcon(id + "-icon", leadingIcon, new UiRect(x, iconY, 24f * Dp, 24f * Dp), White, 1f, action));
        elements.Add(TextMiddle(id + "-text", text, x + 32f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, White, UiTextAlignment.Left, action));
        if (trailingIcon is not null)
            elements.Add(MaterialIcon(id + "-trailing", trailingIcon.Value, new UiRect(x + 32f * Dp + textWidth + 8f * Dp, iconY, 24f * Dp, 24f * Dp), White, 1f, action));
    }

    private static void AddCompoundSpriteButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, string assetName, UiAction action, UiMaterialIcon trailingIcon)
    {
        elements.Add(Fill(id + "-hit", bounds, AppBar, 0f, action, 0f));
        var textWidth = EstimateTextWidth(text, 14f * Dp);
        var contentWidth = 24f * Dp + 8f * Dp + textWidth + 8f * Dp + 24f * Dp;
        var x = bounds.X + (bounds.Width - contentWidth) / 2f;
        var iconY = bounds.Y + (bounds.Height - 24f * Dp) / 2f;
        elements.Add(new UiElementSnapshot(id + "-logo", UiElementKind.Sprite, new UiRect(x, iconY, 24f * Dp, 24f * Dp), White, 1f, assetName, action));
        elements.Add(TextMiddle(id + "-text", text, x + 32f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, White, UiTextAlignment.Left, action));
        elements.Add(MaterialIcon(id + "-caret", trailingIcon, new UiRect(x + 32f * Dp + textWidth + 8f * Dp, iconY, 24f * Dp, 24f * Dp), White, 1f, action));
    }

    private static void AddButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action)
    {
        elements.Add(Fill(id + "-bg", bounds, Field, 1f, action, Radius));
        elements.Add(TextMiddle(id + "-text", text, bounds.X + 4f * Dp, bounds.Y, bounds.Width - 8f * Dp, bounds.Height, MathF.Min(14f * Dp, bounds.Height * 0.45f), White, UiTextAlignment.Center, action));
    }

    private static void AddDropdownButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action)
    {
        elements.Add(Fill(id + "-bg", bounds, Field, 1f, action, Radius));
        var textWidth = MathF.Min(bounds.Width - 48f * Dp, MathF.Max(40f * Dp, EstimateTextWidth(text, 14f * Dp)));
        elements.Add(TextMiddle(id + "-text", text, bounds.X + (bounds.Width - textWidth) / 2f - 8f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, White, UiTextAlignment.Center, action));
        elements.Add(MaterialIcon(id + "-caret", UiMaterialIcon.ArrowDropDown, new UiRect(bounds.Right - 34f * Dp, bounds.Y + 9f * Dp, 24f * Dp, 24f * Dp), White, 1f, action));
    }

    private static void AddButtonGroup(List<UiElementSnapshot> elements, IReadOnlyList<DownloaderButtonSpec> buttons, float centerX, float y)
    {
        var totalWidth = buttons.Sum(button => button.Width) + Math.Max(0, buttons.Count - 1) * 8f * Dp;
        var x = centerX - totalWidth / 2f;
        foreach (var button in buttons)
        {
            AddIconButton(elements, button.Id, new UiRect(x, y, button.Width, 36f * Dp), button.Icon, button.Text, button.Action);
            x += button.Width + 8f * Dp;
        }
    }

    private static void AddIconButton(List<UiElementSnapshot> elements, string id, UiRect bounds, UiMaterialIcon icon, string text, UiAction action)
    {
        elements.Add(Fill(id + "-bg", bounds, Field, 1f, action, Radius));
        var textWidth = string.IsNullOrEmpty(text) ? 0f : MathF.Max(48f * Dp, text.Length * 7f * Dp);
        var contentWidth = 24f * Dp + (textWidth > 0f ? 6f * Dp + textWidth : 0f);
        var iconX = bounds.X + (bounds.Width - contentWidth) / 2f;
        elements.Add(MaterialIcon(id + "-icon", icon, new UiRect(iconX, bounds.Y + 6f * Dp, 24f * Dp, 24f * Dp), White, 1f, action));
        if (textWidth > 0f)
            elements.Add(TextMiddle(id + "-text", text, iconX + 30f * Dp, bounds.Y, textWidth, bounds.Height, 14f * Dp, White, UiTextAlignment.Left, action));
    }

    private static void AddStatusPill(List<UiElementSnapshot> elements, string id, BeatmapRankedStatus status, float x, float y, UiAction action)
    {
        var width = StatusPillWidth(status);
        var bounds = new UiRect(x, y, width, 20f * Dp);
        elements.Add(Fill(id + "-bg", bounds, StatusPillColor(status), 1f, action, Radius));
        elements.Add(TextMiddle(id, RankedStatusText(status), bounds.X + 12f * Dp, bounds.Y, bounds.Width - 24f * Dp, bounds.Height, 10f * Dp, White, UiTextAlignment.Center, action));
    }

    private static float StatusPillWidth(BeatmapRankedStatus status) => Math.Max(58f * Dp, EstimateTextWidth(RankedStatusText(status), 10f * Dp) + 24f * Dp);

    private static UiColor StatusPillColor(BeatmapRankedStatus status) => Panel;

    private static void AddDifficultyDots(
        List<UiElementSnapshot> elements,
        string id,
        IReadOnlyList<BeatmapMirrorBeatmap> beatmaps,
        float x,
        float y,
        float width,
        UiAction action,
        int? selectedIndex,
        bool isCentered,
        UiAction fallbackAction)
    {
        const string glyph = "⦿";
        const float glyphSize = 24f * Dp;
        const float cardCellWidth = 32f * Dp;
        const float detailsCellWidth = 56f * Dp;
        const float rowHeight = DifficultyGlyphRowHeight;
        const float margin = 6f * Dp;
        var count = Math.Min(beatmaps.Count, 16);
        var hasSelection = selectedIndex is not null;
        var cellWidth = hasSelection ? detailsCellWidth : cardCellWidth;
        var totalWidth = hasSelection
            ? count * cellWidth + Math.Max(0, count - 1) * margin
            : count * cellWidth;
        var startX = isCentered ? x + (width - totalWidth) / 2f : x;
        var cursorX = startX;
        for (var i = 0; i < count; i++)
        {
            var dotAction = hasSelection ? DifficultyAction(i) : fallbackAction;
            var selected = selectedIndex == i;
            var hitBounds = new UiRect(cursorX, y, cellWidth, rowHeight);
            if (selected)
                elements.Add(Fill($"{id}-{i}-selected", hitBounds, Field, 1f, dotAction, Radius));

            elements.Add(TextMiddle(
                $"{id}-{i}",
                glyph,
                hitBounds.X,
                hitBounds.Y,
                hitBounds.Width,
                hitBounds.Height,
                glyphSize,
                StarRatingColor(beatmaps[i].StarRating),
                UiTextAlignment.Center,
                dotAction));
            cursorX += cellWidth + (hasSelection ? margin : 0f);
        }
    }

    private static UiColor StarRatingColor(float starRating)
    {
        var points = new (float Rating, UiColor Color)[]
        {
            (0.1f, UiColor.Opaque(170, 170, 170)),
            (0.1f, UiColor.Opaque(66, 144, 251)),
            (1.25f, UiColor.Opaque(79, 192, 255)),
            (2.0f, UiColor.Opaque(79, 255, 213)),
            (2.5f, UiColor.Opaque(124, 255, 79)),
            (3.3f, UiColor.Opaque(246, 240, 92)),
            (4.2f, UiColor.Opaque(255, 128, 104)),
            (4.9f, UiColor.Opaque(255, 78, 111)),
            (5.8f, UiColor.Opaque(198, 69, 184)),
            (6.7f, UiColor.Opaque(101, 99, 222)),
            (7.7f, UiColor.Opaque(24, 21, 142)),
            (9.0f, UiColor.Opaque(0, 0, 0)),
        };
        var rounded = MathF.Ceiling(starRating * 100f) / 100f;
        if (rounded < 0.1f)
            return UiColor.Opaque(170, 170, 170);
        for (var i = 0; i < points.Length - 1; i++)
        {
            var current = points[i];
            var next = points[i + 1];
            if (rounded > next.Rating)
                continue;

            var amount = Math.Clamp((rounded - current.Rating) / Math.Max(0.001f, next.Rating - current.Rating), 0f, 1f);
            return new UiColor(
                (byte)MathF.Round(current.Color.Red + (next.Color.Red - current.Color.Red) * amount),
                (byte)MathF.Round(current.Color.Green + (next.Color.Green - current.Color.Green) * amount),
                (byte)MathF.Round(current.Color.Blue + (next.Color.Blue - current.Color.Blue) * amount),
                255);
        }
        return points[^1].Color;
    }

    private readonly record struct DownloaderButtonSpec(string Id, UiMaterialIcon Icon, string Text, float Width, UiAction Action);

    private static float EstimateTextWidth(string text, float size) => MathF.Max(1f, text.Length * size * 0.55f);

    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float radius = 0f) => new(id, UiElementKind.Fill, bounds, color, alpha, Action: action, CornerRadius: radius);

    private static UiElementSnapshot Text(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) => new(id, UiElementKind.Text, new UiRect(x, y, width, height), color, 1f, Action: action, Text: text, TextStyle: new UiTextStyle(size, false, alignment));

    private static UiElementSnapshot TextMiddle(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) => new(id, UiElementKind.Text, new UiRect(x, y, width, height), color, 1f, Action: action, Text: text, TextStyle: new UiTextStyle(size, false, alignment, false, UiTextVerticalAlignment.Middle));

    private static UiElementSnapshot MaterialIcon(string id, UiMaterialIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) => new(
        id,
        UiElementKind.MaterialIcon,
        bounds,
        color,
        alpha,
        Action: action,
        MaterialIcon: icon);

    private static string DifficultyDots(BeatmapMirrorSet set) => string.Join(' ', set.Beatmaps.Select(_ => "⦿"));

    private static string RankedStatusText(BeatmapRankedStatus rankedStatus) => rankedStatus switch
    {
        BeatmapRankedStatus.Ranked => "Ranked",
        BeatmapRankedStatus.Approved => "Approved",
        BeatmapRankedStatus.Qualified => "Qualified",
        BeatmapRankedStatus.Loved => "Loved",
        BeatmapRankedStatus.WorkInProgress => "WIP",
        BeatmapRankedStatus.Graveyard => "Graveyard",
        _ => "Pending",
    };

    private static string SortText(BeatmapMirrorSort value) => value switch
    {
        BeatmapMirrorSort.Bpm => "BPM",
        BeatmapMirrorSort.DifficultyRating => "Difficulty rating",
        BeatmapMirrorSort.HitLength => "Hit length",
        BeatmapMirrorSort.PassCount => "Pass count",
        BeatmapMirrorSort.PlayCount => "Play count",
        BeatmapMirrorSort.TotalLength => "Total length",
        BeatmapMirrorSort.FavouriteCount => "Favourite count",
        BeatmapMirrorSort.LastUpdated => "Last updated",
        BeatmapMirrorSort.RankedDate => "Ranked date",
        BeatmapMirrorSort.SubmittedDate => "Submitted date",
        _ => value.ToString(),
    };

    private static BeatmapMirrorSort Next(BeatmapMirrorSort value)
    {
        var values = Enum.GetValues<BeatmapMirrorSort>();
        return values[(Array.IndexOf(values, value) + 1) % values.Length];
    }

    private static string FormatDownloadInfo(BeatmapDownloadProgress? progress)
    {
        if (progress is null)
            return string.Empty;

        var percent = progress.Percent is double p ? $" ({p:0}%)" : string.Empty;
        var speed = progress.SpeedBytesPerSecond > 0 ? $"\n{progress.SpeedBytesPerSecond / 1024d / 1024d:0.###} mb/s{percent}" : percent;
        return speed;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception)
        {
        }
    }

    public static int DownloadIndex(UiAction action) => action switch
    {
        UiAction.DownloaderDownloadFirst or UiAction.DownloaderDownloadFirstNoVideo or UiAction.DownloaderDownload0 or UiAction.DownloaderDownloadNoVideo0 => 0,
        UiAction.DownloaderDownload1 or UiAction.DownloaderDownloadNoVideo1 => 1,
        UiAction.DownloaderDownload2 or UiAction.DownloaderDownloadNoVideo2 => 2,
        UiAction.DownloaderDownload3 or UiAction.DownloaderDownloadNoVideo3 => 3,
        UiAction.DownloaderDownload4 or UiAction.DownloaderDownloadNoVideo4 => 4,
        UiAction.DownloaderDownload5 or UiAction.DownloaderDownloadNoVideo5 => 5,
        UiAction.DownloaderDownload6 or UiAction.DownloaderDownloadNoVideo6 => 6,
        UiAction.DownloaderDownload7 or UiAction.DownloaderDownloadNoVideo7 => 7,
        _ => -1,
    };

    public static bool IsNoVideoAction(UiAction action) => action is
        UiAction.DownloaderDownloadFirstNoVideo or UiAction.DownloaderDownloadNoVideo0 or UiAction.DownloaderDownloadNoVideo1 or UiAction.DownloaderDownloadNoVideo2 or UiAction.DownloaderDownloadNoVideo3 or
        UiAction.DownloaderDownloadNoVideo4 or UiAction.DownloaderDownloadNoVideo5 or UiAction.DownloaderDownloadNoVideo6 or UiAction.DownloaderDownloadNoVideo7;

    public static int CardIndex(UiAction action) => action switch
    {
        UiAction.DownloaderCard0 => 0,
        UiAction.DownloaderCard1 => 1,
        UiAction.DownloaderCard2 => 2,
        UiAction.DownloaderCard3 => 3,
        UiAction.DownloaderCard4 => 4,
        UiAction.DownloaderCard5 => 5,
        UiAction.DownloaderCard6 => 6,
        UiAction.DownloaderCard7 => 7,
        _ => -1,
    };

    public static int PreviewIndex(UiAction action) => action switch
    {
        UiAction.DownloaderPreview0 => 0,
        UiAction.DownloaderPreview1 => 1,
        UiAction.DownloaderPreview2 => 2,
        UiAction.DownloaderPreview3 => 3,
        UiAction.DownloaderPreview4 => 4,
        UiAction.DownloaderPreview5 => 5,
        UiAction.DownloaderPreview6 => 6,
        UiAction.DownloaderPreview7 => 7,
        _ => -1,
    };

    public static int DifficultyIndex(UiAction action) => action switch
    {
        UiAction.DownloaderDetailsDifficulty0 => 0,
        UiAction.DownloaderDetailsDifficulty1 => 1,
        UiAction.DownloaderDetailsDifficulty2 => 2,
        UiAction.DownloaderDetailsDifficulty3 => 3,
        UiAction.DownloaderDetailsDifficulty4 => 4,
        UiAction.DownloaderDetailsDifficulty5 => 5,
        UiAction.DownloaderDetailsDifficulty6 => 6,
        UiAction.DownloaderDetailsDifficulty7 => 7,
        UiAction.DownloaderDetailsDifficulty8 => 8,
        UiAction.DownloaderDetailsDifficulty9 => 9,
        UiAction.DownloaderDetailsDifficulty10 => 10,
        UiAction.DownloaderDetailsDifficulty11 => 11,
        UiAction.DownloaderDetailsDifficulty12 => 12,
        UiAction.DownloaderDetailsDifficulty13 => 13,
        UiAction.DownloaderDetailsDifficulty14 => 14,
        UiAction.DownloaderDetailsDifficulty15 => 15,
        _ => -1,
    };

    private static UiAction CardAction(int index) => index switch
    {
        0 => UiAction.DownloaderCard0,
        1 => UiAction.DownloaderCard1,
        2 => UiAction.DownloaderCard2,
        3 => UiAction.DownloaderCard3,
        4 => UiAction.DownloaderCard4,
        5 => UiAction.DownloaderCard5,
        6 => UiAction.DownloaderCard6,
        7 => UiAction.DownloaderCard7,
        _ => UiAction.None,
    };

    private static UiAction PreviewAction(int index) => index switch
    {
        0 => UiAction.DownloaderPreview0,
        1 => UiAction.DownloaderPreview1,
        2 => UiAction.DownloaderPreview2,
        3 => UiAction.DownloaderPreview3,
        4 => UiAction.DownloaderPreview4,
        5 => UiAction.DownloaderPreview5,
        6 => UiAction.DownloaderPreview6,
        7 => UiAction.DownloaderPreview7,
        _ => UiAction.None,
    };

    private static UiAction DownloadAction(int index) => index switch
    {
        0 => UiAction.DownloaderDownload0,
        1 => UiAction.DownloaderDownload1,
        2 => UiAction.DownloaderDownload2,
        3 => UiAction.DownloaderDownload3,
        4 => UiAction.DownloaderDownload4,
        5 => UiAction.DownloaderDownload5,
        6 => UiAction.DownloaderDownload6,
        7 => UiAction.DownloaderDownload7,
        _ => UiAction.None,
    };

    private static UiAction NoVideoAction(int index) => index switch
    {
        0 => UiAction.DownloaderDownloadNoVideo0,
        1 => UiAction.DownloaderDownloadNoVideo1,
        2 => UiAction.DownloaderDownloadNoVideo2,
        3 => UiAction.DownloaderDownloadNoVideo3,
        4 => UiAction.DownloaderDownloadNoVideo4,
        5 => UiAction.DownloaderDownloadNoVideo5,
        6 => UiAction.DownloaderDownloadNoVideo6,
        7 => UiAction.DownloaderDownloadNoVideo7,
        _ => UiAction.None,
    };

    private static UiAction DifficultyAction(int index) => index switch
    {
        0 => UiAction.DownloaderDetailsDifficulty0,
        1 => UiAction.DownloaderDetailsDifficulty1,
        2 => UiAction.DownloaderDetailsDifficulty2,
        3 => UiAction.DownloaderDetailsDifficulty3,
        4 => UiAction.DownloaderDetailsDifficulty4,
        5 => UiAction.DownloaderDetailsDifficulty5,
        6 => UiAction.DownloaderDetailsDifficulty6,
        7 => UiAction.DownloaderDetailsDifficulty7,
        8 => UiAction.DownloaderDetailsDifficulty8,
        9 => UiAction.DownloaderDetailsDifficulty9,
        10 => UiAction.DownloaderDetailsDifficulty10,
        11 => UiAction.DownloaderDetailsDifficulty11,
        12 => UiAction.DownloaderDetailsDifficulty12,
        13 => UiAction.DownloaderDetailsDifficulty13,
        14 => UiAction.DownloaderDetailsDifficulty14,
        15 => UiAction.DownloaderDetailsDifficulty15,
        _ => UiAction.None,
    };
}
