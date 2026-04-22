using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class BeatmapDownloaderScene
{
    public void FocusSearch(VirtualViewport viewport)
    {
        isSearchFocused = true;
        textInputService.RequestTextInput(new TextInputRequest(
            query,
            text => query = text,
            SubmitSearch,
            viewport.ToSurface(SearchBounds(viewport)),
            () => isSearchFocused = false));
    }

    public void SubmitSearch(string text)
    {
        query = text;
        isSearchFocused = false;
        _ = SearchAsync(false);
    }

    public void Refresh() => _ = SearchAsync(false);

    public void ToggleFilters()
    {
        HideSearchInput();
        filtersOpen = !filtersOpen;
        sortDropdownOpen = false;
        statusDropdownOpen = false;
        mirrorsOpen = false;
    }

    public void ToggleMirrorSelector()
    {
        HideSearchInput();
        mirrorsOpen = !mirrorsOpen;
        filtersOpen = false;
        sortDropdownOpen = false;
        statusDropdownOpen = false;
    }

    public void SelectMirror(BeatmapMirrorKind nextMirror)
    {
        if (mirror == nextMirror)
        {
            mirrorsOpen = false;
            return;
        }

        mirror = nextMirror;
        mirrorsOpen = false;
        _ = SearchAsync(false);
    }

    public void ToggleSortDropdown()
    {
        HideSearchInput();
        sortDropdownOpen = !sortDropdownOpen;
        statusDropdownOpen = false;
        sortDropdownScroll = 0f;
    }

    public void SetSort(BeatmapMirrorSort nextSort)
    {
        sort = nextSort;
        sortDropdownOpen = false;
        _ = SearchAsync(false);
    }

    public void ToggleOrder()
    {
        order = order == BeatmapMirrorOrder.Ascending ? BeatmapMirrorOrder.Descending : BeatmapMirrorOrder.Ascending;
        _ = SearchAsync(false);
    }

    public void ToggleStatusDropdown()
    {
        HideSearchInput();
        statusDropdownOpen = !statusDropdownOpen;
        sortDropdownOpen = false;
        statusDropdownScroll = 0f;
    }

    public void SetStatus(BeatmapRankedStatus? nextStatus)
    {
        status = nextStatus;
        statusDropdownOpen = false;
        _ = SearchAsync(false);
    }

    public void SelectDetailsDifficulty(int index)
    {
        if (selectedSetIndex is not int setIndex || setIndex < 0 || setIndex >= sets.Count)
            return;

        if (index < 0 || index >= sets[setIndex].Beatmaps.Count)
            return;

        selectedDifficultyIndex = index;
    }

    public void SelectCard(int visibleSlot)
    {
        HideSearchInput();
        var index = visibleStartIndex + visibleSlot;
        if (index < 0 || index >= sets.Count)
            return;

        selectedSetIndex = index;
        selectedDifficultyIndex = 0;
    }

    public void CloseDetails() => selectedSetIndex = null;

    public void PreviewCard(int visibleSlot) => Preview(visibleStartIndex + visibleSlot);

    public void PreviewDetails()
    {
        if (selectedSetIndex is int index)
            Preview(index);
    }

    public void Download(int index, bool withVideo) => _ = DownloadAsync(index, withVideo);

    public void DownloadVisible(int visibleSlot, bool withVideo) => Download(visibleStartIndex + visibleSlot, withVideo);

    public void DownloadDetails(bool withVideo)
    {
        if (selectedSetIndex is int index)
            Download(index, withVideo);
    }

    public void CancelDownload() => downloadService.CancelActiveDownload();

    private void HideSearchInput()
    {
        isSearchFocused = false;
        textInputService.HideTextInput();
    }

    public void Scroll(float deltaY, VirtualViewport viewport)
    {
        if (sortDropdownOpen)
        {
            sortDropdownScroll = Math.Clamp(sortDropdownScroll + deltaY, 0f, MaxDropdownScroll(SortOptions().Length, viewport));
            return;
        }

        if (statusDropdownOpen)
        {
            statusDropdownScroll = Math.Clamp(statusDropdownScroll + deltaY, 0f, MaxDropdownScroll(StatusOptions().Length, viewport));
            return;
        }

        if (selectedSetIndex is not null || filtersOpen || mirrorsOpen)
            return;

        scrollOffset = Math.Clamp(scrollOffset + deltaY, 0f, MaxScrollOffset(viewport));
        if (hasMore && !isSearching && scrollOffset >= MaxScrollOffset(viewport) - 40f * Dp)
            _ = SearchAsync(true);
    }
}
