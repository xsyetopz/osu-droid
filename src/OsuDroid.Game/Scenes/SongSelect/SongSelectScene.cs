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

public sealed class SongSelectScene(IBeatmapLibrary library, IMenuMusicController musicController, IBeatmapDifficultyService difficultyService, string songsPath, OnlineProfileSnapshot? profile = null, ITextInputService? textInputService = null)
{
    private const float RowWidth = 724f;
    private const float RowHeight = 127f;
    private const float CollapsedRowHeight = 97f;
    private const float RowSpacing = 92f;
    private const float ExpandedRowSpacing = 102f;
    private const float RowBaseY = 300f;
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

    private readonly int[] visibleSetActions = Enumerable.Repeat(-1, VisibleSetSlots).ToArray();
    private readonly int[] visibleDifficultyActions = Enumerable.Repeat(-1, VisibleDifficultySlots).ToArray();
    private readonly int[] visibleCollectionActions = Enumerable.Repeat(-1, VisibleCollectionSlots).ToArray();
    private readonly OnlineProfileSnapshot profile = profile ?? OnlineProfileSnapshot.Guest;
    private readonly object difficultyGate = new();
    private readonly HashSet<string> pendingDifficultyKeys = new(StringComparer.Ordinal);
    private readonly Queue<BeatmapInfo> completedDifficultyUpdates = new();
    private readonly object libraryRefreshGate = new();
    private ITextInputService textInputService = textInputService ?? new NoOpTextInputService();

    private BeatmapLibrarySnapshot snapshot = BeatmapLibrarySnapshot.Empty;
    private BeatmapLibrarySnapshot visibleSnapshot = BeatmapLibrarySnapshot.Empty;
    private Task? libraryRefreshTask;
    private BeatmapLibrarySnapshot? completedLibraryRefresh;
    private int selectedSetIndex;
    private int selectedDifficultyIndex;
    private float scrollY;
    private float collectionScrollY;
    private float selectedSetExpansion = 1f;
    private bool propertiesOpen;
    private bool beatmapOptionsOpen;
    private bool collectionsOpen;
    private bool collectionsFilterMode;
    private bool deleteBeatmapConfirmOpen;
    private string? collectionPendingDelete;
    private string searchQuery = string.Empty;
    private bool favoriteOnlyFilter;
    private string? collectionFilter;
    private SongSelectSortMode sortMode = SongSelectSortMode.Title;
    private DifficultyAlgorithm displayAlgorithm = difficultyService.Algorithm;

    public BeatmapInfo? SelectedBeatmap => SelectedSet?.Beatmaps.Count > 0
        ? SelectedSet.Beatmaps[Math.Clamp(selectedDifficultyIndex, 0, SelectedSet.Beatmaps.Count - 1)]
        : null;

    public string? SelectedBackgroundPath => SelectedBeatmap?.GetBackgroundPath(songsPath) is { } path && File.Exists(path) ? path : null;

    private BeatmapSetInfo? SelectedSet => selectedSetIndex >= 0 && selectedSetIndex < visibleSnapshot.Sets.Count ? visibleSnapshot.Sets[selectedSetIndex] : null;

    public void SetPreviewPlayer(IBeatmapPreviewPlayer player) => musicController.SetPreviewPlayer(player);

    public void SetTextInputService(ITextInputService service) => textInputService = service;

    public void Enter(string? preferredSetDirectory = null, string? preferredBeatmapFilename = null)
    {
        snapshot = library.Snapshot;
        if (snapshot.Sets.Count == 0)
            snapshot = library.Load();
        if (snapshot.Sets.Count == 0 || library.NeedsScanRefresh())
            StartBackgroundLibraryRefresh();
        ApplyBeatmapOptions(preferredSetDirectory, preferredBeatmapFilename);
        selectedSetExpansion = 1f;
        scrollY = ClampScroll(CalculateSelectedSetScroll(selectedSetIndex));
        QueueVisibleDifficultyCalculations();
        PlaySelectedPreview();
    }

    public void Leave()
    {
        selectedSetExpansion = 1f;
        ClosePopups();
    }

