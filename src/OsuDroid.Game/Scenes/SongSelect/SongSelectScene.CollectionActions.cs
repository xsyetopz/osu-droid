using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

public sealed partial class SongSelectScene
{
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
}
