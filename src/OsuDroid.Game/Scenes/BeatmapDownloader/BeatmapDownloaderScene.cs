using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class BeatmapDownloaderScene
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
    private string? lastImportedSetDirectory;
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

    public string? ConsumeLastImportedSetDirectoryNotification()
    {
        var directory = lastImportedSetDirectory;
        lastImportedSetDirectory = null;
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


    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new("BeatmapDownloader", "Beatmap Downloader", string.Empty, Array.Empty<string>(), 0, false, CreateFrame(viewport));









}
