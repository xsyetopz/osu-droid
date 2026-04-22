using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

public sealed partial class SongSelectScene
{
    public void SelectSet(int visibleSlot)
    {
        var start = PerfDiagnostics.Start();
        if (propertiesOpen || beatmapOptionsOpen || collectionsOpen)
            return;

        if (visibleSlot < 0 || visibleSlot >= visibleSetActions.Length)
            return;

        var index = visibleSetActions[visibleSlot] >= 0 ? visibleSetActions[visibleSlot] : visibleSlot;
        if (index < 0 || index >= visibleSnapshot.Sets.Count)
            return;

        SelectSetIndex(index);
        PerfDiagnostics.Log("songSelect.selectSet", start, $"slot={visibleSlot} index={index}");
    }

    public void SelectFirstSet() => SelectSet(0);

    public void SelectRandomSet()
    {
        var start = PerfDiagnostics.Start();
        if (propertiesOpen || beatmapOptionsOpen || collectionsOpen)
            return;

        var count = visibleSnapshot.Sets.Count;
        if (count <= 0)
            return;

        var index = 0;
        if (count > 1)
        {
            var roll = randomIndexProvider(count - 1);
            index = ((roll % (count - 1)) + count - 1) % (count - 1);
            if (index >= selectedSetIndex)
                index++;
        }

        SelectSetIndex(index);
        PerfDiagnostics.Log("songSelect.selectRandomSet", start, $"index={index} count={count}");
    }

    public void SelectDifficulty(int index)
    {
        var start = PerfDiagnostics.Start();
        if (propertiesOpen || beatmapOptionsOpen || collectionsOpen)
            return;

        if (index < 0 || index >= visibleDifficultyActions.Length)
            return;

        var set = SelectedSet;
        var difficultyIndex = visibleDifficultyActions[index] >= 0 ? visibleDifficultyActions[index] : index;
        if (set is null || difficultyIndex < 0 || difficultyIndex >= set.Beatmaps.Count)
            return;

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
        RefreshSelectedBackgroundPath();
        ClosePopups();
        QueueVisibleDifficultyCalculations();
        PlaySelectedPreview();
    }

}