    public void Update(TimeSpan elapsed)
    {
        selectedSetExpansion = Math.Clamp(selectedSetExpansion + (float)elapsed.TotalSeconds * 2f, 0f, 1f);
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

    public void SelectSet(int visibleSlot)
    {
        if (propertiesOpen || beatmapOptionsOpen || collectionsOpen)
            return;

        if (visibleSlot < 0 || visibleSlot >= visibleSetActions.Length)
            return;

        var index = visibleSetActions[visibleSlot] >= 0 ? visibleSetActions[visibleSlot] : visibleSlot;
        if (index < 0 || index >= visibleSnapshot.Sets.Count)
            return;

        selectedSetIndex = index;
        selectedSetExpansion = 1f;
        selectedDifficultyIndex = 0;
        scrollY = ClampScroll(CalculateSelectedSetScroll(index));
        QueueVisibleDifficultyCalculations();
        PlaySelectedPreview();
    }

    public void SelectFirstSet() => SelectSet(0);

    public void SelectDifficulty(int index)
    {
        if (propertiesOpen || beatmapOptionsOpen || collectionsOpen)
            return;

        if (index < 0 || index >= visibleDifficultyActions.Length)
            return;

        var set = SelectedSet;
        var difficultyIndex = visibleDifficultyActions[index] >= 0 ? visibleDifficultyActions[index] : index;
        if (set is null || difficultyIndex < 0 || difficultyIndex >= set.Beatmaps.Count)
            return;

        selectedDifficultyIndex = difficultyIndex;
        QueueVisibleDifficultyCalculations();
        PlaySelectedPreview();
    }

    public void OpenProperties()
    {
        if (SelectedSet is null)
            return;

        propertiesOpen = true;
        beatmapOptionsOpen = false;
        collectionsOpen = false;
        collectionsFilterMode = false;
        deleteBeatmapConfirmOpen = false;
        collectionPendingDelete = null;
    }

    public void OpenBeatmapOptions()
    {
        if (visibleSnapshot.Sets.Count == 0)
            return;

        beatmapOptionsOpen = true;
        propertiesOpen = false;
        collectionsOpen = false;
        collectionsFilterMode = false;
        deleteBeatmapConfirmOpen = false;
        collectionPendingDelete = null;
    }

    public void OpenPropertiesForDifficulty(int visibleSlot)
    {
        if (visibleSlot < 0 || visibleSlot >= visibleDifficultyActions.Length)
            return;

        var set = SelectedSet;
        var difficultyIndex = visibleDifficultyActions[visibleSlot] >= 0 ? visibleDifficultyActions[visibleSlot] : visibleSlot;
        if (set is null || difficultyIndex < 0 || difficultyIndex >= set.Beatmaps.Count)
            return;

        selectedDifficultyIndex = difficultyIndex;
        OpenProperties();
    }

    public void ClosePopups()
    {
        propertiesOpen = false;
        beatmapOptionsOpen = false;
        collectionsOpen = false;
        collectionsFilterMode = false;
        deleteBeatmapConfirmOpen = false;
        collectionPendingDelete = null;
        textInputService.HideTextInput();
    }

    public void CloseCollections()
    {
        collectionsOpen = false;
        collectionsFilterMode = false;
        collectionPendingDelete = null;
    }

    public void ToggleFavorite()
    {
        var options = CurrentOptions();
        if (options is null)
            return;

        library.SaveOptions(options with { IsFavorite = !options.IsFavorite });
    }

    public void AdjustOffset(int delta)
    {
        var options = CurrentOptions();
        if (options is null)
            return;

        library.SaveOptions(options with { Offset = Math.Clamp(options.Offset + delta, -250, 250) });
    }

    public void FocusOffsetInput(VirtualViewport viewport)
    {
        var options = CurrentOptions();
        var bounds = PropertiesOffsetInputBounds(viewport);
        textInputService.RequestTextInput(new TextInputRequest(
            (options?.Offset ?? 0).ToString(CultureInfo.InvariantCulture),
            SaveOffsetText,
            SaveOffsetText,
            viewport.ToSurface(bounds),
            () => { }));
    }

    public void OpenCollections()
    {
        if (SelectedSet is null)
            return;

        collectionsOpen = true;
        collectionsFilterMode = false;
        deleteBeatmapConfirmOpen = false;
        collectionPendingDelete = null;
        collectionScrollY = 0f;
    }

    public void OpenCollectionFilter()
    {
        if (!beatmapOptionsOpen)
            return;

        collectionsOpen = true;
        collectionsFilterMode = true;
        deleteBeatmapConfirmOpen = false;
        collectionPendingDelete = null;
        collectionScrollY = 0f;
    }

    public void ToggleCollectionFilterPicker()
    {
        if (collectionsOpen && collectionsFilterMode)
        {
            collectionFilter = null;
            collectionsOpen = false;
            collectionsFilterMode = false;
            ApplyBeatmapOptions();
            return;
        }

        OpenCollectionFilter();
    }

    public void FocusBeatmapOptionsSearch(VirtualViewport viewport)
    {
        var bounds = BeatmapOptionsSearchBounds(viewport);
        textInputService.RequestTextInput(new TextInputRequest(
            searchQuery,
            SetBeatmapOptionsSearchQuery,
            SetBeatmapOptionsSearchQuery,
            viewport.ToSurface(bounds),
            () => { }));
    }

    public void SetBeatmapOptionsSearchQuery(string text)
    {
        searchQuery = text.Trim();
        ApplyBeatmapOptions();
    }

    public void ToggleBeatmapOptionsFavoriteOnly()
    {
        favoriteOnlyFilter = !favoriteOnlyFilter;
        ApplyBeatmapOptions();
    }

    public void ToggleBeatmapOptionsAlgorithm()
    {
        var selected = SelectedBeatmap;
        displayAlgorithm = displayAlgorithm == DifficultyAlgorithm.Droid ? DifficultyAlgorithm.Standard : DifficultyAlgorithm.Droid;
        visibleSnapshot = SortDifficultyRows(visibleSnapshot);
        RestoreSelectedDifficulty(selected);
        scrollY = ClampScroll(CalculateSelectedSetScroll(selectedSetIndex));
        QueueVisibleDifficultyCalculations();
    }

    public void CycleBeatmapOptionsSort()
    {
        sortMode = sortMode == SongSelectSortMode.Length ? SongSelectSortMode.Title : sortMode + 1;
        ApplyBeatmapOptions();
    }

    public void HandleCollectionPrimaryAction(int visibleSlot)
    {
        if (collectionsFilterMode)
        {
            SelectCollectionFilter(visibleSlot);
            return;
        }

        ToggleCollection(visibleSlot);
    }

    public void SelectCollectionFilter(int visibleSlot)
    {
        if (visibleSlot < 0 || visibleSlot >= visibleCollectionActions.Length)
            return;

        var index = visibleCollectionActions[visibleSlot] >= 0 ? visibleCollectionActions[visibleSlot] : visibleSlot;
        if (index == 0)
            collectionFilter = null;
        else
        {
            var collections = library.GetCollections(SelectedSet?.Directory);
            var collectionIndex = index - 1;
            if (collectionIndex < 0 || collectionIndex >= collections.Count)
                return;

            var collection = collections[collectionIndex];
            collectionFilter = string.Equals(collectionFilter, collection.Name, StringComparison.Ordinal)
                ? null
                : collection.Name;
        }

        collectionsOpen = false;
        collectionsFilterMode = false;
        ApplyBeatmapOptions();
    }

    public void FocusNewCollectionInput(VirtualViewport viewport)
    {
        var bounds = CollectionsNewFolderBounds(viewport);
        textInputService.RequestTextInput(new TextInputRequest(
            string.Empty,
            _ => { },
            CreateCollection,
            viewport.ToSurface(bounds),
            () => { }));
    }

    public void ToggleCollection(int visibleSlot)
    {
        var set = SelectedSet;
        var collection = CollectionAtVisibleSlot(visibleSlot);
        if (set is null || collection is null)
            return;

        library.ToggleCollectionMembership(collection.Name, set.Directory);
    }

    public void RequestDeleteCollection(int visibleSlot)
    {
        var collection = CollectionAtVisibleSlot(visibleSlot);
        if (collection is null)
            return;

        collectionPendingDelete = collection.Name;
    }

    public void ConfirmDeleteCollection()
    {
        if (collectionPendingDelete is null)
            return;

        library.DeleteCollection(collectionPendingDelete);
        collectionPendingDelete = null;
        collectionScrollY = Math.Clamp(collectionScrollY, 0f, MaxCollectionScroll(VirtualViewport.LegacyLandscape));
    }

    public void CancelDeleteCollection() => collectionPendingDelete = null;

    public void RequestDeleteBeatmap()
    {
        if (SelectedSet is not null)
            deleteBeatmapConfirmOpen = true;
    }

    public void CancelDeleteBeatmap() => deleteBeatmapConfirmOpen = false;

    public void ConfirmDeleteBeatmap()
    {
        var set = SelectedSet;
        if (set is null)
            return;

        library.DeleteBeatmapSet(set.Directory);
        snapshot = library.Load();
        ApplyBeatmapOptions();
        selectedSetIndex = visibleSnapshot.Sets.Count == 0 ? -1 : Math.Clamp(selectedSetIndex, 0, visibleSnapshot.Sets.Count - 1);
        selectedDifficultyIndex = 0;
        scrollY = ClampScroll(CalculateSelectedSetScroll(selectedSetIndex));
        ClosePopups();
        QueueVisibleDifficultyCalculations();
        PlaySelectedPreview();
    }

    public GameFrameSnapshot CreateSnapshot(VirtualViewport viewport) => new("SongSelect", "Song Select", string.Empty, Array.Empty<string>(), 0, false, CreateFrame(viewport));

    public static int SetIndex(UiAction action) => action switch
    {
        UiAction.SongSelectSet0 or UiAction.SongSelectFirstSet => 0,
        UiAction.SongSelectSet1 => 1,
        UiAction.SongSelectSet2 => 2,
        UiAction.SongSelectSet3 => 3,
        UiAction.SongSelectSet4 => 4,
        UiAction.SongSelectSet5 => 5,
        UiAction.SongSelectSet6 => 6,
        UiAction.SongSelectSet7 => 7,
        _ => -1,
    };

    public static int DifficultyIndex(UiAction action) => action switch
    {
        UiAction.SongSelectDifficulty0 => 0,
        UiAction.SongSelectDifficulty1 => 1,
        UiAction.SongSelectDifficulty2 => 2,
        UiAction.SongSelectDifficulty3 => 3,
        UiAction.SongSelectDifficulty4 => 4,
        UiAction.SongSelectDifficulty5 => 5,
        UiAction.SongSelectDifficulty6 => 6,
        UiAction.SongSelectDifficulty7 => 7,
        UiAction.SongSelectDifficulty8 => 8,
        UiAction.SongSelectDifficulty9 => 9,
        UiAction.SongSelectDifficulty10 => 10,
        UiAction.SongSelectDifficulty11 => 11,
        UiAction.SongSelectDifficulty12 => 12,
        UiAction.SongSelectDifficulty13 => 13,
        UiAction.SongSelectDifficulty14 => 14,
        UiAction.SongSelectDifficulty15 => 15,
        _ => -1,
    };

    private UiFrameSnapshot CreateFrame(VirtualViewport viewport)
    {
        Array.Fill(visibleSetActions, -1);
        Array.Fill(visibleDifficultyActions, -1);
        Array.Fill(visibleCollectionActions, -1);

        var elements = new List<UiElementSnapshot>
        {
            Fill("songselect-base", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), Black),
        };

        AddBeatmapBackground(elements, viewport);
        elements.Add(Fill("songselect-dim", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), BackgroundShade));
        AddBeatmapRows(elements, viewport);
        AddTopPanel(elements, viewport);
        AddBottomControls(elements, viewport);
        AddScorePreview(elements, viewport);
        AddModal(elements, viewport);

        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private void AddModal(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (!propertiesOpen && !beatmapOptionsOpen)
            return;

        elements.Add(Fill("songselect-popup-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), ModalShade, 1f, UiAction.SongSelectPropertiesDismiss));

        if (collectionsOpen)
            AddCollectionsPanel(elements, viewport);
        else if (beatmapOptionsOpen)
            AddBeatmapOptionsPanel(elements, viewport);
        else
            AddPropertiesPanel(elements, viewport);

        if (deleteBeatmapConfirmOpen)
            AddConfirmPanel(
                elements,
                viewport,
                "songselect-delete-beatmap",
                "Delete beatmap",
                "Are you sure?",
                UiAction.SongSelectPropertiesDeleteConfirm,
                UiAction.SongSelectPropertiesDeleteCancel);
        else if (collectionPendingDelete is not null)
            AddConfirmPanel(
                elements,
                viewport,
                "songselect-delete-collection",
                "Remove collection",
                "Are you sure?",
                UiAction.SongSelectCollectionDeleteConfirm,
                UiAction.SongSelectCollectionDeleteCancel);
    }

    private void AddPropertiesPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var options = CurrentOptions() ?? new BeatmapOptions(string.Empty);
        var panelHeight = PropertiesRowHeight * 5f;
        var panel = new UiRect((viewport.VirtualWidth - PropertiesWidth) / 2f, (viewport.VirtualHeight - panelHeight) / 2f, PropertiesWidth, panelHeight);
        elements.Add(Fill("songselect-properties-panel", panel, PropertiesPanel, 1f, UiAction.SongSelectPropertiesPanel, 14f * Dp));

        AddPropertiesRowText(elements, "songselect-properties-title", "Song Properties", panel.X, panel.Y, panel.Width, PropertiesRowHeight, 15f * Dp, White, UiAction.SongSelectPropertiesPanel);
        AddDivider(elements, "songselect-properties-divider-title", panel.X, panel.Y + PropertiesRowHeight, panel.Width);

        var offsetY = panel.Y + PropertiesRowHeight;
        var buttonWidth = 70f * Dp;
        elements.Add(Fill("songselect-properties-offset-minus-hit", new UiRect(panel.X, offsetY, buttonWidth, PropertiesRowHeight), PropertiesPanel, 1f, UiAction.SongSelectPropertiesOffsetMinus));
        elements.Add(MaterialIcon("songselect-properties-offset-minus", UiMaterialIcon.Minus, new UiRect(panel.X + (buttonWidth - 24f * Dp) / 2f, offsetY + 14f * Dp, 24f * Dp, 24f * Dp), White, 1f, UiAction.SongSelectPropertiesOffsetMinus));
        var input = PropertiesOffsetInputBounds(viewport);
        elements.Add(Fill("songselect-properties-offset-input-hit", input, PropertiesPanel, 1f, UiAction.SongSelectPropertiesOffsetInput));
        AddPropertiesRowText(elements, "songselect-properties-offset-label", "Offset", input.X, input.Y + 4f * Dp, input.Width, 16f * Dp, 10f * Dp, PropertiesSecondary, UiAction.SongSelectPropertiesOffsetInput);
        AddPropertiesRowText(elements, "songselect-properties-offset-value", options.Offset.ToString(CultureInfo.InvariantCulture), input.X, input.Y + 18f * Dp, input.Width, 30f * Dp, 18f * Dp, White, UiAction.SongSelectPropertiesOffsetInput);
        elements.Add(Fill("songselect-properties-offset-plus-hit", new UiRect(panel.Right - buttonWidth, offsetY, buttonWidth, PropertiesRowHeight), PropertiesPanel, 1f, UiAction.SongSelectPropertiesOffsetPlus));
        elements.Add(MaterialIcon("songselect-properties-offset-plus", UiMaterialIcon.Plus, new UiRect(panel.Right - buttonWidth + (buttonWidth - 24f * Dp) / 2f, offsetY + 14f * Dp, 24f * Dp, 24f * Dp), White, 1f, UiAction.SongSelectPropertiesOffsetPlus));
        AddDivider(elements, "songselect-properties-divider-offset", panel.X, offsetY + PropertiesRowHeight, panel.Width);

        var favoriteY = offsetY + PropertiesRowHeight;
        AddIconRow(elements, "songselect-properties-favorite", options.IsFavorite ? UiMaterialIcon.Heart : UiMaterialIcon.HeartOutline, "Add to Favorites", panel.X, favoriteY, panel.Width, UiAction.SongSelectPropertiesFavorite, White);
        AddDivider(elements, "songselect-properties-divider-favorite", panel.X, favoriteY + PropertiesRowHeight, panel.Width);

        var manageY = favoriteY + PropertiesRowHeight;
        AddIconRow(elements, "songselect-properties-manage", UiMaterialIcon.Folder, "Manage Favorites", panel.X, manageY, panel.Width, UiAction.SongSelectPropertiesManageCollections, White);
        AddDivider(elements, "songselect-properties-divider-manage", panel.X, manageY + PropertiesRowHeight, panel.Width);

        AddIconRow(elements, "songselect-properties-delete", UiMaterialIcon.Delete, "Delete beatmap", panel.X, manageY + PropertiesRowHeight, panel.Width, UiAction.SongSelectPropertiesDelete, PropertiesDanger);
    }

    private void AddBeatmapOptionsPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var search = BeatmapOptionsSearchBounds(viewport);
        elements.Add(Fill("songselect-beatmap-options-search", search, BeatmapOptionsSearchPanel, 1f, UiAction.SongSelectBeatmapOptionsSearch, BeatmapOptionsRadius));
        elements.Add(TextMiddle(
            "songselect-beatmap-options-search-text",
            searchQuery.Length == 0 ? "Search for..." : searchQuery,
            search.X + 16f * Dp,
            search.Y,
            search.Width - 64f * Dp,
            search.Height,
            16f * Dp,
            searchQuery.Length == 0 ? PropertiesSecondary : White,
            UiTextAlignment.Left,
            UiAction.SongSelectBeatmapOptionsSearch));
        elements.Add(MaterialIcon("songselect-beatmap-options-search-icon", UiMaterialIcon.Search, new UiRect(search.Right - 40f * Dp, search.Y + 16f * Dp, 24f * Dp, 24f * Dp), PropertiesSecondary, 1f, UiAction.SongSelectBeatmapOptionsSearch));

        var optionsY = search.Bottom + 12f * Dp;
        var x = search.X;
        var favoriteWidth = 56f * Dp;
        var algorithmWidth = 190f * Dp;
        var sortWidth = 150f * Dp;
        var folderWidth = 210f * Dp;
        var stripWidth = favoriteWidth + algorithmWidth + sortWidth + folderWidth + BeatmapOptionsDividerWidth * 3f;
        elements.Add(Fill("songselect-beatmap-options-strip", new UiRect(search.X, optionsY, stripWidth, BeatmapOptionsRowHeight), PropertiesPanel, 1f, UiAction.SongSelectPropertiesPanel, BeatmapOptionsRadius));

        AddOptionsButton(elements, "songselect-beatmap-options-favorite", new UiRect(x, optionsY, favoriteWidth, BeatmapOptionsRowHeight), favoriteOnlyFilter ? UiMaterialIcon.Heart : UiMaterialIcon.HeartOutline, string.Empty, UiAction.SongSelectBeatmapOptionsFavorite);
        x += favoriteWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-favorite", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(elements, "songselect-beatmap-options-algorithm", new UiRect(x, optionsY, algorithmWidth, BeatmapOptionsRowHeight), UiMaterialIcon.Star, displayAlgorithm == DifficultyAlgorithm.Standard ? "osu!standard" : "osu!droid", UiAction.SongSelectBeatmapOptionsAlgorithm);
        x += algorithmWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-algorithm", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(elements, "songselect-beatmap-options-sort", new UiRect(x, optionsY, sortWidth, BeatmapOptionsRowHeight), UiMaterialIcon.Sort, SortLabel(sortMode), UiAction.SongSelectBeatmapOptionsSort);
        x += sortWidth;
        AddOptionsDivider(elements, "songselect-beatmap-options-divider-sort", x, optionsY);
        x += BeatmapOptionsDividerWidth;
        AddOptionsButton(elements, "songselect-beatmap-options-folder", new UiRect(x, optionsY, folderWidth, BeatmapOptionsRowHeight), UiMaterialIcon.Folder, collectionFilter ?? "Folder", UiAction.SongSelectBeatmapOptionsFolder);
    }

