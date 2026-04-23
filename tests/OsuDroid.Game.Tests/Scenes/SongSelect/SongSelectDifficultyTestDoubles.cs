using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{
    private sealed class FakeDifficultyService : IBeatmapDifficultyService
    {
        public DifficultyAlgorithm Algorithm => DifficultyAlgorithm.Droid;

        public BeatmapInfo EnsureCalculated(BeatmapInfo beatmap) => beatmap;

        public void EnsureCalculatorVersions()
        {
        }
    }

    private sealed class UpdatingDifficultyService : IBeatmapDifficultyService
    {
        public DifficultyAlgorithm Algorithm => DifficultyAlgorithm.Droid;

        public BeatmapInfo EnsureCalculated(BeatmapInfo beatmap) => beatmap with
        {
            DroidStarRating = beatmap.DroidStarRating ?? 6.5f,
            StandardStarRating = beatmap.StandardStarRating ?? 6.6f,
        };

        public void EnsureCalculatorVersions()
        {
        }
    }
}
