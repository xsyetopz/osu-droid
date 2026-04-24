using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.SongSelect;

internal enum SongSelectSortMode
{
    Title,
    Artist,
    Creator,
    Date,
    Bpm,
    DroidStars,
    StandardStars,
    Length,
}

public sealed partial class SongSelectScene(IBeatmapLibrary library, IMenuMusicController musicController, IBeatmapDifficultyService difficultyService, string songsPath, OnlineProfilePanelState? onlinePanelState = null, ITextInputService? textInputService = null, Func<int, int>? randomIndexProvider = null, GameLocalizer? localizer = null)
{
    private const float RowWidth = 724f;
    private const float RowHeight = 127f;
    private const float CollapsedRowHeight = 97f;
    private const float RowSpacing = 92f;
    private const float ExpandedRowSpacing = RowHeight - 25f;
    private const float RowBaseY = 300f;
    private const float BackgroundLuminancePerSecond = 1f;
    private const float TopPanelHeight = 184f;
    private const float SongSelectTopX = -1640f;
    private const float SongSelectTopWidth = 5259.1304f;
    private const float BackButtonSize = 187.5f;
    private const float SmallButtonSize = 111f;
    private const float OnlinePanelWidth = 410f;
    private const float OnlinePanelHeight = 110f;
    private const float OnlineAvatarFooterSize = 110f;
    private const float OnlinePanelGap = 20f;
    private const float ScrollTouchMinimumXRatio = 0.4f;
    private const float Dp = DroidUiMetrics.DpScale;
    private const float PropertiesWidth = 320f * Dp;
    private const float PropertiesRowHeight = 52f * Dp;
    private const float BeatmapOptionsWidth = 1160f * Dp;
    private const float BeatmapOptionsSearchHeight = 56f * Dp;
    private const float BeatmapOptionsRowHeight = 50f * Dp;
    private const float BeatmapOptionsRadius = 14f * Dp;
    private const float BeatmapOptionsDividerWidth = 1f * Dp;
    private const float BeatmapOptionsHorizontalPadding = 16f * Dp;
    private const float BeatmapOptionsFolderEndPadding = 24f * Dp;
    private const float BeatmapOptionsDrawableGap = 12f * Dp;
    private const float BeatmapOptionsIconSize = 24f * Dp;
    private const float BeatmapOptionsTextSize = 14f * Dp;
    private const float BeatmapOptionsTextWidthFactor = 0.62f;
    private const float CollectionsWidth = 500f * Dp;
    private const float CollectionsMargin = 20f * Dp;
    private const float CollectionRowHeight = 60f * Dp;
    private const int VisibleSetSlots = 8;
    private const int VisibleDifficultySlots = 16;
    private const int VisibleCollectionSlots = 8;

    private static readonly UiColor s_white = UiColor.Opaque(255, 255, 255);
    private static readonly UiColor s_black = UiColor.Opaque(0, 0, 0);
    private static readonly UiColor s_backgroundShade = new(0, 0, 0, 132);
    private static readonly UiColor s_setRowTint = UiColor.Opaque(240, 150, 0);
    private static readonly UiColor s_difficultyRowTint = UiColor.Opaque(25, 25, 240);
    private static readonly UiColor s_selectedRowTint = s_white;
    private static readonly UiColor s_onlinePanelTint = UiColor.Opaque(51, 51, 51);
    private static readonly UiColor s_modalShade = new(0, 0, 0, 128);
    private static readonly UiColor s_propertiesPanel = UiColor.Opaque(30, 30, 46);
    private static readonly UiColor s_propertiesPanelDark = UiColor.Opaque(19, 19, 26);
    private static readonly UiColor s_collectionsPanelDark = UiColor.Opaque(19, 19, 26);
    private static readonly UiColor s_propertiesDivider = new(19, 19, 26, 115);
    private static readonly UiColor s_propertiesSecondary = UiColor.Opaque(130, 130, 168);
    private static readonly UiColor s_propertiesDanger = UiColor.Opaque(255, 191, 191);
    private static readonly UiColor s_beatmapOptionsSearchPanel = UiColor.Opaque(54, 54, 83);
    private static readonly UiColor s_beatmapOptionsDivider = new(255, 255, 255, 10);
    private static readonly UiColor s_beatmapOptionsAccent = UiColor.Opaque(243, 115, 115);
    private static readonly UiColor s_beatmapOptionsInactiveCheckbox = UiColor.Opaque(54, 54, 83);

