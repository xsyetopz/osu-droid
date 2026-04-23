using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    public void OpenCollections()
    {
        if (SelectedSet is null)
        {
            return;
        }

        _collectionsOpen = true;
        _collectionsFilterMode = false;
        _deleteBeatmapConfirmOpen = false;
        _collectionPendingDelete = null;
        collectionScrollY = 0f;
    }

    public void OpenCollectionFilter()
    {
        if (!_beatmapOptionsOpen)
        {
            return;
        }

        _collectionsOpen = true;
        _collectionsFilterMode = true;
        _deleteBeatmapConfirmOpen = false;
        _collectionPendingDelete = null;
        collectionScrollY = 0f;
    }

    public void ToggleCollectionFilterPicker()
    {
        if (_collectionsOpen && _collectionsFilterMode)
        {
            collectionFilter = null;
            _collectionsOpen = false;
            _collectionsFilterMode = false;
            ApplyBeatmapOptions();
            return;
        }

        OpenCollectionFilter();
    }

    public void FocusBeatmapOptionsSearch(VirtualViewport viewport)
    {
        UiRect bounds = BeatmapOptionsSearchBounds(viewport);
        _textInputService.RequestTextInput(new TextInputRequest(
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

    public DifficultyAlgorithm ToggleBeatmapOptionsAlgorithm()
    {
        DifficultyAlgorithm next = _displayAlgorithm == DifficultyAlgorithm.Droid ? DifficultyAlgorithm.Standard : DifficultyAlgorithm.Droid;
        SetDisplayAlgorithm(next);
        return _displayAlgorithm;
    }

    public void CycleBeatmapOptionsSort()
    {
        sortMode = sortMode == SongSelectSortMode.Length ? SongSelectSortMode.Title : sortMode + 1;
        ApplyBeatmapOptions();
    }

    public void HandleCollectionPrimaryAction(int visibleSlot)
    {
        if (_collectionsFilterMode)
        {
            SelectCollectionFilter(visibleSlot);
            return;
        }

        ToggleCollection(visibleSlot);
    }

    public void SelectCollectionFilter(int visibleSlot)
    {
        if (visibleSlot < 0 || visibleSlot >= _visibleCollectionIndices.Length)
        {
            return;
        }

        int index = _visibleCollectionIndices[visibleSlot] >= 0 ? _visibleCollectionIndices[visibleSlot] : visibleSlot;
        if (index == 0)
        {
            collectionFilter = null;
        }
        else
        {
            IReadOnlyList<BeatmapCollection> collections = library.GetCollections(SelectedSet?.Directory);
            int collectionIndex = index - 1;
            if (collectionIndex < 0 || collectionIndex >= collections.Count)
            {
                return;
            }

            BeatmapCollection collection = collections[collectionIndex];
            collectionFilter = string.Equals(collectionFilter, collection.Name, StringComparison.Ordinal)
                ? null
                : collection.Name;
        }

        _collectionsOpen = false;
        _collectionsFilterMode = false;
        ApplyBeatmapOptions();
    }

    public void FocusNewCollectionInput(VirtualViewport viewport)
    {
        UiRect bounds = CollectionsNewFolderBounds(viewport);
        _textInputService.RequestTextInput(new TextInputRequest(
            string.Empty,
            _ => { },
            CreateCollection,
            viewport.ToSurface(bounds),
            () => { }));
    }

    public void ToggleCollection(int visibleSlot)
    {
        BeatmapSetInfo? set = SelectedSet;
        BeatmapCollection? collection = CollectionAtVisibleSlot(visibleSlot);
        if (set is null || collection is null)
        {
            return;
        }

        library.ToggleCollectionMembership(collection.Name, set.Directory);
    }

    public void RequestDeleteCollection(int visibleSlot)
    {
        BeatmapCollection? collection = CollectionAtVisibleSlot(visibleSlot);
        if (collection is null)
        {
            return;
        }

        _collectionPendingDelete = collection.Name;
    }

    public void ConfirmDeleteCollection()
    {
        if (_collectionPendingDelete is null)
        {
            return;
        }

        library.DeleteCollection(_collectionPendingDelete);
        _collectionPendingDelete = null;
        collectionScrollY = Math.Clamp(collectionScrollY, 0f, MaxCollectionScroll(VirtualViewport.LegacyLandscape));
    }

    public void CancelDeleteCollection() => _collectionPendingDelete = null;
}
