using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Runtime.Diagnostics;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Input;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
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
}
