using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;
using System.Globalization;

namespace OsuDroid.Game.Scenes;

public sealed partial class SongSelectScene
{
    private void QueueVisibleDifficultyCalculations()
    {
        var start = PerfDiagnostics.Start();
        var selected = SelectedBeatmap;
        var beatmaps = EnumerateDifficultyCalculationCandidates()
            .OrderByDescending(beatmap => selected is not null && BeatmapMatches(beatmap, selected))
            .ThenByDescending(beatmap => SelectedSet is not null && string.Equals(beatmap.SetDirectory, SelectedSet.Directory, StringComparison.Ordinal))
            .Where(NeedsDifficultyCalculation)
            .Where(TrackPendingDifficulty)
            .ToArray();
        if (beatmaps.Length == 0)
        {
            PerfDiagnostics.Log("songSelect.queueDifficulty", start, "queued=0");
            return;
        }

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
        PerfDiagnostics.Log("songSelect.queueDifficulty", start, $"queued={beatmaps.Length} sets={visibleSnapshot.Sets.Count}");
    }

    private IEnumerable<BeatmapInfo> EnumerateDifficultyCalculationCandidates()
    {
        var set = SelectedSet;
        if (set is not null)
        {
            foreach (var beatmap in set.Beatmaps)
                yield return beatmap;
        }

        if (visibleSnapshot.Sets.Count == 0)
            yield break;

        var start = Math.Max(0, selectedSetIndex - 4);
        var end = Math.Min(visibleSnapshot.Sets.Count - 1, selectedSetIndex + 4);
        for (var index = start; index <= end; index++)
        {
            if (index == selectedSetIndex)
                continue;

            foreach (var beatmap in visibleSnapshot.Sets[index].Beatmaps)
                yield return beatmap;
        }
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
        var start = PerfDiagnostics.Start();
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
            !string.IsNullOrWhiteSpace(audioPath));
        PerfDiagnostics.Log("songSelect.playPreview", start, $"set=\"{set.Directory}\" file=\"{beatmap.Filename}\"");
    }

    private void RefreshSelectedBackgroundPath()
    {
        var beatmap = SelectedBeatmap;
        var key = beatmap is null ? null : $"{beatmap.SetDirectory}/{beatmap.Filename}";
        if (string.Equals(selectedBackgroundBeatmapKey, key, StringComparison.Ordinal))
            return;

        var nextPath = beatmap?.GetBackgroundPath(songsPath);
        selectedBackgroundBeatmapKey = key;
        if (string.Equals(selectedBackgroundPath, nextPath, StringComparison.Ordinal))
            return;

        selectedBackgroundPath = nextPath;
        selectedBackgroundLuminance = nextPath is null ? 1f : 0f;
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
        var stars = CurrentStarRating(beatmap) is float value ? FormatStatNumber(value) : "...";
        return $"AR: {FormatStatNumber(beatmap.ApproachRate)} OD: {FormatStatNumber(beatmap.OverallDifficulty)} CS: {FormatStatNumber(beatmap.CircleSize)} HP: {FormatStatNumber(beatmap.HpDrainRate)} Stars: {stars}";
    }

    private static string FormatStatNumber(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
