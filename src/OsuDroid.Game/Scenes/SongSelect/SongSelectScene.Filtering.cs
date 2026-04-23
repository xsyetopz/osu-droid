using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

public sealed partial class SongSelectScene
{
    private BeatmapOptions? CurrentOptions()
    {
        var set = SelectedSet;
        return set is null ? null : library.GetOptions(set.Directory);
    }

    private void ApplyBeatmapOptions(string? preferredSetDirectory = null, string? preferredBeatmapFilename = null, bool queueDifficultyCalculations = true)
    {
        var start = PerfDiagnostics.Start();
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
            selectedBackgroundPath = null;
            selectedBackgroundBeatmapKey = null;
            PerfDiagnostics.Log("songSelect.applyOptions", start, "sets=0");
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
        RefreshSelectedBackgroundPath();
        if (queueDifficultyCalculations)
            QueueVisibleDifficultyCalculations();
        PerfDiagnostics.Log("songSelect.applyOptions", start, $"sets={visibleSnapshot.Sets.Count} search={searchQuery.Length} favorite={favoriteOnlyFilter} sort={sortMode}");
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
        if (visibleSlot < 0 || visibleSlot >= visibleCollectionIndices.Length)
            return null;

        var collections = library.GetCollections(SelectedSet?.Directory);
        var index = visibleCollectionIndices[visibleSlot] >= 0 ? visibleCollectionIndices[visibleSlot] : visibleSlot;
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
        var count = VisibleDifficultyCount(set);
        return ExpandedRowSpacing + ExpandedRowSpacing * (count - 1) * selectedSetExpansion;
    }

    private static float CalculateSetTotalHeight(BeatmapSetInfo set) => ExpandedRowSpacing * VisibleDifficultyCount(set);
}