    private readonly int[] _visibleSetIndices = Enumerable.Repeat(-1, VisibleSetSlots).ToArray();
    private readonly int[] _visibleDifficultyIndices = Enumerable.Repeat(-1, VisibleDifficultySlots).ToArray();
    private readonly int[] _visibleCollectionIndices = Enumerable.Repeat(-1, VisibleCollectionSlots).ToArray();
    private readonly SelectionState _selectionState = new();
    private readonly QueryState _queryState = new();
    private readonly BackgroundState _backgroundState = new();
    private OnlineProfilePanelState? _onlinePanelState = onlinePanelState;
    private readonly Func<int, int> _randomIndexProvider = randomIndexProvider ?? Random.Shared.Next;
    private readonly GameLocalizer _localizer = localizer ?? new GameLocalizer();
    private readonly object _difficultyGate = new();
    private readonly HashSet<string> _pendingDifficultyKeys = new(StringComparer.Ordinal);
    private readonly Queue<BeatmapInfo> _completedDifficultyUpdates = new();
    private readonly object _libraryRefreshGate = new();
    private ITextInputService _textInputService = textInputService ?? new NoOpTextInputService();

    private BeatmapLibrarySnapshot _snapshot = BeatmapLibrarySnapshot.Empty;
    private BeatmapLibrarySnapshot _visibleSnapshot = BeatmapLibrarySnapshot.Empty;
    private Task? _libraryRefreshTask;
    private BeatmapLibrarySnapshot? _completedLibraryRefresh;
    private bool _propertiesOpen;
    private bool _beatmapOptionsOpen;
    private bool _collectionsOpen;
    private bool _collectionsFilterMode;
    private bool _deleteBeatmapConfirmOpen;
    private string? _collectionPendingDelete;
    private bool _forceRomanizedMetadata;
    private DifficultyAlgorithm _displayAlgorithm = difficultyService.Algorithm;

    private int selectedSetIndex
    {
        get => _selectionState.SetIndex;
        set => _selectionState.SetIndex = value;
    }

    private int selectedDifficultyIndex
    {
        get => _selectionState.DifficultyIndex;
        set => _selectionState.DifficultyIndex = value;
    }

    private float scrollY
    {
        get => _selectionState.ScrollY;
        set => _selectionState.ScrollY = value;
    }

    private float collectionScrollY
    {
        get => _selectionState.CollectionScrollY;
        set => _selectionState.CollectionScrollY = value;
    }

    private float selectedSetExpansion
    {
        get => _selectionState.SetExpansion;
        set => _selectionState.SetExpansion = value;
    }

    private string searchQuery
    {
        get => _queryState.SearchQuery;
        set => _queryState.SearchQuery = value;
    }

    private bool favoriteOnlyFilter
    {
        get => _queryState.FavoriteOnlyFilter;
        set => _queryState.FavoriteOnlyFilter = value;
    }

    private string? collectionFilter
    {
        get => _queryState.CollectionFilter;
        set => _queryState.CollectionFilter = value;
    }

    private SongSelectSortMode sortMode
    {
        get => _queryState.SortMode;
        set => _queryState.SortMode = value;
    }

    private string? selectedBackgroundPath
    {
        get => _backgroundState.Path;
        set => _backgroundState.Path = value;
    }

    private string? selectedBackgroundBeatmapKey
    {
        get => _backgroundState.BeatmapKey;
        set => _backgroundState.BeatmapKey = value;
    }

    private float selectedBackgroundLuminance
    {
        get => _backgroundState.Luminance;
        set => _backgroundState.Luminance = Math.Clamp(value, 0f, 1f);
    }

    public BeatmapInfo? SelectedBeatmap => SelectedSet?.Beatmaps.Count > 0
        ? SelectedSet.Beatmaps[Math.Clamp(selectedDifficultyIndex, 0, SelectedSet.Beatmaps.Count - 1)]
        : null;

    public string? SelectedBackgroundPath => selectedBackgroundPath;

