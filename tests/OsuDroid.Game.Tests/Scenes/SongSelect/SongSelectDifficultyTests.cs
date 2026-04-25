using System.Globalization;
using NUnit.Framework;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Scenes.SongSelect;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{

    [Test]
    public void DifficultySelectionChangesSelectedBeatmap()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.SelectDifficulty(1);

        Assert.That(scene.SelectedBeatmap?.Version, Is.EqualTo("Insane"));
    }
    [Test]
    public void DifficultiesAreSortedByDisplayedStarRating()
    {
        BeatmapInfo hard = CreateBeatmap("Hard", null, 3.4f);
        BeatmapInfo easy = CreateBeatmap("Easy", null, 1.2f);
        BeatmapInfo normal = CreateBeatmap("Normal", null, 2.3f);
        var scene = new SongSelectScene(
            new FakeLibrary(new BeatmapLibrarySnapshot([new BeatmapSetInfo(1, "1 Artist - Title", [hard, easy, normal])])),
            new NoOpMenuMusicController(),
            new FakeDifficultyService(),
            CreateSongsRoot("audio.mp3"));

        scene.Enter();

        Assert.That(scene.SelectedBeatmap?.Version, Is.EqualTo("Easy"));
        scene.SelectDifficulty(1);
        Assert.That(scene.SelectedBeatmap?.Version, Is.EqualTo("Normal"));
        scene.SelectDifficulty(2);
        Assert.That(scene.SelectedBeatmap?.Version, Is.EqualTo("Hard"));
    }
    [Test]
    public void DifficultyAlgorithmToggleResortsAndKeepsSelectedBeatmap()
    {
        BeatmapInfo easy = CreateBeatmap("Easy", null, 1.2f) with { StandardStarRating = 4.2f };
        BeatmapInfo hard = CreateBeatmap("Hard", null, 3.4f) with { StandardStarRating = 2.1f };
        var scene = new SongSelectScene(
            new FakeLibrary(new BeatmapLibrarySnapshot([new BeatmapSetInfo(1, "1 Artist - Title", [easy, hard])])),
            new NoOpMenuMusicController(),
            new FakeDifficultyService(),
            CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.SelectDifficulty(1);
        Assert.That(scene.SelectedBeatmap?.Version, Is.EqualTo("Hard"));

        scene.ToggleBeatmapOptionsAlgorithm();

        Assert.That(scene.SelectedBeatmap?.Version, Is.EqualTo("Hard"));
        scene.SelectDifficulty(1);
        Assert.That(scene.SelectedBeatmap?.Version, Is.EqualTo("Easy"));
    }
    [Test]
    public void BackgroundDifficultyCalculationUpdatesVisibleAndBackingSnapshots()
    {
        string songs = CreateSongsRoot("audio.mp3");
        BeatmapInfo easy = CreateBeatmap("Easy", null, 0f) with { DroidStarRating = null, StandardStarRating = null };
        BeatmapInfo insane = CreateBeatmap("Insane", null, 0f) with { DroidStarRating = null, StandardStarRating = null };
        var scene = new SongSelectScene(
            new FakeLibrary(new BeatmapLibrarySnapshot([new BeatmapSetInfo(1, "1 Artist - Title", [easy, insane])])),
            new NoOpMenuMusicController(),
            new UpdatingDifficultyService(),
            songs);

        scene.Enter();
        SpinUntil(() =>
        {
            scene.Update(TimeSpan.FromMilliseconds(16));
            return scene.SelectedBeatmap?.DroidStarRating is 6.5f;
        });
        scene.OpenBeatmapOptions();
        scene.SetBeatmapOptionsSearchQuery("Artist");
        scene.SetBeatmapOptionsSearchQuery(string.Empty);

        Assert.That(scene.SelectedBeatmap?.DroidStarRating, Is.EqualTo(6.5f));
        Assert.That(scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements
            .Single(element => element.Id == "songselect-difficulty").Text, Does.Contain("Stars: 6.5"));
    }

    [Test]
    public void DifficultyStatsUseInvariantDotDecimals()
    {
        CultureInfo previousCulture = CultureInfo.CurrentCulture;
        CultureInfo previousUiCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("et-EE");
            CultureInfo.CurrentUICulture = new CultureInfo("et-EE");
            BeatmapInfo beatmap = CreateBeatmap("Normal", null, 2.3f) with
            {
                ApproachRate = 5.5f,
                OverallDifficulty = 4.5f,
                CircleSize = 3.2f,
                HpDrainRate = 3f,
                DroidStarRating = 2.3f,
            };
            var scene = new SongSelectScene(
                new FakeLibrary(new BeatmapLibrarySnapshot([new BeatmapSetInfo(1, "1 Artist - Title", [beatmap])])),
                new NoOpMenuMusicController(),
                new FakeDifficultyService(),
                CreateSongsRoot("audio.mp3"));

            scene.Enter();
            string? text = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements
                .Single(element => element.Id == "songselect-difficulty")
                .Text;

            Assert.That(text, Does.Contain("AR: 5.5"));
            Assert.That(text, Does.Contain("OD: 4.5"));
            Assert.That(text, Does.Contain("CS: 3.2"));
            Assert.That(text, Does.Contain("HP: 3"));
            Assert.That(text, Does.Contain("Stars: 2.3"));
            Assert.That(text, Does.Not.Contain("5,5"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }
    [Test]
    public void DifficultyCalculatorProducesPersistableDroidAndStandardRatings()
    {
        string root = Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string osu = Path.Combine(root, "fixture.osu");
        File.WriteAllText(osu, """
            osu file format v14

            [General]
            AudioFilename: audio.mp3

            [Metadata]
            Title:Fixture
            Artist:Artist
            Creator:Mapper
            Version:Hard

            [Difficulty]
            HPDrainRate:5
            CircleSize:4
            OverallDifficulty:8
            ApproachRate:9

            [HitObjects]
            64,192,0,1,0,0:0:0:0:
            448,192,500,1,0,0:0:0:0:
            256,96,750,2,0,B|256:288|448:288,1,240
            64,192,1000,1,0,0:0:0:0:
            448,192,1250,1,0,0:0:0:0:
            """);

        BeatmapStarRatings ratings = new BeatmapDifficultyCalculator().Calculate(osu);

        Assert.That(ratings.Droid, Is.GreaterThan(0f));
        Assert.That(ratings.Standard, Is.GreaterThan(0f));
        Assert.That(ratings.Droid, Is.Not.EqualTo(ratings.Standard));
    }
    [Test]
    public void DifficultyCalculatorMatchesReferenceComRianOsuFixture()
    {
        string osu = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "third_party",
            "osu-droid-legacy",
            "tests",
            "test",
            "resources",
            "beatmaps",
            "YOASOBI - Love Letter (ohm002) [Please accept my overflowing emotions.].osu");

        BeatmapStarRatings ratings = new BeatmapDifficultyCalculator().Calculate(Path.GetFullPath(osu));

        Assert.That(ratings.Droid, Is.EqualTo(3.86f).Within(0.000001f));
        Assert.That(ratings.Standard, Is.EqualTo(4.70f).Within(0.000001f));
    }
}