    private void AddCollectionsPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var set = SelectedSet;
        var sourceCollections = library.GetCollections(set?.Directory).ToArray();
        var collections = collectionsFilterMode
            ? new[] { new BeatmapCollection("All folders", sourceCollections.Sum(collection => collection.BeatmapCount), collectionFilter is null) }.Concat(sourceCollections).ToArray()
            : sourceCollections;
        var panelHeight = viewport.VirtualHeight - CollectionsMargin * 2f;
        var panel = new UiRect((viewport.VirtualWidth - CollectionsWidth) / 2f, viewport.VirtualHeight - CollectionsMargin - panelHeight, CollectionsWidth, panelHeight);
        elements.Add(Fill("songselect-collections-panel", panel, CollectionsPanelDark, 1f, UiAction.SongSelectPropertiesPanel, 14f * Dp));
        AddIconRow(elements, "songselect-collections-new", UiMaterialIcon.Plus, "New folder", panel.X, panel.Y, panel.Width, UiAction.SongSelectCollectionsNewFolder, White);
        AddDivider(elements, "songselect-collections-divider-new", panel.X, panel.Y + PropertiesRowHeight, panel.Width);

        var listY = panel.Y + PropertiesRowHeight + 12f * Dp;
        var rowGap = 8f * Dp;
        var rowStep = CollectionRowHeight + rowGap;
        var first = Math.Max(0, (int)MathF.Floor(collectionScrollY / rowStep));
        var yOffset = -(collectionScrollY - first * rowStep);
        for (var slot = 0; slot < VisibleCollectionSlots; slot++)
        {
            var index = first + slot;
            if (index >= collections.Length)
                break;

            var rowY = listY + yOffset + slot * rowStep;
            if (rowY > panel.Bottom - rowGap)
                break;

            visibleCollectionActions[slot] = index;
            AddCollectionRow(elements, slot, collections[index], panel.X + 12f * Dp, rowY, panel.Width - 24f * Dp, collectionsFilterMode, collectionFilter);
        }

