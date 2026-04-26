using NUnit.Framework;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class BeatmapDifficultyServiceTests
{
    [Test]
    public void VersionResetRecalculatesNonNullInMemoryRatings()
    {
        using TestContextData context = CreateContext();
        BeatmapInfo stale = CreateBeatmap(1.11f, 2.22f);
        context.Repository.UpsertBeatmaps([stale]);
        context.Repository.SetDifficultyMetadata("droidStarRatingVersion", 1);
        context.Repository.SetDifficultyMetadata("standardStarRatingVersion", 1);
        WriteOsuFile(context.Paths, stale);
        var calculator = new FixedDifficultyCalculator(new BeatmapStarRatings(6.99f, 5.64f));
        var service = new BeatmapDifficultyService(
            context.Repository,
            context.Paths.Songs,
            calculator
        );

        BeatmapInfo updated = service.EnsureCalculated(stale);
        BeatmapInfo persisted = context.Repository.LoadLibrary().Sets.Single().Beatmaps.Single();

        Assert.Multiple(() =>
        {
            Assert.That(updated.DroidStarRating, Is.EqualTo(6.99f));
            Assert.That(updated.StandardStarRating, Is.EqualTo(5.64f));
            Assert.That(persisted.DroidStarRating, Is.EqualTo(6.99f));
            Assert.That(persisted.StandardStarRating, Is.EqualTo(5.64f));
            Assert.That(
                context.Repository.GetDifficultyMetadata("droidStarRatingVersion"),
                Is.EqualTo(BeatmapDifficultyService.DroidCalculatorVersion)
            );
            Assert.That(
                context.Repository.GetDifficultyMetadata("standardStarRatingVersion"),
                Is.EqualTo(BeatmapDifficultyService.StandardCalculatorVersion)
            );
            Assert.That(calculator.CallCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void CurrentVersionKeepsCachedRatings()
    {
        using TestContextData context = CreateContext();
        BeatmapInfo cached = CreateBeatmap(3.96f, 4.70f);
        context.Repository.UpsertBeatmaps([cached]);
        context.Repository.SetDifficultyMetadata(
            "droidStarRatingVersion",
            BeatmapDifficultyService.DroidCalculatorVersion
        );
        context.Repository.SetDifficultyMetadata(
            "standardStarRatingVersion",
            BeatmapDifficultyService.StandardCalculatorVersion
        );
        WriteOsuFile(context.Paths, cached);
        var calculator = new FixedDifficultyCalculator(new BeatmapStarRatings(6.99f, 5.64f));
        var service = new BeatmapDifficultyService(
            context.Repository,
            context.Paths.Songs,
            calculator
        );

        BeatmapInfo updated = service.EnsureCalculated(cached);
        BeatmapInfo persisted = context.Repository.LoadLibrary().Sets.Single().Beatmaps.Single();

        Assert.Multiple(() =>
        {
            Assert.That(updated.DroidStarRating, Is.EqualTo(3.96f));
            Assert.That(updated.StandardStarRating, Is.EqualTo(4.70f));
            Assert.That(persisted.DroidStarRating, Is.EqualTo(3.96f));
            Assert.That(persisted.StandardStarRating, Is.EqualTo(4.70f));
            Assert.That(calculator.CallCount, Is.Zero);
        });
    }

    private static TestContextData CreateContext()
    {
        string root = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"difficulty-service-{Guid.NewGuid():N}"
        );
        var paths = new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(root));
        paths.EnsureDirectories();
        var database = new DroidDatabase(paths.GetDatabasePath("test"));
        database.EnsureCreated();
        return new TestContextData(root, paths, new BeatmapLibraryRepository(database));
    }

    private static BeatmapInfo CreateBeatmap(float? droidStarRating, float? standardStarRating) =>
        new(
            Filename: "map.osu",
            SetDirectory: "Artist - Title",
            Md5: "abc123",
            Id: 123,
            AudioFilename: "audio.mp3",
            BackgroundFilename: null,
            Status: null,
            SetId: 12,
            Title: "Title",
            TitleUnicode: "Title",
            Artist: "Artist",
            ArtistUnicode: "Artist",
            Creator: "Creator",
            Version: "Insane",
            Tags: string.Empty,
            Source: string.Empty,
            DateImported: 0,
            ApproachRate: 9,
            OverallDifficulty: 8,
            CircleSize: 4,
            HpDrainRate: 6,
            DroidStarRating: droidStarRating,
            StandardStarRating: standardStarRating,
            BpmMax: 180,
            BpmMin: 180,
            MostCommonBpm: 180,
            Length: 120000,
            PreviewTime: 0,
            HitCircleCount: 1,
            SliderCount: 0,
            SpinnerCount: 0,
            MaxCombo: 1,
            EpilepsyWarning: false
        );

    private static void WriteOsuFile(DroidGamePathLayout paths, BeatmapInfo beatmap)
    {
        string setPath = Path.Combine(paths.Songs, beatmap.SetDirectory);
        Directory.CreateDirectory(setPath);
        File.WriteAllText(Path.Combine(setPath, beatmap.Filename), "osu file format v14");
    }

    private sealed class FixedDifficultyCalculator(BeatmapStarRatings ratings)
        : IBeatmapDifficultyCalculator
    {
        public int CallCount { get; private set; }

        public BeatmapStarRatings Calculate(string osuFilePath)
        {
            CallCount++;
            Assert.That(File.Exists(osuFilePath), Is.True);
            return ratings;
        }
    }

    private sealed class TestContextData(
        string root,
        DroidGamePathLayout paths,
        BeatmapLibraryRepository repository
    ) : IDisposable
    {
        public DroidGamePathLayout Paths { get; } = paths;

        public BeatmapLibraryRepository Repository { get; } = repository;

        public void Dispose()
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
