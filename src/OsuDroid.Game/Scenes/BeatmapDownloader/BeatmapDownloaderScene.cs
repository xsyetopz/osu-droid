using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

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

    private static readonly UiColor s_background = UiColor.Opaque(19, 19, 26);
    private static readonly UiColor s_appBar = UiColor.Opaque(30, 30, 46);
    private static readonly UiColor s_panel = UiColor.Opaque(22, 22, 34);
    private static readonly UiColor s_footer = UiColor.Opaque(30, 30, 46);
    private static readonly UiColor s_field = UiColor.Opaque(54, 54, 83);
    private static readonly UiColor s_accent = UiColor.Opaque(243, 115, 115);
    private static readonly UiColor s_white = UiColor.Opaque(255, 255, 255);
    private static readonly UiColor s_secondary = UiColor.Opaque(178, 178, 204);
    private static readonly UiColor s_muted = UiColor.Opaque(130, 130, 168);
    private static readonly UiColor s_coverFallback = UiColor.Opaque(75, 75, 128);
    private static readonly UiColor s_filterPanel = UiColor.Opaque(33, 33, 51);
    private static readonly UiColor s_modalShade = new(19, 19, 26, 190);

    private static readonly HttpClient s_coverClient = new();

    private readonly IBeatmapMirrorClient _mirrorClient;
    private readonly IBeatmapDownloadService _downloadService;
    private readonly GameLocalizer _localizer;
    private IBeatmapPreviewPlayer _previewPlayer;
    private readonly string _coverCacheDirectory;
    private ITextInputService _textInputService;
    private CancellationTokenSource _searchCancellation = new();
    private IReadOnlyList<BeatmapMirrorSet> _sets = [];
    private readonly HashSet<string> _coverDownloads = new(StringComparer.Ordinal);
    private bool _isSearching;
    private bool _hasMore = true;
    private bool _hasSearchError;
    private bool _isSearchFocused;
    private bool _filtersOpen;
    private bool _sortDropdownOpen;
    private bool _statusDropdownOpen;
    private bool _mirrorsOpen;
    private float _sortDropdownScroll;
    private float _statusDropdownScroll;
    private int _previewPlayCount;
    private DateTime _lastPreviewStartedUtc;
    private int? _previewingSetIndex;
    private bool _ownsPreviewPlayback;
    private int _offset;
    private int _visibleStartIndex;
    private int? _selectedSetIndex;
    private int _selectedDifficultyIndex;
    private bool _preferNoVideoDownloads;
    private bool _forceRomanizedMetadata;
    private float _scrollOffset;
    private string _query = string.Empty;
    private string? _message;
    private string? _lastImportedSetDirectory;
    private BeatmapMirrorKind _mirror = BeatmapMirrorKind.OsuDirect;
    private BeatmapMirrorSort _sort = BeatmapMirrorSort.RankedDate;
    private BeatmapMirrorOrder _order = BeatmapMirrorOrder.Descending;
    private BeatmapRankedStatus? _status;

    public BeatmapDownloaderScene(
        IBeatmapMirrorClient _mirrorClient,
        IBeatmapDownloadService _downloadService,
        ITextInputService _textInputService,
        IBeatmapPreviewPlayer _previewPlayer,
        string _coverCacheDirectory,
        GameLocalizer? _localizer = null)
    {
        this._mirrorClient = _mirrorClient;
        this._downloadService = _downloadService;
        this._textInputService = _textInputService;
        this._previewPlayer = _previewPlayer;
        this._coverCacheDirectory = _coverCacheDirectory;
        this._localizer = _localizer ?? new GameLocalizer();
        _message = this._localizer["BeatmapDownloader_SearchInitial"];
        Directory.CreateDirectory(_coverCacheDirectory);
    }

    public string Query => _query;

    public BeatmapMirrorKind Mirror => _mirror;

    public string? ConsumeLastImportedSetDirectoryNotification()
    {
        string? directory = _lastImportedSetDirectory;
        _lastImportedSetDirectory = null;
        return directory;
    }

    public void Enter()
    {
        if (_sets.Count == 0 && !_isSearching)
        {
            _ = SearchAsync(false);
        }
    }

    public void Leave()
    {
        HideSearchInput();
        if (_ownsPreviewPlayback)
        {
            _previewPlayer.StopPreview();
        }

        _previewingSetIndex = null;
        _ownsPreviewPlayback = false;
        _searchCancellation.Cancel();
        _filtersOpen = false;
        _sortDropdownOpen = false;
        _statusDropdownOpen = false;
        _mirrorsOpen = false;
        _selectedSetIndex = null;
    }

    public void SetTextInputService(ITextInputService service) => _textInputService = service;

    public void SetPreferNoVideoDownloads(bool preferNoVideo) => _preferNoVideoDownloads = preferNoVideo;

    public void SetForceRomanized(bool forceRomanized) => _forceRomanizedMetadata = forceRomanized;

    public void SetPreviewPlayer(IBeatmapPreviewPlayer player)
    {
        if (ReferenceEquals(_previewPlayer, player))
        {
            return;
        }

        if (_ownsPreviewPlayback)
        {
            _previewPlayer.StopPreview();
        }

        _previewPlayer = player;
        _previewingSetIndex = null;
        _ownsPreviewPlayback = false;
    }


    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new("BeatmapDownloader", "Beatmap Downloader", string.Empty, Array.Empty<string>(), 0, false, CreateFrame(viewport));

    private string DisplayTitle(BeatmapMirrorSet set) => _forceRomanizedMetadata || string.IsNullOrWhiteSpace(set.TitleUnicode) ? set.Title : set.TitleUnicode;

    private string DisplayArtist(BeatmapMirrorSet set) => _forceRomanizedMetadata || string.IsNullOrWhiteSpace(set.ArtistUnicode) ? set.Artist : set.ArtistUnicode;









}