        if (collections.Length == 0)
            elements.Add(TextMiddle("songselect-collections-empty", "No collections", panel.X, listY, panel.Width, CollectionRowHeight, 16f * Dp, PropertiesSecondary, UiTextAlignment.Center));
    }

    private static void AddCollectionRow(List<UiElementSnapshot> elements, int slot, BeatmapCollection collection, float x, float y, float width, bool filterMode, string? selectedFilter)
    {
        var action = filterMode ? UiActionForCollectionToggle(slot) : UiAction.None;
        elements.Add(Fill($"songselect-collection-{slot}", new UiRect(x, y, width, CollectionRowHeight), PropertiesPanel, 1f, action, 14f * Dp));
        elements.Add(TextMiddle($"songselect-collection-{slot}-name", collection.Name, x + 16f * Dp, y, width - 180f * Dp, CollectionRowHeight, 15f * Dp, White, UiTextAlignment.Left, action));
        if (!filterMode || slot != 0)
            elements.Add(TextMiddle($"songselect-collection-{slot}-count", $"· {collection.BeatmapCount} beatmaps", x + 170f * Dp, y, width - 280f * Dp, CollectionRowHeight, 12f * Dp, PropertiesSecondary, UiTextAlignment.Left, action));
        if (filterMode)
        {
            if (string.Equals(collection.Name, selectedFilter, StringComparison.Ordinal))
                elements.Add(MaterialIcon($"songselect-collection-{slot}-selected", UiMaterialIcon.Check, new UiRect(x + width - 40f * Dp, y + 18f * Dp, 24f * Dp, 24f * Dp), White, 1f, action));
            return;
        }

        AddSmallAction(elements, $"songselect-collection-{slot}-delete", UiMaterialIcon.Delete, x + width - 112f * Dp, y, UiActionForCollectionDelete(slot), PropertiesDanger);
        AddSmallAction(elements, $"songselect-collection-{slot}-toggle", collection.ContainsSelectedSet ? UiMaterialIcon.Minus : UiMaterialIcon.Plus, x + width - 56f * Dp, y, UiActionForCollectionToggle(slot), White);
    }

    private static void AddSmallAction(List<UiElementSnapshot> elements, string id, UiMaterialIcon icon, float x, float y, UiAction action, UiColor color)
    {
        elements.Add(Fill(id + "-hit", new UiRect(x, y, 56f * Dp, CollectionRowHeight), PropertiesPanel, 1f, action));
        elements.Add(MaterialIcon(id, icon, new UiRect(x + 16f * Dp, y + 18f * Dp, 24f * Dp, 24f * Dp), color, 1f, action));
    }

    private static void AddConfirmPanel(List<UiElementSnapshot> elements, VirtualViewport viewport, string id, string title, string message, UiAction confirmAction, UiAction cancelAction)
    {
        elements.Add(Fill(id + "-shade", new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), new UiColor(0, 0, 0, 96), 1f, cancelAction));
        var width = 300f * Dp;
        var height = 150f * Dp;
        var panel = new UiRect((viewport.VirtualWidth - width) / 2f, (viewport.VirtualHeight - height) / 2f, width, height);
        elements.Add(Fill(id + "-panel", panel, PropertiesPanel, 1f, UiAction.SongSelectPropertiesPanel, 14f * Dp));
        AddPropertiesRowText(elements, id + "-title", title, panel.X, panel.Y, panel.Width, 44f * Dp, 15f * Dp, White, UiAction.SongSelectPropertiesPanel);
        AddPropertiesRowText(elements, id + "-message", message, panel.X, panel.Y + 44f * Dp, panel.Width, 44f * Dp, 14f * Dp, PropertiesSecondary, UiAction.SongSelectPropertiesPanel);
        AddFullWidthRow(elements, id + "-yes", "Yes", panel.X, panel.Y + 88f * Dp, panel.Width / 2f, confirmAction, PropertiesDanger);
        AddFullWidthRow(elements, id + "-no", "No", panel.X + panel.Width / 2f, panel.Y + 88f * Dp, panel.Width / 2f, cancelAction, White);
    }

    private static void AddFullWidthRow(List<UiElementSnapshot> elements, string id, string text, float x, float y, float width, UiAction action, UiColor color)
    {
        elements.Add(Fill(id + "-hit", new UiRect(x, y, width, PropertiesRowHeight), PropertiesPanel, 1f, action));
        elements.Add(TextMiddle(id, text, x + 16f * Dp, y, width - 32f * Dp, PropertiesRowHeight, 15f * Dp, color, UiTextAlignment.Left, action));
    }

    private static void AddIconRow(List<UiElementSnapshot> elements, string id, UiMaterialIcon icon, string text, float x, float y, float width, UiAction action, UiColor color)
    {
        elements.Add(Fill(id + "-hit", new UiRect(x, y, width, PropertiesRowHeight), PropertiesPanel, 1f, action));
        elements.Add(MaterialIcon(id + "-icon", icon, new UiRect(x + 24f * Dp, y + 14f * Dp, 24f * Dp, 24f * Dp), color, 1f, action));
        elements.Add(TextMiddle(id, text, x + 58f * Dp, y, width - 74f * Dp, PropertiesRowHeight, 15f * Dp, color, UiTextAlignment.Left, action));
    }

    private static void AddOptionsButton(List<UiElementSnapshot> elements, string id, UiRect bounds, UiMaterialIcon icon, string text, UiAction action)
    {
        elements.Add(Fill(id + "-hit", bounds, PropertiesPanel, 0f, action));
        var iconX = bounds.X + 16f * Dp;
        if (text.Length == 0)
            iconX = bounds.X + (bounds.Width - 24f * Dp) / 2f;
        elements.Add(MaterialIcon(id + "-icon", icon, new UiRect(iconX, bounds.Y + 14f * Dp, 24f * Dp, 24f * Dp), White, 1f, action));
        if (text.Length > 0)
            elements.Add(TextMiddle(id, text, bounds.X + 52f * Dp, bounds.Y, bounds.Width - 68f * Dp, bounds.Height, 14f * Dp, White, UiTextAlignment.Left, action));
    }

    private static void AddOptionsDivider(List<UiElementSnapshot> elements, string id, float x, float y) =>
        elements.Add(Fill(id, new UiRect(x, y, BeatmapOptionsDividerWidth, BeatmapOptionsRowHeight), BeatmapOptionsDivider, 1f));

    private static void AddPropertiesRowText(List<UiElementSnapshot> elements, string id, string text, float x, float y, float width, float height, float size, UiColor color, UiAction action) =>
        elements.Add(TextMiddle(id, text, x, y, width, height, size, color, UiTextAlignment.Center, action));

    private static void AddDivider(List<UiElementSnapshot> elements, string id, float x, float y, float width) =>
        elements.Add(Fill(id, new UiRect(x, y, width, 1f * Dp), PropertiesDivider, 1f));

    private static UiRect BeatmapOptionsSearchBounds(VirtualViewport viewport)
    {
        var width = Math.Min(BeatmapOptionsWidth, viewport.VirtualWidth - 120f * Dp);
        return new UiRect((viewport.VirtualWidth - width) / 2f, 8f * Dp, width, BeatmapOptionsSearchHeight);
    }

    private static string SortLabel(SongSelectSortMode mode) => mode switch
    {
        SongSelectSortMode.Artist => "Artist",
        SongSelectSortMode.Creator => "Creator",
        SongSelectSortMode.Date => "Date",
        SongSelectSortMode.Bpm => "BPM",
        SongSelectSortMode.DroidStars => "Droid ★",
        SongSelectSortMode.StandardStars => "Std ★",
        SongSelectSortMode.Length => "Length",
        _ => "Title",
    };

    private void AddBeatmapBackground(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (SelectedBackgroundPath is { } backgroundPath)
        {
            elements.Add(new UiElementSnapshot("songselect-beatmap-background", UiElementKind.Sprite, new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), White, 1f, ExternalAssetPath: backgroundPath, SpriteFit: UiSpriteFit.Cover));
            return;
        }

        elements.Add(new UiElementSnapshot("songselect-fallback-background", UiElementKind.Sprite, new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight), White, 1f, DroidAssets.MenuBackground, SpriteFit: UiSpriteFit.Cover));
    }

    private void AddTopPanel(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        elements.Add(new UiElementSnapshot(
            "songselect-top-overlay",
            UiElementKind.Sprite,
            new UiRect(SongSelectTopX, 0f, SongSelectTopWidth, TopPanelHeight),
            White,
            0.6f,
            DroidAssets.SongSelectTop,
            SpriteFit: UiSpriteFit.Stretch));

        var beatmap = SelectedBeatmap;
        if (beatmap is null)
        {
            elements.Add(Text("songselect-empty", "There are no songs in library, try using the beatmap downloader.", 70f, 2f, 850f, 36f, 24f, White));
            return;
        }

        AddTopPanelText(elements, beatmap);
    }

    private void AddBeatmapRows(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (visibleSnapshot.Sets.Count == 0)
            return;

        var visibleSlot = 0;
        var y = -scrollY;
        for (var setIndex = 0; setIndex < visibleSnapshot.Sets.Count; setIndex++)
        {
            var set = visibleSnapshot.Sets[setIndex];
            var firstBeatmap = set.Beatmaps.FirstOrDefault();
            if (firstBeatmap is null)
                continue;

            var height = setIndex == selectedSetIndex
                ? CalculateSelectedSetHeight(set)
                : CollapsedRowHeight;
            var rowY = RowBaseY + y;
            var x = CalculateRowX(y + viewport.VirtualHeight * 0.5f + height * 0.5f, viewport);
            if (setIndex == selectedSetIndex)
            {
                AddSelectedDifficultyRows(elements, set, rowY, x, viewport);
            }
            else if (rowY > -RowHeight && rowY < viewport.VirtualHeight && visibleSlot < VisibleSetSlots)
            {
                var action = SetAction(visibleSlot);
                visibleSetActions[visibleSlot] = setIndex;
                AddSetRow(elements, $"songselect-set-{visibleSlot}", firstBeatmap, new UiRect(x, rowY, RowWidth, RowHeight), action);
                visibleSlot++;
            }

            y += height;
        }
    }

    private void AddSelectedDifficultyRows(List<UiElementSnapshot> elements, BeatmapSetInfo set, float anchorY, float anchorX, VirtualViewport viewport)
    {
        var beatmaps = set.Beatmaps.Take(VisibleDifficultySlots).ToArray();
        var y = anchorY;
        for (var index = 0; index < beatmaps.Length; index++)
        {
            var beatmap = beatmaps[index];
            var centerY = y + viewport.VirtualHeight * 0.5f + RowHeight * 0.5f;
            var x = anchorX + 170f * MathF.Abs(MathF.Cos(centerY * MathF.PI / (viewport.VirtualHeight * 2f))) - 100f;
            var isSelected = index == selectedDifficultyIndex;
            var action = DifficultyAction(index);
            visibleDifficultyActions[index] = index;
            AddDifficultyRow(elements, $"songselect-diff-row-{index}", beatmap, new UiRect(x, y, RowWidth, RowHeight), isSelected, action);
            y += ExpandedRowSpacing * selectedSetExpansion;
        }
    }

    private static void AddSetRow(List<UiElementSnapshot> elements, string id, BeatmapInfo beatmap, UiRect bounds, UiAction action)
    {
        elements.Add(Sprite(id, DroidAssets.SongSelectButtonBackground, bounds, SetRowTint, 0.8f, action));
        elements.Add(Text($"{id}-title", $"{DisplayArtist(beatmap)} - {DisplayTitle(beatmap)}", bounds.X + 32f, bounds.Y + 25f, 620f, 34f, 24f, White, UiTextAlignment.Left, action));
        elements.Add(Text($"{id}-creator", $"Creator: {beatmap.Creator}", bounds.X + 150f, bounds.Y + 60f, 500f, 28f, 20f, White, UiTextAlignment.Left, action));
    }

    private void AddDifficultyRow(List<UiElementSnapshot> elements, string id, BeatmapInfo beatmap, UiRect bounds, bool isSelected, UiAction action)
    {
        var tint = isSelected ? SelectedRowTint : DifficultyRowTint;
        var textColor = isSelected ? Black : White;
        elements.Add(Sprite(id, DroidAssets.SongSelectButtonBackground, bounds, tint, 0.8f, action));
        elements.Add(Text($"{id}-title", $"{beatmap.Version} ({beatmap.Creator})", bounds.X + 32f, bounds.Y + 22f, 540f, 34f, 24f, textColor, UiTextAlignment.Left, action));

        var stars = Math.Clamp(CurrentStarRating(beatmap) ?? 0f, 0f, 10f);
        var fullStars = Math.Min(10, (int)MathF.Floor(stars));
        var starY = bounds.Y + 50f;
        for (var star = 0; star < fullStars; star++)
            elements.Add(Sprite($"{id}-star-{star}", DroidAssets.SongSelectStar, new UiRect(bounds.X + 60f + star * 52f, starY, 46f, 47f), White, 1f, action));

        var fraction = stars - fullStars;
        if (fraction > 0f && fullStars < 10)
            elements.Add(Sprite($"{id}-star-half", DroidAssets.SongSelectStar, new UiRect(bounds.X + 60f + fullStars * 52f, starY, 46f * fraction, 47f), White, 1f, action) with
            {
                SpriteSource = new UiRect(0f, 0f, 46f * fraction, 47f),
            });
    }

    private static void AddBottomControls(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        var backY = viewport.VirtualHeight - BackButtonSize;
        elements.Add(Sprite("songselect-back", DroidAssets.SongSelectBack, new UiRect(0f, backY, BackButtonSize, BackButtonSize), White, 1f, UiAction.SongSelectBack));

        var smallY = viewport.VirtualHeight - SmallButtonSize;
        var modsX = BackButtonSize;
        elements.Add(Sprite("songselect-mods", DroidAssets.SongSelectMods, new UiRect(modsX, smallY, SmallButtonSize, SmallButtonSize), White, 1f, UiAction.SongSelectMods));
        elements.Add(Sprite("songselect-options", DroidAssets.SongSelectOptions, new UiRect(modsX + SmallButtonSize, smallY, SmallButtonSize, SmallButtonSize), White, 1f, UiAction.SongSelectBeatmapOptions));
        elements.Add(Sprite("songselect-random", DroidAssets.SongSelectRandom, new UiRect(modsX + SmallButtonSize * 2f, smallY, SmallButtonSize, SmallButtonSize), White, 1f, UiAction.SongSelectRandom));
    }

    private void AddScorePreview(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        elements.Add(Sprite("songselect-scoring-switcher", DroidAssets.RankingDisabled, new UiRect(10f, 10f, 50f, 50f), White, 1f));

        var panelX = BackButtonSize + SmallButtonSize * 3f + OnlinePanelGap;
        var panelY = viewport.VirtualHeight - OnlinePanelHeight;
        OnlineProfilePanelSnapshots.Add(
            elements,
            "songselect-score",
            new UiRect(panelX, panelY, OnlinePanelWidth, OnlinePanelHeight),
            OnlineAvatarFooterSize,
            profile);
    }

    private void AddTopPanelText(List<UiElementSnapshot> elements, BeatmapInfo beatmap)
    {
        var titleY = 2f;
        elements.Add(Text("songselect-title", $"{DisplayArtist(beatmap)} - {DisplayTitle(beatmap)} [{beatmap.Version}]", 70f, titleY, 1024f, 32f, 24f, White));

        var creatorY = titleY + 32f + 2f;
        elements.Add(Text("songselect-creator", $"Beatmap by {beatmap.Creator}", 70f, creatorY, 1024f, 26f, 20f, White));

        var lengthY = creatorY + 26f + 2f;
        elements.Add(Text("songselect-length", FormatLengthLine(beatmap), 4f, lengthY, 1024f, 26f, 18f, White));

        var objectsY = lengthY + 26f + 2f;
        elements.Add(Text("songselect-objects", FormatObjectLine(beatmap), 4f, objectsY, 1120f, 26f, 18f, White));

        var difficultyY = objectsY + 26f + 2f;
        elements.Add(Text("songselect-difficulty", FormatDifficultyLine(beatmap), 4f, difficultyY, 1024f, 24f, 18f, White));
    }

    private BeatmapOptions? CurrentOptions()
    {
        var set = SelectedSet;
        return set is null ? null : library.GetOptions(set.Directory);
    }

    private void ApplyBeatmapOptions(string? preferredSetDirectory = null, string? preferredBeatmapFilename = null)
    {
        var selected = SelectedBeatmap;
        var selectedDirectory = preferredSetDirectory ?? SelectedSet?.Directory;
        var sets = snapshot.Sets.AsEnumerable();

        if (searchQuery.Length > 0)
            sets = sets.Where(SetMatchesSearch);

        if (favoriteOnlyFilter)
            sets = sets.Where(set => library.GetOptions(set.Directory).IsFavorite);

        if (!string.IsNullOrWhiteSpace(collectionFilter))
        {
            var directories = library.GetCollectionSetDirectories(collectionFilter);
            sets = sets.Where(set => directories.Contains(set.Directory));
        }

        visibleSnapshot = SortDifficultyRows(new BeatmapLibrarySnapshot(SortSets(sets).ToArray()));
        if (visibleSnapshot.Sets.Count == 0)
        {
            selectedSetIndex = -1;
            selectedDifficultyIndex = 0;
            scrollY = 0f;
            return;
        }

        var nextIndex = selectedDirectory is null
            ? Math.Clamp(selectedSetIndex, 0, visibleSnapshot.Sets.Count - 1)
            : visibleSnapshot.Sets.ToList().FindIndex(set => string.Equals(set.Directory, selectedDirectory, StringComparison.Ordinal));
        selectedSetIndex = nextIndex >= 0 ? nextIndex : 0;
        if (!string.IsNullOrWhiteSpace(preferredBeatmapFilename))
            selectedDifficultyIndex = SelectInitialDifficultyIndex(preferredBeatmapFilename);
        else
            RestoreSelectedDifficulty(selected);
        scrollY = ClampScroll(CalculateSelectedSetScroll(selectedSetIndex));
        QueueVisibleDifficultyCalculations();
    }

    private bool SetMatchesSearch(BeatmapSetInfo set)
    {
        foreach (var beatmap in set.Beatmaps)
        {
            if (ContainsSearch(beatmap.Title) ||
                ContainsSearch(beatmap.TitleUnicode) ||
                ContainsSearch(beatmap.Artist) ||
                ContainsSearch(beatmap.ArtistUnicode) ||
                ContainsSearch(beatmap.Creator) ||
                ContainsSearch(beatmap.Version) ||
                ContainsSearch(beatmap.Tags) ||
                ContainsSearch(beatmap.Source))
            {
                return true;
            }
        }

        return ContainsSearch(set.Directory);
    }

    private bool ContainsSearch(string value) => value.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);

    private IEnumerable<BeatmapSetInfo> SortSets(IEnumerable<BeatmapSetInfo> sets) => sortMode switch
    {
        SongSelectSortMode.Artist => sets
            .OrderBy(set => set.Beatmaps.FirstOrDefault()?.Artist ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(set => set.Beatmaps.FirstOrDefault()?.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase),
        SongSelectSortMode.Creator => sets
            .OrderBy(set => set.Beatmaps.FirstOrDefault()?.Creator ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(set => set.Beatmaps.FirstOrDefault()?.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase),
        SongSelectSortMode.Date => sets.OrderByDescending(set => set.Beatmaps.Max(beatmap => beatmap.DateImported)),
        SongSelectSortMode.Bpm => sets.OrderBy(set => set.Beatmaps.FirstOrDefault()?.MostCommonBpm ?? 0f),
        SongSelectSortMode.DroidStars => sets.OrderBy(set => set.Beatmaps.Max(beatmap => beatmap.DroidStarRating ?? 0f)),
        SongSelectSortMode.StandardStars => sets.OrderBy(set => set.Beatmaps.Max(beatmap => beatmap.StandardStarRating ?? 0f)),
        SongSelectSortMode.Length => sets.OrderBy(set => set.Beatmaps.FirstOrDefault()?.Length ?? 0L),
        _ => sets
            .OrderBy(set => set.Beatmaps.FirstOrDefault()?.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(set => set.Beatmaps.FirstOrDefault()?.Artist ?? string.Empty, StringComparer.OrdinalIgnoreCase),
    };

    private void SaveOffsetText(string text)
    {
        var options = CurrentOptions();
        if (options is null)
            return;

        if (string.IsNullOrWhiteSpace(text))
        {
            library.SaveOptions(options with { Offset = 0 });
            return;
        }

        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            return;

        library.SaveOptions(options with { Offset = Math.Clamp(value, -250, 250) });
    }

    private void CreateCollection(string name)
    {
        var trimmed = name.Trim();
        if (trimmed.Length == 0)
            return;

        library.CreateCollection(trimmed);
    }

    private BeatmapCollection? CollectionAtVisibleSlot(int visibleSlot)
    {
        if (visibleSlot < 0 || visibleSlot >= visibleCollectionActions.Length)
            return null;

        var collections = library.GetCollections(SelectedSet?.Directory);
        var index = visibleCollectionActions[visibleSlot] >= 0 ? visibleCollectionActions[visibleSlot] : visibleSlot;
        return index >= 0 && index < collections.Count ? collections[index] : null;
    }

    private float MaxCollectionScroll(VirtualViewport viewport)
    {
        var count = library.GetCollections(SelectedSet?.Directory).Count;
        var panelHeight = Math.Min(viewport.VirtualHeight - CollectionsMargin * 2f, 500f * Dp);
        var listHeight = Math.Max(0f, panelHeight - PropertiesRowHeight - 24f * Dp);
        var contentHeight = count * (CollectionRowHeight + 8f * Dp);
        return Math.Max(0f, contentHeight - listHeight);
    }

    private static UiRect PropertiesOffsetInputBounds(VirtualViewport viewport)
    {
        var panelX = (viewport.VirtualWidth - PropertiesWidth) / 2f;
        var panelY = (viewport.VirtualHeight - PropertiesRowHeight * 5f) / 2f;
        return new UiRect(panelX + 70f * Dp, panelY + PropertiesRowHeight, PropertiesWidth - 140f * Dp, PropertiesRowHeight);
    }

    private static UiRect CollectionsNewFolderBounds(VirtualViewport viewport)
    {
        var panelHeight = Math.Min(viewport.VirtualHeight - CollectionsMargin * 2f, 500f * Dp);
        return new UiRect((viewport.VirtualWidth - CollectionsWidth) / 2f, (viewport.VirtualHeight - panelHeight) / 2f, CollectionsWidth, PropertiesRowHeight);
    }

    private int SelectInitialSetIndex(string? preferredSetDirectory)
    {
        if (preferredSetDirectory is not null)
        {
            var preferred = visibleSnapshot.Sets.ToList().FindIndex(set => string.Equals(set.Directory, preferredSetDirectory, StringComparison.Ordinal));
            if (preferred >= 0)
                return preferred;
        }

        return visibleSnapshot.Sets.Count > 0 ? 0 : -1;
    }

    private int SelectInitialDifficultyIndex(string? preferredBeatmapFilename)
    {
        var set = SelectedSet;
        if (set is null || string.IsNullOrWhiteSpace(preferredBeatmapFilename))
            return 0;

        var preferred = set.Beatmaps.ToList().FindIndex(beatmap => string.Equals(beatmap.Filename, preferredBeatmapFilename, StringComparison.Ordinal));
        return preferred >= 0 ? preferred : 0;
    }

    private float ClampScroll(float value)
    {
        var max = Math.Max(0f, RowBaseY + CalculateTotalScrollHeight() - VirtualViewport.LegacyLandscape.VirtualHeight * 0.5f);
        return Math.Clamp(value, -VirtualViewport.LegacyLandscape.VirtualHeight * 0.5f, max);
    }

    private float CalculateSelectedSetScroll(int setIndex)
    {
        if (setIndex < 0 || setIndex >= visibleSnapshot.Sets.Count)
            return 0f;

        var previousHeight = 0f;
        for (var index = 0; index < setIndex; index++)
            previousHeight += CollapsedRowHeight;

        return RowBaseY + previousHeight + CalculateSetTotalHeight(visibleSnapshot.Sets[setIndex]) * 0.5f - VirtualViewport.LegacyLandscape.VirtualHeight * 0.5f;
    }

    private float CalculateTotalScrollHeight()
    {
        var height = 0f;
        for (var index = 0; index < visibleSnapshot.Sets.Count; index++)
        {
            var set = visibleSnapshot.Sets[index];
            height += index == selectedSetIndex ? CalculateSetTotalHeight(set) : CollapsedRowHeight;
        }

        return height;
    }

    private static int VisibleDifficultyCount(BeatmapSetInfo set) => Math.Max(1, Math.Min(VisibleDifficultySlots, set.Beatmaps.Count));

    private float CalculateSelectedSetHeight(BeatmapSetInfo set)
    {
        var expandedHeight = ExpandedRowSpacing * VisibleDifficultyCount(set);
        return CollapsedRowHeight + (expandedHeight - CollapsedRowHeight) * selectedSetExpansion;
    }

    private static float CalculateSetTotalHeight(BeatmapSetInfo set) => ExpandedRowSpacing * VisibleDifficultyCount(set);

    private void QueueVisibleDifficultyCalculations()
    {
        var selected = SelectedBeatmap;
        var beatmaps = visibleSnapshot.Sets.SelectMany(set => set.Beatmaps)
            .OrderByDescending(beatmap => selected is not null && BeatmapMatches(beatmap, selected))
            .ThenByDescending(beatmap => SelectedSet is not null && string.Equals(beatmap.SetDirectory, SelectedSet.Directory, StringComparison.Ordinal))
            .Where(NeedsDifficultyCalculation)
            .Where(TrackPendingDifficulty)
            .ToArray();
        if (beatmaps.Length == 0)
            return;

        _ = Task.Run(() =>
        {
            foreach (var beatmap in beatmaps)
            {
                BeatmapInfo updated;
                try
                {
                    updated = difficultyService.EnsureCalculated(beatmap);
                }
                catch
                {
                    updated = beatmap;
                }

                lock (difficultyGate)
                {
                    pendingDifficultyKeys.Remove(DifficultyKey(beatmap));
                    completedDifficultyUpdates.Enqueue(updated);
                }
            }
        });
    }

    private bool TrackPendingDifficulty(BeatmapInfo beatmap)
    {
        lock (difficultyGate)
            return pendingDifficultyKeys.Add(DifficultyKey(beatmap));
    }

    private bool NeedsDifficultyCalculation(BeatmapInfo beatmap) =>
        beatmap.DroidStarRating is null || beatmap.StandardStarRating is null;

    private void ApplyCompletedDifficultyUpdates()
    {
        var applied = false;
        while (true)
        {
            BeatmapInfo updated;
            lock (difficultyGate)
            {
                if (completedDifficultyUpdates.Count == 0)
                {
                    if (applied && sortMode is SongSelectSortMode.DroidStars or SongSelectSortMode.StandardStars)
                        ApplyBeatmapOptions();
                    return;
                }

                updated = completedDifficultyUpdates.Dequeue();
            }

            snapshot = ReplaceBeatmap(snapshot, updated);
            var selected = SelectedBeatmap;
            visibleSnapshot = SortDifficultyRows(ReplaceBeatmap(visibleSnapshot, updated));
            RestoreSelectedDifficulty(selected);
            applied = true;
        }
    }

    private void StartBackgroundLibraryRefresh()
    {
        lock (libraryRefreshGate)
        {
            if (libraryRefreshTask is { IsCompleted: false })
                return;

            libraryRefreshTask = Task.Run(() =>
            {
                try
                {
                    lock (libraryRefreshGate)
                    {
                        completedLibraryRefresh = library.Scan();
                    }
                }
                catch
                {
                }
            });
        }
    }

    private void ApplyCompletedLibraryRefresh()
    {
        BeatmapLibrarySnapshot? refreshed;
        lock (libraryRefreshGate)
        {
            refreshed = completedLibraryRefresh;
            completedLibraryRefresh = null;
        }

        if (refreshed is null)
            return;

        snapshot = refreshed;
        ApplyBeatmapOptions();
        QueueVisibleDifficultyCalculations();
    }

    private static BeatmapLibrarySnapshot ReplaceBeatmap(BeatmapLibrarySnapshot source, BeatmapInfo updated)
    {
        if (source.Sets.Count == 0)
            return source;

        var changed = false;
        var sets = source.Sets.Select(set =>
        {
            var beatmaps = set.Beatmaps.ToArray();
            var index = Array.FindIndex(beatmaps, beatmap => BeatmapMatches(beatmap, updated));
            if (index < 0)
                return set;

            beatmaps[index] = updated;
            changed = true;
            return set with { Beatmaps = beatmaps };
        }).ToArray();

        return changed ? new BeatmapLibrarySnapshot(sets) : source;
    }

    private static bool BeatmapMatches(BeatmapInfo left, BeatmapInfo right) =>
        string.Equals(left.SetDirectory, right.SetDirectory, StringComparison.Ordinal) &&
        string.Equals(left.Filename, right.Filename, StringComparison.Ordinal);

    private static string DifficultyKey(BeatmapInfo beatmap) => string.Concat(beatmap.SetDirectory, "/", beatmap.Filename);

    private BeatmapLibrarySnapshot SortDifficultyRows(BeatmapLibrarySnapshot source)
    {
        if (source.Sets.Count == 0)
            return source;

        return new BeatmapLibrarySnapshot(source.Sets
            .Select(set => set with { Beatmaps = SortBeatmapsByDifficulty(set.Beatmaps).ToArray() })
            .ToArray());
    }

    private IEnumerable<BeatmapInfo> SortBeatmapsByDifficulty(IEnumerable<BeatmapInfo> beatmaps) => beatmaps
        .OrderBy(beatmap => CurrentStarRating(beatmap) is null)
        .ThenBy(beatmap => CurrentStarRating(beatmap) ?? float.MaxValue)
        .ThenBy(beatmap => beatmap.Version, StringComparer.OrdinalIgnoreCase)
        .ThenBy(beatmap => beatmap.Filename, StringComparer.OrdinalIgnoreCase);

    private void RestoreSelectedDifficulty(BeatmapInfo? selected)
    {
        var set = SelectedSet;
        if (set is null || set.Beatmaps.Count == 0)
        {
            selectedDifficultyIndex = 0;
            return;
        }

        if (selected is not null)
        {
            var index = set.Beatmaps.ToList().FindIndex(beatmap => BeatmapMatches(beatmap, selected));
            if (index >= 0)
            {
                selectedDifficultyIndex = index;
                return;
            }
        }

        selectedDifficultyIndex = Math.Clamp(selectedDifficultyIndex, 0, set.Beatmaps.Count - 1);
    }

    private void PlaySelectedPreview()
    {
        var set = SelectedSet;
        var beatmap = SelectedBeatmap;
        if (set is null || beatmap is null)
            return;

        var audioPath = beatmap.GetAudioPath(songsPath);
        musicController.Queue(
            new MenuTrack(
                $"beatmap:{set.Directory}/{beatmap.Filename}",
                $"{DisplayArtist(beatmap)} - {DisplayTitle(beatmap)}",
                audioPath,
                beatmap.EffectivePreviewTime,
                (int)Math.Clamp(beatmap.Length, 0L, int.MaxValue),
                beatmap.MostCommonBpm,
                set.Directory,
                beatmap.Filename),
            File.Exists(audioPath));
    }

    private float? CurrentStarRating(BeatmapInfo beatmap) => displayAlgorithm == DifficultyAlgorithm.Standard ? beatmap.StandardStarRating : beatmap.DroidStarRating;

    private static float CalculateRowX(float centerY, VirtualViewport viewport)
    {
        return viewport.VirtualWidth / 1.85f + 200f * MathF.Abs(MathF.Cos(centerY * MathF.PI / (viewport.VirtualHeight * 2f)));
    }

    private static string DisplayTitle(BeatmapInfo beatmap) => string.IsNullOrWhiteSpace(beatmap.TitleUnicode) ? beatmap.Title : beatmap.TitleUnicode;

    private static string DisplayArtist(BeatmapInfo beatmap) => string.IsNullOrWhiteSpace(beatmap.ArtistUnicode) ? beatmap.Artist : beatmap.ArtistUnicode;

    private static string FormatLengthLine(BeatmapInfo beatmap)
    {
        var bpm = Math.Abs(beatmap.BpmMax - beatmap.BpmMin) < 0.01f
            ? beatmap.MostCommonBpm.ToString("0", CultureInfo.InvariantCulture)
            : string.Create(CultureInfo.InvariantCulture, $"{beatmap.BpmMin:0}-{beatmap.BpmMax:0} ({beatmap.MostCommonBpm:0})");
        return $"Length: {TimeSpan.FromMilliseconds(beatmap.Length):m\\:ss} BPM: {bpm} Combo: {beatmap.MaxCombo}";
    }

    private static string FormatObjectLine(BeatmapInfo beatmap) =>
        $"Circles: {beatmap.HitCircleCount} Sliders: {beatmap.SliderCount} Spinners: {beatmap.SpinnerCount} (MapId: {beatmap.SetId?.ToString(CultureInfo.InvariantCulture) ?? "0"})";

    private string FormatDifficultyLine(BeatmapInfo beatmap)
    {
        var stars = CurrentStarRating(beatmap) is float value ? value.ToString("0.##", CultureInfo.InvariantCulture) : "...";
        return $"AR: {beatmap.ApproachRate:0.#} OD: {beatmap.OverallDifficulty:0.#} CS: {beatmap.CircleSize:0.#} HP: {beatmap.HpDrainRate:0.#} Stars: {stars}";
    }

    private static UiAction SetAction(int visibleSlot) => visibleSlot switch
    {
        0 => UiAction.SongSelectSet0,
        1 => UiAction.SongSelectSet1,
        2 => UiAction.SongSelectSet2,
        3 => UiAction.SongSelectSet3,
        4 => UiAction.SongSelectSet4,
        5 => UiAction.SongSelectSet5,
        6 => UiAction.SongSelectSet6,
        7 => UiAction.SongSelectSet7,
        _ => UiAction.None,
    };

    private static UiAction DifficultyAction(int index) => index switch
    {
        0 => UiAction.SongSelectDifficulty0,
        1 => UiAction.SongSelectDifficulty1,
        2 => UiAction.SongSelectDifficulty2,
        3 => UiAction.SongSelectDifficulty3,
        4 => UiAction.SongSelectDifficulty4,
        5 => UiAction.SongSelectDifficulty5,
        6 => UiAction.SongSelectDifficulty6,
        7 => UiAction.SongSelectDifficulty7,
        8 => UiAction.SongSelectDifficulty8,
        9 => UiAction.SongSelectDifficulty9,
        10 => UiAction.SongSelectDifficulty10,
        11 => UiAction.SongSelectDifficulty11,
        12 => UiAction.SongSelectDifficulty12,
        13 => UiAction.SongSelectDifficulty13,
        14 => UiAction.SongSelectDifficulty14,
        15 => UiAction.SongSelectDifficulty15,
        _ => UiAction.None,
    };

    private static UiAction UiActionForCollectionToggle(int index) => index switch
    {
        0 => UiAction.SongSelectCollectionToggle0,
        1 => UiAction.SongSelectCollectionToggle1,
        2 => UiAction.SongSelectCollectionToggle2,
        3 => UiAction.SongSelectCollectionToggle3,
        4 => UiAction.SongSelectCollectionToggle4,
        5 => UiAction.SongSelectCollectionToggle5,
        6 => UiAction.SongSelectCollectionToggle6,
        7 => UiAction.SongSelectCollectionToggle7,
        _ => UiAction.None,
    };

    private static UiAction UiActionForCollectionDelete(int index) => index switch
    {
        0 => UiAction.SongSelectCollectionDelete0,
        1 => UiAction.SongSelectCollectionDelete1,
        2 => UiAction.SongSelectCollectionDelete2,
        3 => UiAction.SongSelectCollectionDelete3,
        4 => UiAction.SongSelectCollectionDelete4,
        5 => UiAction.SongSelectCollectionDelete5,
        6 => UiAction.SongSelectCollectionDelete6,
        7 => UiAction.SongSelectCollectionDelete7,
        _ => UiAction.None,
    };

    private static UiElementSnapshot Fill(string id, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None, float radius = 0f) =>
        new(id, UiElementKind.Fill, bounds, color, alpha, Action: action, CornerRadius: radius);

    private static UiElementSnapshot Sprite(string id, string asset, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) =>
        new(id, UiElementKind.Sprite, bounds, color, alpha, asset, action);

    private static UiElementSnapshot Text(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) =>
        new(id, UiElementKind.Text, new UiRect(x, y, width, height), color, 1f, Action: action, Text: text, TextStyle: new UiTextStyle(size, false, alignment));

    private static UiElementSnapshot TextMiddle(string id, string text, float x, float y, float width, float height, float size, UiColor color, UiTextAlignment alignment = UiTextAlignment.Left, UiAction action = UiAction.None) =>
        new(id, UiElementKind.Text, new UiRect(x, y, width, height), color, 1f, Action: action, Text: text, TextStyle: new UiTextStyle(size, false, alignment, VerticalAlignment: UiTextVerticalAlignment.Middle));

    private static UiElementSnapshot MaterialIcon(string id, UiMaterialIcon icon, UiRect bounds, UiColor color, float alpha = 1f, UiAction action = UiAction.None) =>
        new(
            id,
            UiElementKind.MaterialIcon,
            bounds,
            color,
            alpha,
            Action: action,
            MaterialIcon: icon);
}
