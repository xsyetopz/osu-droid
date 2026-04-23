using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    public void FocusSearch(VirtualViewport viewport)
    {
        _isSearchFocused = true;
        _textInputService.RequestTextInput(new TextInputRequest(
            _query,
            text => _query = text,
            SubmitSearch,
            viewport.ToSurface(SearchBounds(viewport)),
            () => _isSearchFocused = false,
            _localizer["BeatmapDownloader_SearchPlaceholder"]));
    }

    public void SubmitSearch(string text)
    {
        _query = text;
        _isSearchFocused = false;
        _ = SearchAsync(false);
    }

    public void Refresh() => _ = SearchAsync(false);

    public void ToggleFilters()
    {
        HideSearchInput();
        _filtersOpen = !_filtersOpen;
        _sortDropdownOpen = false;
        _statusDropdownOpen = false;
        _mirrorsOpen = false;
    }

    public void ToggleMirrorSelector()
    {
        HideSearchInput();
        _mirrorsOpen = !_mirrorsOpen;
        _filtersOpen = false;
        _sortDropdownOpen = false;
        _statusDropdownOpen = false;
    }

    public void SelectMirror(BeatmapMirrorKind nextMirror)
    {
        if (_mirror == nextMirror)
        {
            _mirrorsOpen = false;
            return;
        }

        _mirror = nextMirror;
        _mirrorsOpen = false;
        _ = SearchAsync(false);
    }

    public void ToggleSortDropdown()
    {
        HideSearchInput();
        _sortDropdownOpen = !_sortDropdownOpen;
        _statusDropdownOpen = false;
        _sortDropdownScroll = 0f;
    }

    public void SetSort(BeatmapMirrorSort nextSort)
    {
        _sort = nextSort;
        _sortDropdownOpen = false;
        _ = SearchAsync(false);
    }

    public void ToggleOrder()
    {
        _order = _order == BeatmapMirrorOrder.Ascending ? BeatmapMirrorOrder.Descending : BeatmapMirrorOrder.Ascending;
        _ = SearchAsync(false);
    }

    public void ToggleStatusDropdown()
    {
        HideSearchInput();
        _statusDropdownOpen = !_statusDropdownOpen;
        _sortDropdownOpen = false;
        _statusDropdownScroll = 0f;
    }

    public void SetStatus(BeatmapRankedStatus? nextStatus)
    {
        _status = nextStatus;
        _statusDropdownOpen = false;
        _ = SearchAsync(false);
    }

    public void SelectDetailsDifficulty(int index)
    {
        if (_selectedSetIndex is not int setIndex || setIndex < 0 || setIndex >= _sets.Count)
        {
            return;
        }

        if (index < 0 || index >= _sets[setIndex].Beatmaps.Count)
        {
            return;
        }

        _selectedDifficultyIndex = index;
    }

    public void SelectCard(int visibleSlot)
    {
        HideSearchInput();
        int index = _visibleStartIndex + visibleSlot;
        if (index < 0 || index >= _sets.Count)
        {
            return;
        }

        _selectedSetIndex = index;
        _selectedDifficultyIndex = 0;
    }

    public void CloseDetails() => _selectedSetIndex = null;

    public void PreviewCard(int visibleSlot) => Preview(_visibleStartIndex + visibleSlot);

    public void PreviewDetails()
    {
        if (_selectedSetIndex is int index)
        {
            Preview(index);
        }
    }

    public void Download(int index, bool withVideo) => _ = DownloadAsync(index, withVideo);

    public void DownloadVisible(int visibleSlot, bool withVideo) => Download(_visibleStartIndex + visibleSlot, withVideo);

    public void DownloadDetails(bool withVideo)
    {
        if (_selectedSetIndex is int index)
        {
            Download(index, withVideo);
        }
    }

    public void CancelDownload() => _downloadService.CancelActiveDownload();

    private void HideSearchInput()
    {
        _isSearchFocused = false;
        _textInputService.HideTextInput();
    }

    public void Scroll(float deltaY, VirtualViewport viewport)
    {
        if (_sortDropdownOpen)
        {
            _sortDropdownScroll = Math.Clamp(_sortDropdownScroll + deltaY, 0f, MaxDropdownScroll(SortOptions().Length, viewport));
            return;
        }

        if (_statusDropdownOpen)
        {
            _statusDropdownScroll = Math.Clamp(_statusDropdownScroll + deltaY, 0f, MaxDropdownScroll(StatusOptions().Length, viewport));
            return;
        }

        if (_selectedSetIndex is not null || _filtersOpen || _mirrorsOpen)
        {
            return;
        }

        _scrollOffset = Math.Clamp(_scrollOffset + deltaY, 0f, MaxScrollOffset(viewport));
        if (_hasMore && !_isSearching && _scrollOffset >= MaxScrollOffset(viewport) - 40f * Dp)
        {
            _ = SearchAsync(true);
        }
    }
}