    private BeatmapSetInfo? SelectedSet => selectedSetIndex >= 0 && selectedSetIndex < _visibleSnapshot.Sets.Count ? _visibleSnapshot.Sets[selectedSetIndex] : null;

    public void SetPreviewPlayer(IBeatmapPreviewPlayer player) => musicController.SetPreviewPlayer(player);

    public void SetTextInputService(ITextInputService service) => _textInputService = service;

    public void SetOnlinePanelState(OnlineProfilePanelState? state) => _onlinePanelState = state;

    public void SetDisplayAlgorithm(DifficultyAlgorithm algorithm)
    {
        if (_displayAlgorithm == algorithm)
        {
            return;
        }

        BeatmapInfo? selected = SelectedBeatmap;
        _displayAlgorithm = algorithm;
        _visibleSnapshot = SortDifficultyRows(_visibleSnapshot);
        RestoreSelectedDifficulty(selected);
        scrollY = ClampScroll(CalculateSelectedSetScroll(selectedSetIndex));
        QueueVisibleDifficultyCalculations();
    }

    public void SetForceRomanized(bool forceRomanized)
    {
        if (_forceRomanizedMetadata == forceRomanized)
        {
            return;
        }

        _forceRomanizedMetadata = forceRomanized;
    }

    public void Enter(string? preferredSetDirectory = null, string? preferredBeatmapFilename = null)
    {
        long start = PerfDiagnostics.Start();
        _snapshot = library.Snapshot;
        if (_snapshot.Sets.Count == 0)
        {
            _snapshot = library.Load();
        }

        if (_snapshot.Sets.Count == 0 || library.NeedsScanRefresh())
        {
            StartBackgroundLibraryRefresh();
        }

        ApplyBeatmapOptions(preferredSetDirectory, preferredBeatmapFilename, queueDifficultyCalculations: false);
        selectedSetExpansion = 1f;
        scrollY = ClampScroll(CalculateSelectedSetScroll(selectedSetIndex));
        RefreshSelectedBackgroundPath();
        QueueVisibleDifficultyCalculations();
        PlaySelectedPreview();
        PerfDiagnostics.Log("songSelect.enter", start, $"sets={_visibleSnapshot.Sets.Count} selectedSet={selectedSetIndex}");
    }

    public void PrepareForWarmup()
    {
        if (_visibleSnapshot.Sets.Count > 0)
        {
            return;
        }

        _snapshot = library.Snapshot;
        if (_snapshot.Sets.Count == 0)
        {
            _snapshot = library.Load();
        }

        ApplyBeatmapOptions(queueDifficultyCalculations: false);
        RefreshSelectedBackgroundPath();
    }

    public void Leave()
    {
        selectedSetExpansion = 1f;
        ClosePopups();
    }

    public void Update(TimeSpan elapsed)
    {
        float elapsedSeconds = (float)elapsed.TotalSeconds;
        selectedSetExpansion = Math.Clamp(selectedSetExpansion + elapsedSeconds * 2f, 0f, 1f);
        selectedBackgroundLuminance += elapsedSeconds * BackgroundLuminancePerSecond;
        ApplyCompletedLibraryRefresh();
        ApplyCompletedDifficultyUpdates();
    }

    public void Scroll(float deltaY, UiPoint point, VirtualViewport viewport)
    {
        if (_collectionsOpen)
        {
            collectionScrollY = Math.Clamp(collectionScrollY + deltaY, 0f, MaxCollectionScroll(viewport));
            return;
        }

        if (_propertiesOpen || _beatmapOptionsOpen)
        {
            return;
        }

        if (point.X < viewport.VirtualWidth * ScrollTouchMinimumXRatio)
        {
            return;
        }

        Scroll(deltaY, viewport);
    }

    public void Scroll(float deltaY, VirtualViewport viewport)
    {
        if (_collectionsOpen)
        {
            collectionScrollY = Math.Clamp(collectionScrollY + deltaY, 0f, MaxCollectionScroll(viewport));
            return;
        }

        if (_propertiesOpen || _beatmapOptionsOpen)
        {
            return;
        }

        scrollY = ClampScroll(scrollY + deltaY);
    }

}
