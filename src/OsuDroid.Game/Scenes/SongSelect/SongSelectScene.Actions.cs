using System.Globalization;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    public void SelectSet(int visibleSlot)
    {
        long start = PerfDiagnostics.Start();
        if (_propertiesOpen || _beatmapOptionsOpen || _collectionsOpen)
        {
            return;
        }

        if (visibleSlot < 0 || visibleSlot >= _visibleSetIndices.Length)
        {
            return;
        }

        int index = _visibleSetIndices[visibleSlot] >= 0 ? _visibleSetIndices[visibleSlot] : visibleSlot;
        if (index < 0 || index >= _visibleSnapshot.Sets.Count)
        {
            return;
        }

        SelectSetIndex(index);
        PerfDiagnostics.Log("songSelect.selectSet", start, $"slot={visibleSlot} index={index}");
    }

    public void SelectFirstSet() => SelectSet(0);

    public void SelectRandomSet()
    {
        long start = PerfDiagnostics.Start();
        if (_propertiesOpen || _beatmapOptionsOpen || _collectionsOpen)
        {
            return;
        }

        int count = _visibleSnapshot.Sets.Count;
        if (count <= 0)
        {
            return;
        }

        int index = 0;
        if (count > 1)
        {
            int roll = _randomIndexProvider(count - 1);
            index = ((roll % (count - 1)) + count - 1) % (count - 1);
            if (index >= selectedSetIndex)
            {
                index++;
            }
        }

        SelectSetIndex(index);
        PerfDiagnostics.Log("songSelect.selectRandomSet", start, $"index={index} count={count}");
    }

    public void SelectDifficulty(int index)
    {
        long start = PerfDiagnostics.Start();
        if (_propertiesOpen || _beatmapOptionsOpen || _collectionsOpen)
        {
            return;
        }

        if (index < 0 || index >= _visibleDifficultyIndices.Length)
        {
            return;
        }

        BeatmapSetInfo? set = SelectedSet;
        int difficultyIndex = _visibleDifficultyIndices[index] >= 0 ? _visibleDifficultyIndices[index] : index;
        if (set is null || difficultyIndex < 0 || difficultyIndex >= set.Beatmaps.Count)
        {
            return;
        }

        selectedDifficultyIndex = difficultyIndex;
        RefreshSelectedBackgroundPath();
        QueueVisibleDifficultyCalculations();
        PlaySelectedPreview();
        PerfDiagnostics.Log("songSelect.selectDifficulty", start, $"slot={index} difficulty={difficultyIndex}");
    }

    private void SelectSetIndex(int index)
    {
        selectedSetIndex = index;
        selectedSetExpansion = 0f;
        selectedDifficultyIndex = 0;
        scrollY = ClampScroll(CalculateSelectedSetScroll(index));
        RefreshSelectedBackgroundPath();
        QueueVisibleDifficultyCalculations();
        PlaySelectedPreview();
    }

    public void OpenProperties()
    {
        if (SelectedSet is null)
        {
            return;
        }

        _propertiesOpen = true;
        _beatmapOptionsOpen = false;
        _collectionsOpen = false;
        _collectionsFilterMode = false;
        _deleteBeatmapConfirmOpen = false;
        _collectionPendingDelete = null;
    }

    public void OpenBeatmapOptions()
    {
        if (_visibleSnapshot.Sets.Count == 0)
        {
            return;
        }

        _beatmapOptionsOpen = true;
        _propertiesOpen = false;
        _collectionsOpen = false;
        _collectionsFilterMode = false;
        _deleteBeatmapConfirmOpen = false;
        _collectionPendingDelete = null;
    }

    public void OpenPropertiesForDifficulty(int visibleSlot)
    {
        if (visibleSlot < 0 || visibleSlot >= _visibleDifficultyIndices.Length)
        {
            return;
        }

        BeatmapSetInfo? set = SelectedSet;
        int difficultyIndex = _visibleDifficultyIndices[visibleSlot] >= 0 ? _visibleDifficultyIndices[visibleSlot] : visibleSlot;
        if (set is null || difficultyIndex < 0 || difficultyIndex >= set.Beatmaps.Count)
        {
            return;
        }

        selectedDifficultyIndex = difficultyIndex;
        OpenProperties();
    }

    public void ClosePopups()
    {
        _propertiesOpen = false;
        _beatmapOptionsOpen = false;
        _collectionsOpen = false;
        _collectionsFilterMode = false;
        _deleteBeatmapConfirmOpen = false;
        _collectionPendingDelete = null;
        _textInputService.HideTextInput();
    }

    public void CloseCollections()
    {
        _collectionsOpen = false;
        _collectionsFilterMode = false;
        _collectionPendingDelete = null;
    }

    public void ToggleFavorite()
    {
        BeatmapOptions? options = CurrentOptions();
        if (options is null)
        {
            return;
        }

        library.SaveOptions(options with { IsFavorite = !options.IsFavorite });
    }

    public void AdjustOffset(int delta)
    {
        BeatmapOptions? options = CurrentOptions();
        if (options is null)
        {
            return;
        }

        library.SaveOptions(options with { Offset = Math.Clamp(options.Offset + delta, -250, 250) });
    }

    public void FocusOffsetInput(VirtualViewport viewport)
    {
        BeatmapOptions? options = CurrentOptions();
        UiRect bounds = PropertiesOffsetInputBounds(viewport);
        _textInputService.RequestTextInput(new TextInputRequest(
            (options?.Offset ?? 0).ToString(CultureInfo.InvariantCulture),
            SaveOffsetText,
            SaveOffsetText,
            viewport.ToSurface(bounds),
            () => { }));
    }


    public void RequestDeleteBeatmap()
    {
        if (SelectedSet is not null)
        {
            _deleteBeatmapConfirmOpen = true;
        }
    }

    public void CancelDeleteBeatmap() => _deleteBeatmapConfirmOpen = false;

    public void ConfirmDeleteBeatmap()
    {
        BeatmapSetInfo? set = SelectedSet;
        if (set is null)
        {
            return;
        }

        library.DeleteBeatmapSet(set.Directory);
        _snapshot = library.Load();
        ApplyBeatmapOptions();
        selectedSetIndex = _visibleSnapshot.Sets.Count == 0 ? -1 : Math.Clamp(selectedSetIndex, 0, _visibleSnapshot.Sets.Count - 1);
        selectedDifficultyIndex = 0;
        scrollY = ClampScroll(CalculateSelectedSetScroll(selectedSetIndex));
        RefreshSelectedBackgroundPath();
        ClosePopups();
        QueueVisibleDifficultyCalculations();
        PlaySelectedPreview();
    }

}
