using System.Collections.Concurrent;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.UI.Frames;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Input;
using OsuDroid.Game.UI.Scrolling;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

internal enum BeatmapDownloaderScrollTarget
{
    Results,
    SortDropdown,
    StatusDropdown,
}

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

    private static readonly UiColor s_background = DroidUiColors.Surface;
    private static readonly UiColor s_appBar = DroidUiColors.SurfaceAppBar;
    private static readonly UiColor s_panel = DroidUiColors.SurfaceRow;
    private static readonly UiColor s_footer = DroidUiColors.SurfaceAppBar;
    private static readonly UiColor s_field = DroidUiColors.SurfaceInput;
    private static readonly UiColor s_accent = DroidUiColors.Accent;
    private static readonly UiColor s_white = DroidUiColors.TextPrimary;
    private static readonly UiColor s_secondary = DroidUiColors.TextSecondary;
    private static readonly UiColor s_muted = DroidUiColors.MutedText;
    private static readonly UiColor s_filterLabel = DroidUiColors.FilterLabel;
    private static readonly UiColor s_dropdownSelected = DroidUiColors.DropdownSelected;
    private static readonly UiColor s_coverFallback = DroidUiColors.CoverFallback;
    private static readonly UiColor s_filterPanel = DroidUiColors.FilterPanel;
    private static readonly UiColor s_dropdownPanel = DroidUiColors.DropdownPanel;
    private static readonly UiColor s_dialogScrim = DroidUiColors.ModalShade;
    private static readonly UiColor s_modalShade = DroidUiColors.ModalShadeStrong;

    private static readonly HttpClient s_coverClient = new();

    private readonly IBeatmapMirrorClient _mirrorClient;
    private readonly IBeatmapDownloadService _downloadService;
    private readonly GameLocalizer _localizer;
    private IBeatmapPreviewPlayer _previewPlayer;
    private readonly string _coverCacheDirectory;
    private readonly string? _downloadTracePath;
    private readonly ConcurrentQueue<BeatmapDownloadCompletion> _downloadCompletions = new();
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
    private double _elapsedSeconds;
    private BeatmapDownloaderScrollTarget? _activeScrollTarget;
    private readonly KineticScrollState _resultsScroll = new(KineticScrollAxis.Vertical);
    private readonly KineticScrollState _sortDropdownKineticScroll = new(
        KineticScrollAxis.Vertical
    );
    private readonly KineticScrollState _statusDropdownKineticScroll = new(
        KineticScrollAxis.Vertical
    );
    private string _query = string.Empty;
    private string? _message;
    private string? _lastImportedSetDirectory;
    private float _loadingIndicatorRotationDegrees;
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
        GameLocalizer? _localizer = null,
        string? _downloadTracePath = null
    )
    {
        this._mirrorClient = _mirrorClient;
        this._downloadService = _downloadService;
        this._textInputService = _textInputService;
        this._previewPlayer = _previewPlayer;
        this._coverCacheDirectory = _coverCacheDirectory;
        this._downloadTracePath = _downloadTracePath;
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

    public void SetPreferNoVideoDownloads(bool preferNoVideo) =>
        _preferNoVideoDownloads = preferNoVideo;

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

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) =>
        new(
            "BeatmapDownloader",
            "Beatmap Downloader",
            string.Empty,
            Array.Empty<string>(),
            0,
            false,
            CreateFrame(viewport)
        );

    public void Update(TimeSpan elapsed)
    {
        float elapsedSeconds = (float)elapsed.TotalSeconds;
        _elapsedSeconds += elapsedSeconds;
        _resultsScroll.Update(
            elapsedSeconds,
            () => _scrollOffset,
            value => _scrollOffset = value,
            0f,
            MaxScrollOffset(VirtualViewport.AndroidReferenceLandscape)
        );
        _sortDropdownKineticScroll.Update(
            elapsedSeconds,
            () => _sortDropdownScroll,
            value => _sortDropdownScroll = value,
            0f,
            MaxDropdownScroll(SortOptions().Length, VirtualViewport.AndroidReferenceLandscape)
        );
        _statusDropdownKineticScroll.Update(
            elapsedSeconds,
            () => _statusDropdownScroll,
            value => _statusDropdownScroll = value,
            0f,
            MaxDropdownScroll(StatusOptions().Length, VirtualViewport.AndroidReferenceLandscape)
        );
        float maxScroll = MaxScrollOffset(VirtualViewport.AndroidReferenceLandscape);
        if (maxScroll > 0f && _hasMore && !_isSearching && _scrollOffset >= maxScroll - 40f * Dp)
        {
            _ = SearchAsync(true);
        }

        _loadingIndicatorRotationDegrees -= (float)(360d * elapsed.TotalSeconds);
        if (_loadingIndicatorRotationDegrees <= -360f)
        {
            _loadingIndicatorRotationDegrees %= 360f;
        }

        ApplyQueuedDownloadCompletions();
    }

    private string DisplayTitle(BeatmapMirrorSet set) =>
        _forceRomanizedMetadata || string.IsNullOrWhiteSpace(set.TitleUnicode)
            ? set.Title
            : set.TitleUnicode;

    private string DisplayArtist(BeatmapMirrorSet set) =>
        _forceRomanizedMetadata || string.IsNullOrWhiteSpace(set.ArtistUnicode)
            ? set.Artist
            : set.ArtistUnicode;

    private sealed record BeatmapDownloadCompletion(
        bool IsSuccess,
        string? ArchivePath,
        string? ErrorMessage
    );
}
