using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

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

public sealed partial class SongSelectScene(IBeatmapLibrary library, IMenuMusicController musicController, IBeatmapDifficultyService difficultyService, string songsPath, OnlineProfileSnapshot? profile = null, ITextInputService? textInputService = null, Func<int, int>? randomIndexProvider = null)
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
    private const string DefaultFavoriteFolderName = "Default";
    private const string CreateFavoriteFolderLabel = "Create new folder";
    private const float CollectionsWidth = 500f * Dp;
    private const float CollectionsMargin = 20f * Dp;
    private const float CollectionRowHeight = 60f * Dp;
    private const int VisibleSetSlots = 8;
    private const int VisibleDifficultySlots = 16;
    private const int VisibleCollectionSlots = 8;

    private static readonly UiColor White = UiColor.Opaque(255, 255, 255);
    private static readonly UiColor Black = UiColor.Opaque(0, 0, 0);
    private static readonly UiColor BackgroundShade = new(0, 0, 0, 132);
    private static readonly UiColor SetRowTint = UiColor.Opaque(240, 150, 0);
    private static readonly UiColor DifficultyRowTint = UiColor.Opaque(25, 25, 240);
    private static readonly UiColor SelectedRowTint = White;
    private static readonly UiColor OnlinePanelTint = UiColor.Opaque(51, 51, 51);
    private static readonly UiColor ModalShade = new(0, 0, 0, 128);
    private static readonly UiColor PropertiesPanel = UiColor.Opaque(30, 30, 46);
    private static readonly UiColor PropertiesPanelDark = UiColor.Opaque(19, 19, 26);
    private static readonly UiColor CollectionsPanelDark = UiColor.Opaque(19, 19, 26);
    private static readonly UiColor PropertiesDivider = new(19, 19, 26, 115);
    private static readonly UiColor PropertiesSecondary = UiColor.Opaque(130, 130, 168);
    private static readonly UiColor PropertiesDanger = UiColor.Opaque(255, 191, 191);
    private static readonly UiColor BeatmapOptionsSearchPanel = UiColor.Opaque(54, 54, 83);
    private static readonly UiColor BeatmapOptionsDivider = new(255, 255, 255, 10);
    private static readonly UiColor BeatmapOptionsAccent = UiColor.Opaque(243, 115, 115);
    private static readonly UiColor BeatmapOptionsInactiveCheckbox = UiColor.Opaque(54, 54, 83);

    private readonly int[] visibleSetIndices = Enumerable.Repeat(-1, VisibleSetSlots).ToArray();
    private readonly int[] visibleDifficultyIndices = Enumerable.Repeat(-1, VisibleDifficultySlots).ToArray();
    private readonly int[] visibleCollectionIndices = Enumerable.Repeat(-1, VisibleCollectionSlots).ToArray();
    private readonly SelectionState selectionState = new();
    private readonly QueryState queryState = new();
    private readonly BackgroundState backgroundState = new();
    private readonly OnlineProfileSnapshot profile = profile ?? OnlineProfileSnapshot.Guest;
    private readonly Func<int, int> randomIndexProvider = randomIndexProvider ?? Random.Shared.Next;
    private readonly object difficultyGate = new();
    private readonly HashSet<string> pendingDifficultyKeys = new(StringComparer.Ordinal);
    private readonly Queue<BeatmapInfo> completedDifficultyUpdates = new();
    private readonly object libraryRefreshGate = new();
    private ITextInputService textInputService = textInputService ?? new NoOpTextInputService();

    private BeatmapLibrarySnapshot snapshot = BeatmapLibrarySnapshot.Empty;
    private BeatmapLibrarySnapshot visibleSnapshot = BeatmapLibrarySnapshot.Empty;
    private Task? libraryRefreshTask;
    private BeatmapLibrarySnapshot? completedLibraryRefresh;
    private bool propertiesOpen;
    private bool beatmapOptionsOpen;
    private bool collectionsOpen;
    private bool collectionsFilterMode;
    private bool deleteBeatmapConfirmOpen;
    private string? collectionPendingDelete;
    private DifficultyAlgorithm displayAlgorithm = difficultyService.Algorithm;

    private int selectedSetIndex
    {
        get => selectionState.SetIndex;
        set => selectionState.SetIndex = value;
    }

    private int selectedDifficultyIndex
    {
        get => selectionState.DifficultyIndex;
        set => selectionState.DifficultyIndex = value;
    }

    private float scrollY
    {
        get => selectionState.ScrollY;
        set => selectionState.ScrollY = value;
    }

    private float collectionScrollY
    {
        get => selectionState.CollectionScrollY;
        set => selectionState.CollectionScrollY = value;
    }

    private float selectedSetExpansion
    {
        get => selectionState.SetExpansion;
        set => selectionState.SetExpansion = value;
    }

    private string searchQuery
    {
        get => queryState.SearchQuery;
        set => queryState.SearchQuery = value;
    }

    private bool favoriteOnlyFilter
    {
        get => queryState.FavoriteOnlyFilter;
        set => queryState.FavoriteOnlyFilter = value;
    }

    private string? collectionFilter
    {
        get => queryState.CollectionFilter;
        set => queryState.CollectionFilter = value;
    }

    private SongSelectSortMode sortMode
    {
        get => queryState.SortMode;
        set => queryState.SortMode = value;
    }

    private string? selectedBackgroundPath
    {
        get => backgroundState.Path;
        set => backgroundState.Path = value;
    }

    private string? selectedBackgroundBeatmapKey
    {
        get => backgroundState.BeatmapKey;
        set => backgroundState.BeatmapKey = value;
    }

    private float selectedBackgroundLuminance
    {
        get => backgroundState.Luminance;
        set => backgroundState.Luminance = Math.Clamp(value, 0f, 1f);
    }

    public BeatmapInfo? SelectedBeatmap => SelectedSet?.Beatmaps.Count > 0
        ? SelectedSet.Beatmaps[Math.Clamp(selectedDifficultyIndex, 0, SelectedSet.Beatmaps.Count - 1)]
        : null;

    public string? SelectedBackgroundPath => selectedBackgroundPath;

    private BeatmapSetInfo? SelectedSet => selectedSetIndex >= 0 && selectedSetIndex < visibleSnapshot.Sets.Count ? visibleSnapshot.Sets[selectedSetIndex] : null;

    public void SetPreviewPlayer(IBeatmapPreviewPlayer player) => musicController.SetPreviewPlayer(player);

    public void SetTextInputService(ITextInputService service) => textInputService = service;

    public void Enter(string? preferredSetDirectory = null, string? preferredBeatmapFilename = null)
    {
        var start = PerfDiagnostics.Start();
        snapshot = library.Snapshot;
        if (snapshot.Sets.Count == 0)
            snapshot = library.Load();
        if (snapshot.Sets.Count == 0 || library.NeedsScanRefresh())
            StartBackgroundLibraryRefresh();
        ApplyBeatmapOptions(preferredSetDirectory, preferredBeatmapFilename, queueDifficultyCalculations: false);
        selectedSetExpansion = 1f;
        scrollY = ClampScroll(CalculateSelectedSetScroll(selectedSetIndex));
        RefreshSelectedBackgroundPath();
        QueueVisibleDifficultyCalculations();
        PlaySelectedPreview();
        PerfDiagnostics.Log("songSelect.enter", start, $"sets={visibleSnapshot.Sets.Count} selectedSet={selectedSetIndex}");
    }

    public void PrepareForWarmup()
    {
        if (visibleSnapshot.Sets.Count > 0)
            return;

        snapshot = library.Snapshot;
        if (snapshot.Sets.Count == 0)
            snapshot = library.Load();
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
        var elapsedSeconds = (float)elapsed.TotalSeconds;
        selectedSetExpansion = Math.Clamp(selectedSetExpansion + elapsedSeconds * 2f, 0f, 1f);
        selectedBackgroundLuminance += elapsedSeconds * BackgroundLuminancePerSecond;
        ApplyCompletedLibraryRefresh();
        ApplyCompletedDifficultyUpdates();
    }

    public void Scroll(float deltaY, UiPoint point, VirtualViewport viewport)
    {
        if (collectionsOpen)
        {
            collectionScrollY = Math.Clamp(collectionScrollY + deltaY, 0f, MaxCollectionScroll(viewport));
            return;
        }

        if (propertiesOpen || beatmapOptionsOpen)
            return;

        if (point.X < viewport.VirtualWidth * ScrollTouchMinimumXRatio)
            return;

        Scroll(deltaY, viewport);
    }

    public void Scroll(float deltaY, VirtualViewport viewport)
    {
        if (collectionsOpen)
        {
            collectionScrollY = Math.Clamp(collectionScrollY + deltaY, 0f, MaxCollectionScroll(viewport));
            return;
        }

        if (propertiesOpen || beatmapOptionsOpen)
            return;

        scrollY = ClampScroll(scrollY + deltaY);
    }

}
