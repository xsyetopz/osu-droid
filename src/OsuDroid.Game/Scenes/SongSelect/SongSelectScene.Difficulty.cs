using System.Globalization;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    private void QueueVisibleDifficultyCalculations()
    {
        long start = PerfDiagnostics.Start();
        BeatmapInfo? selected = SelectedBeatmap;
        BeatmapInfo[] beatmaps = EnumerateDifficultyCalculationCandidates()
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
            foreach (BeatmapInfo? beatmap in beatmaps)
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

                lock (_difficultyGate)
                {
                    _pendingDifficultyKeys.Remove(DifficultyKey(beatmap));
                    _completedDifficultyUpdates.Enqueue(updated);
                }
            }
        });
        PerfDiagnostics.Log("songSelect.queueDifficulty", start, $"queued={beatmaps.Length} sets={_visibleSnapshot.Sets.Count}");
    }

    private IEnumerable<BeatmapInfo> EnumerateDifficultyCalculationCandidates()
    {
        BeatmapSetInfo? set = SelectedSet;
        if (set is not null)
        {
            foreach (BeatmapInfo beatmap in set.Beatmaps)
            {
                yield return beatmap;
            }
        }

        if (_visibleSnapshot.Sets.Count == 0)
        {
            yield break;
        }

        int start = Math.Max(0, selectedSetIndex - 4);
        int end = Math.Min(_visibleSnapshot.Sets.Count - 1, selectedSetIndex + 4);
        for (int index = start; index <= end; index++)
        {
            if (index == selectedSetIndex)
            {
                continue;
            }

            foreach (BeatmapInfo beatmap in _visibleSnapshot.Sets[index].Beatmaps)
            {
                yield return beatmap;
            }
        }
    }

    private bool TrackPendingDifficulty(BeatmapInfo beatmap)
    {
        lock (_difficultyGate)
        {
            return _pendingDifficultyKeys.Add(DifficultyKey(beatmap));
        }
    }

    private bool NeedsDifficultyCalculation(BeatmapInfo beatmap) =>
        beatmap.DroidStarRating is null || beatmap.StandardStarRating is null;

    private void ApplyCompletedDifficultyUpdates()
    {
        bool applied = false;
        while (true)
        {
            BeatmapInfo updated;
            lock (_difficultyGate)
            {
                if (_completedDifficultyUpdates.Count == 0)
                {
                    if (applied && sortMode is SongSelectSortMode.DroidStars or SongSelectSortMode.StandardStars)
                    {
                        ApplyBeatmapOptions();
                    }

                    return;
                }

                updated = _completedDifficultyUpdates.Dequeue();
            }

            _snapshot = ReplaceBeatmap(_snapshot, updated);
            BeatmapInfo? selected = SelectedBeatmap;
            _visibleSnapshot = SortDifficultyRows(ReplaceBeatmap(_visibleSnapshot, updated));
            RestoreSelectedDifficulty(selected);
            applied = true;
        }
    }

    private void StartBackgroundLibraryRefresh()
    {
        lock (_libraryRefreshGate)
        {
            if (_libraryRefreshTask is { IsCompleted: false })
            {
                return;
            }

            _libraryRefreshTask = Task.Run(() =>
            {
                try
                {
                    lock (_libraryRefreshGate)
                    {
                        _completedLibraryRefresh = library.Scan();
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
        lock (_libraryRefreshGate)
        {
            refreshed = _completedLibraryRefresh;
            _completedLibraryRefresh = null;
        }

        if (refreshed is null)
        {
            return;
        }

        _snapshot = refreshed;
        ApplyBeatmapOptions();
        QueueVisibleDifficultyCalculations();
    }

    private static BeatmapLibrarySnapshot ReplaceBeatmap(BeatmapLibrarySnapshot source, BeatmapInfo updated)
    {
        if (source.Sets.Count == 0)
        {
            return source;
        }

        bool changed = false;
        BeatmapSetInfo[] sets = source.Sets.Select(set =>
        {
            BeatmapInfo[] beatmaps = set.Beatmaps.ToArray();
            int index = Array.FindIndex(beatmaps, beatmap => BeatmapMatches(beatmap, updated));
            if (index < 0)
            {
                return set;
            }

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
        return source.Sets.Count == 0
            ? source
            : new BeatmapLibrarySnapshot(source.Sets
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
        BeatmapSetInfo? set = SelectedSet;
        if (set is null || set.Beatmaps.Count == 0)
        {
            selectedDifficultyIndex = 0;
            return;
        }

        if (selected is not null)
        {
            int index = set.Beatmaps.ToList().FindIndex(beatmap => BeatmapMatches(beatmap, selected));
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
        long start = PerfDiagnostics.Start();
        BeatmapSetInfo? set = SelectedSet;
        BeatmapInfo? beatmap = SelectedBeatmap;
        if (set is null || beatmap is null)
        {
            return;
        }

        string audioPath = beatmap.GetAudioPath(songsPath);
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
        BeatmapInfo? beatmap = SelectedBeatmap;
        string? key = beatmap is null ? null : $"{beatmap.SetDirectory}/{beatmap.Filename}";
        if (string.Equals(selectedBackgroundBeatmapKey, key, StringComparison.Ordinal))
        {
            return;
        }

        string? nextPath = beatmap?.GetBackgroundPath(songsPath);
        selectedBackgroundBeatmapKey = key;
        if (string.Equals(selectedBackgroundPath, nextPath, StringComparison.Ordinal))
        {
            return;
        }

        selectedBackgroundPath = nextPath;
        selectedBackgroundLuminance = nextPath is null ? 1f : 0f;
    }

    private float? CurrentStarRating(BeatmapInfo beatmap) => _displayAlgorithm == DifficultyAlgorithm.Standard ? beatmap.StandardStarRating : beatmap.DroidStarRating;

    private static float CalculateRowX(float centerY, VirtualViewport viewport) => viewport.VirtualWidth / 1.85f + 200f * MathF.Abs(MathF.Cos(centerY * MathF.PI / (viewport.VirtualHeight * 2f)));

    private string DisplayTitle(BeatmapInfo beatmap) => _forceRomanizedMetadata || string.IsNullOrWhiteSpace(beatmap.TitleUnicode) ? beatmap.Title : beatmap.TitleUnicode;

    private string DisplayArtist(BeatmapInfo beatmap) => _forceRomanizedMetadata || string.IsNullOrWhiteSpace(beatmap.ArtistUnicode) ? beatmap.Artist : beatmap.ArtistUnicode;

    private string FormatLengthLine(BeatmapInfo beatmap)
    {
        string bpm = Math.Abs(beatmap.BpmMax - beatmap.BpmMin) < 0.01f
            ? beatmap.MostCommonBpm.ToString("0", CultureInfo.InvariantCulture)
            : string.Create(CultureInfo.InvariantCulture, $"{beatmap.BpmMin:0}-{beatmap.BpmMax:0} ({beatmap.MostCommonBpm:0})");
        return _localizer.Format("SongSelect_DifficultyStats", TimeSpan.FromMilliseconds(beatmap.Length).ToString("m\\:ss", CultureInfo.InvariantCulture), bpm, beatmap.MaxCombo);
    }

    private string FormatObjectLine(BeatmapInfo beatmap) =>
        _localizer.Format(
            "SongSelect_ObjectStats",
            beatmap.HitCircleCount,
            beatmap.SliderCount,
            beatmap.SpinnerCount,
            beatmap.SetId?.ToString(CultureInfo.InvariantCulture) ?? "0");

    private string FormatDifficultyLine(BeatmapInfo beatmap)
    {
        string stars = CurrentStarRating(beatmap) is float value ? FormatStatNumber(value) : "...";
        return _localizer.Format(
            "SongSelect_DifficultyAdvancedStats",
            FormatStatNumber(beatmap.ApproachRate),
            FormatStatNumber(beatmap.OverallDifficulty),
            FormatStatNumber(beatmap.CircleSize),
            FormatStatNumber(beatmap.HpDrainRate),
            stars);
    }

    private static string FormatStatNumber(float value) => value.ToString("0.##", CultureInfo.InvariantCulture);
}
