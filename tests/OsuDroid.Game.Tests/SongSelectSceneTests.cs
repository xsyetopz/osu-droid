using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class SongSelectSceneTests
{
    [Test]
    public void EnterQueuesSelectedBeatmapPreview()
    {
        var songs = CreateSongsRoot("audio.mp3");
        var controller = new PreviewMenuMusicController(new NoOpBeatmapPreviewPlayer());
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), controller, new FakeDifficultyService(), songs);

        scene.Enter();

        Assert.That(controller.State.ArtistTitle, Is.EqualTo("Artist - Title"));
        Assert.That(controller.State.IsPlaying, Is.False);
    }

    [Test]
    public void EnterDoesNotSynchronouslyScanEmptyLibrary()
    {
        var library = new FakeLibrary(BeatmapLibrarySnapshot.Empty);
        library.ScanDelay = TimeSpan.FromMilliseconds(500);
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));
        var start = DateTime.UtcNow;

        scene.Enter();

        Assert.That(DateTime.UtcNow - start, Is.LessThan(TimeSpan.FromMilliseconds(150)));
    }

    [Test]
    public void EnterStartsBackgroundRefreshWhenLibraryIndexIsStale()
    {
        var library = new FakeLibrary(CreateSnapshot())
        {
            NeedsRefresh = true,
        };
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        SpinUntil(() => library.ScanCallCount > 0);

        Assert.That(library.ScanCallCount, Is.GreaterThan(0));
    }

    [Test]
    public void PreviewUsesClampedEffectivePreviewTime()
    {
        var songs = CreateSongsRoot("audio.mp3");
        var player = new ConfirmingPreviewPlayer();
        var controller = new PreviewMenuMusicController(player);
        var beatmap = CreateBeatmap("Easy", null, 1.5f) with { PreviewTime = 999999, Length = 123000 };
        var scene = new SongSelectScene(new FakeLibrary(new BeatmapLibrarySnapshot([new BeatmapSetInfo(1, "1 Artist - Title", [beatmap])])), controller, new FakeDifficultyService(), songs);

        scene.Enter();

        Assert.That(player.PositionMilliseconds, Is.EqualTo(123000));
    }

    [Test]
    public void EnterMarksPreviewPlayingOnlyAfterPlayerConfirmsPlayback()
    {
        var songs = CreateSongsRoot("audio.mp3");
        var controller = new PreviewMenuMusicController(new ConfirmingPreviewPlayer());
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), controller, new FakeDifficultyService(), songs);

        scene.Enter();

        Assert.That(controller.State.ArtistTitle, Is.EqualTo("Artist - Title"));
        Assert.That(controller.State.IsPlaying, Is.True);
    }

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
        var hard = CreateBeatmap("Hard", null, 3.4f);
        var easy = CreateBeatmap("Easy", null, 1.2f);
        var normal = CreateBeatmap("Normal", null, 2.3f);
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
        var easy = CreateBeatmap("Easy", null, 1.2f) with { StandardStarRating = 4.2f };
        var hard = CreateBeatmap("Hard", null, 3.4f) with { StandardStarRating = 2.1f };
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
        var songs = CreateSongsRoot("audio.mp3");
        var easy = CreateBeatmap("Easy", null, 0f) with { DroidStarRating = null, StandardStarRating = null };
        var insane = CreateBeatmap("Insane", null, 0f) with { DroidStarRating = null, StandardStarRating = null };
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
    public void SnapshotUsesCoverBackgroundAndStarColoredDifficultyButtons()
    {
        var songs = CreateSongsRoot("audio.mp3", "bg.jpg");
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot("bg.jpg")), new NoOpMenuMusicController(), new FakeDifficultyService(), songs);

        scene.Enter();
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "songselect-beatmap-background" && element.SpriteFit == UiSpriteFit.Cover), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-diff-row-0"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-diff-row-0-star-0" && element.AssetName == DroidAssets.SongSelectStar), Is.True);
        var fractionalStar = frame.Elements.Single(element => element.Id == "songselect-diff-row-0-star-half");
        Assert.That(fractionalStar.Bounds.Width, Is.EqualTo(46f * 0.4f).Within(0.01f));
        Assert.That(fractionalStar.Bounds.Height, Is.EqualTo(47f));
        Assert.That(fractionalStar.SpriteSource, Is.Not.Null);
        Assert.That(fractionalStar.SpriteSource!.Value.Width, Is.EqualTo(46f * 0.4f).Within(0.01f));
        Assert.That(fractionalStar.SpriteSource.Value.Height, Is.EqualTo(47f));
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-detail-panel"), Is.False);
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-top-overlay").Bounds.X, Is.EqualTo(-1640f));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-top-overlay").Alpha, Is.EqualTo(0.6f));
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-top-line"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-back" && element.AssetName == DroidAssets.SongSelectBack), Is.True);
        Assert.That(frame.Elements.Any(element => element.Text == "xsytpz2319"), Is.False);
    }

    [Test]
    public void BottomControlsUseDroidSkinSprites()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "songselect-back").AssetName, Is.EqualTo(DroidAssets.SongSelectBack));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-mods").AssetName, Is.EqualTo(DroidAssets.SongSelectMods));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-options").AssetName, Is.EqualTo(DroidAssets.SongSelectOptions));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-random").AssetName, Is.EqualTo(DroidAssets.SongSelectRandom));
        Assert.That(frame.Elements.Any(element => element.Text is "Mods" or "Beatmap Options" or "Random"), Is.False);
    }

    [Test]
    public void OnlineScorePanelUsesLegacyBoundsAndOfflineDefaults()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        var mainMenuFrame = new MainMenuScene().CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        var mainMenuPanel = mainMenuFrame.Elements.Single(element => element.Id == "profile-panel");
        var mainMenuAvatarFooter = mainMenuFrame.Elements.Single(element => element.Id == "profile-avatar-footer");
        var scorePanel = frame.Elements.Single(element => element.Id == "songselect-score-panel");
        var scoreAvatarFooter = frame.Elements.Single(element => element.Id == "songselect-score-avatar-footer");

        Assert.That(scorePanel.Bounds, Is.EqualTo(new UiRect(540.5f, 610f, 410f, 110f)));
        Assert.That(scorePanel.Alpha, Is.EqualTo(mainMenuPanel.Alpha));
        Assert.That(scorePanel.Color, Is.EqualTo(mainMenuPanel.Color));
        Assert.That(scoreAvatarFooter.Bounds, Is.EqualTo(new UiRect(540.5f, 610f, 110f, 110f)));
        Assert.That(scoreAvatarFooter.Alpha, Is.EqualTo(mainMenuAvatarFooter.Alpha));
        Assert.That(scoreAvatarFooter.Color, Is.EqualTo(mainMenuAvatarFooter.Color));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-score-player").Text, Is.EqualTo("Guest"));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-score-avatar").AssetName, Is.EqualTo(DroidAssets.EmptyAvatar));
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-score-pp"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-score-acc"), Is.False);
    }

    [Test]
    public void LoggedInScorePanelShowsPerformanceAndAccuracy()
    {
        var profile = new OnlineProfileSnapshot("Player", DroidAssets.EmptyAvatar, PerformancePoints: 12345, Accuracy: 98.76f);
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"), profile);

        scene.Enter();
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "songselect-score-player").Text, Is.EqualTo("Player"));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-score-pp").Text, Does.Contain("12,345pp"));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-score-acc").Text, Is.EqualTo("98.76%"));
    }

    [Test]
    public void ScoringSwitcherStaysDisabledUntilOnlineScoringIsEnabled()
    {
        var rankedBeatmap = CreateBeatmap("Insane", null, 4.8f, 1);
        var snapshot = new BeatmapLibrarySnapshot([new BeatmapSetInfo(1, "1 Artist - Title", [rankedBeatmap])]);
        var scene = new SongSelectScene(new FakeLibrary(snapshot), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "songselect-scoring-switcher").AssetName, Is.EqualTo(DroidAssets.RankingDisabled));
    }

    [Test]
    public void SelectedSetUsesLegacyExpandedRowSpacingAndCentering()
    {
        var firstSet = new BeatmapSetInfo(1, "1 First", [CreateBeatmap("Easy", null, 1.5f)]);
        var secondSet = new BeatmapSetInfo(2, "2 Second", [
            CreateBeatmap("Easy", null, 1.5f, setId: 2, setDirectory: "2 Second"),
            CreateBeatmap("Normal", null, 2.5f, setId: 2, setDirectory: "2 Second"),
            CreateBeatmap("Hard", null, 3.5f, setId: 2, setDirectory: "2 Second"),
            CreateBeatmap("Insane", null, 4.5f, setId: 2, setDirectory: "2 Second"),
        ]);
        var scene = new SongSelectScene(new FakeLibrary(new BeatmapLibrarySnapshot([firstSet, secondSet])), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.SelectSet(1);
        scene.Update(TimeSpan.FromSeconds(0.5));
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "songselect-diff-row-0").Bounds.Y, Is.EqualTo(156f).Within(0.01f));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-diff-row-1").Bounds.Y, Is.EqualTo(258f).Within(0.01f));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-diff-row-2").Bounds.Y, Is.EqualTo(360f).Within(0.01f));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-diff-row-3").Bounds.Y, Is.EqualTo(462f).Within(0.01f));
    }

    [Test]
    public void BeatmapOptionsButtonOpensSearchOptionsPopup()
    {
        var library = new FakeLibrary(CreateSnapshot());
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.Enter();
        scene.OpenBeatmapOptions();

        var frame = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "songselect-properties-panel"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-beatmap-options-search"), Is.True);
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-beatmap-options-search-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Search));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-beatmap-options-favorite-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.HeartOutline));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-beatmap-options-algorithm-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Star));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-beatmap-options-sort-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Sort));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-beatmap-options-folder-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Folder));
    }

    [Test]
    public void BeatmapOptionsUsesLegacyRoundedContainerGraphics()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));
        var radius = 14f * DroidUiMetrics.DpScale;

        scene.Enter();
        scene.OpenBeatmapOptions();

        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        var search = frame.Elements.Single(element => element.Id == "songselect-beatmap-options-search");
        var strip = frame.Elements.Single(element => element.Id == "songselect-beatmap-options-strip");
        var hitFills = frame.Elements.Where(element => element.Id.StartsWith("songselect-beatmap-options-", StringComparison.Ordinal) && element.Id.EndsWith("-hit", StringComparison.Ordinal)).ToArray();

        Assert.That(search.CornerRadius, Is.EqualTo(radius));
        Assert.That(search.Color, Is.EqualTo(UiColor.Opaque(54, 54, 83)));
        Assert.That(strip.CornerRadius, Is.EqualTo(radius));
        Assert.That(strip.Color, Is.EqualTo(UiColor.Opaque(30, 30, 46)));
        Assert.That(hitFills, Has.All.Property(nameof(UiElementSnapshot.Alpha)).EqualTo(0f));
        Assert.That(frame.Elements.Count(element => element.Id.StartsWith("songselect-beatmap-options-divider-", StringComparison.Ordinal)), Is.EqualTo(3));
    }

    [Test]
    public void LongPressDifficultyOpensPropertiesForThatDifficulty()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.OpenPropertiesForDifficulty(1);

        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(scene.SelectedBeatmap?.Version, Is.EqualTo("Insane"));
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-properties-panel"), Is.True);
    }

    [Test]
    public void PropertiesPopupUsesMaterialIconsInsteadOfTextGlyphs()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.OpenProperties();

        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "songselect-properties-favorite-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.HeartOutline));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-properties-manage-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Folder));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-properties-delete-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Delete));
        Assert.That(frame.Elements.Where(element => element.Text is not null).Select(element => element.Text), Has.None.Contains("☐ Add to Favorites"));
        Assert.That(frame.Elements.Where(element => element.Text is not null).Select(element => element.Text), Has.None.Contains("▣ Manage Favorites"));
        Assert.That(frame.Elements.Where(element => element.Text is not null).Select(element => element.Text), Has.None.Contains("⌫ Delete beatmap"));
    }

    [Test]
    public void BeatmapOptionsSearchAndFavoriteFilterVisibleSets()
    {
        var first = new BeatmapSetInfo(1, "1 Artist - Title", [CreateBeatmap("Easy", null, 1.5f, setDirectory: "1 Artist - Title")]);
        var second = new BeatmapSetInfo(2, "2 Other - Song", [CreateBeatmap("Normal", null, 2.5f, setId: 2, setDirectory: "2 Other - Song", title: "Song", artist: "Other")]);
        var library = new FakeLibrary(new BeatmapLibrarySnapshot([first, second]));
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        library.SaveOptions(new BeatmapOptions("2 Other - Song", IsFavorite: true));
        scene.Enter();
        scene.OpenBeatmapOptions();
        scene.SetBeatmapOptionsSearchQuery("Other");
        scene.ToggleBeatmapOptionsFavoriteOnly();

        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Text?.Contains("Artist - Title", StringComparison.Ordinal) == true), Is.False);
        Assert.That(frame.Elements.Any(element => element.Text?.Contains("Other - Song", StringComparison.Ordinal) == true), Is.True);
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-beatmap-options-favorite-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Heart));
    }

    [Test]
    public void FolderFilterPopupUsesLegacyBottomAnchoredCollectionsPanel()
    {
        var library = new FakeLibrary(CreateSnapshot());
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var dp = DroidUiMetrics.DpScale;

        library.CreateCollection("Folder");
        scene.Enter();
        scene.OpenBeatmapOptions();
        scene.OpenCollectionFilter();

        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var panel = frame.Elements.Single(element => element.Id == "songselect-collections-panel");

        Assert.That(panel.Bounds.Width, Is.EqualTo(500f * dp));
        Assert.That(panel.Bounds.Y, Is.EqualTo(20f * dp).Within(0.001f));
        Assert.That(panel.Bounds.Bottom, Is.EqualTo(viewport.VirtualHeight - 20f * dp).Within(0.001f));
        Assert.That(panel.CornerRadius, Is.EqualTo(14f * dp));
        Assert.That(panel.Color, Is.EqualTo(UiColor.Opaque(19, 19, 26)));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-collections-new").Text, Is.EqualTo("New folder"));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-collection-0-name").Text, Is.EqualTo("All folders"));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-collection-1-name").Text, Is.EqualTo("Folder"));
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-collection-0-delete"), Is.False);
    }

    [Test]
    public void DifficultyCalculatorProducesPersistableDroidAndStandardRatings()
    {
        var root = Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var osu = Path.Combine(root, "fixture.osu");
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

        var ratings = new BeatmapDifficultyCalculator().Calculate(osu);

        Assert.That(ratings.Droid, Is.GreaterThan(0f));
        Assert.That(ratings.Standard, Is.GreaterThan(0f));
        Assert.That(ratings.Droid, Is.Not.EqualTo(ratings.Standard));
    }

    [Test]
    public void DifficultyCalculatorMatchesLegacyComRianOsuFixture()
    {
        var osu = Path.Combine(
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

        var ratings = new BeatmapDifficultyCalculator().Calculate(Path.GetFullPath(osu));

        Assert.That(ratings.Droid, Is.EqualTo(3.8554857722148643f).Within(0.000001f));
        Assert.That(ratings.Standard, Is.EqualTo(4.552663607000551f).Within(0.000001f));
    }

    [Test]
    public void PropertiesOffsetAndFavoritePersist()
    {
        var library = new FakeLibrary(CreateSnapshot());
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.OpenProperties();
        scene.AdjustOffset(1);
        scene.AdjustOffset(300);
        scene.ToggleFavorite();

        var options = library.GetOptions("1 Artist - Title");

        Assert.That(options.Offset, Is.EqualTo(250));
        Assert.That(options.IsFavorite, Is.True);
    }

    [Test]
    public void CollectionsCanBeCreatedToggledAndDeleted()
    {
        var library = new FakeLibrary(CreateSnapshot());
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.OpenProperties();
        scene.OpenCollections();
        Assert.That(library.CreateCollection("Folder"), Is.True);
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-collection-0-toggle"), Is.True);

        scene.ToggleCollection(0);
        Assert.That(library.GetCollections("1 Artist - Title").Single().ContainsSelectedSet, Is.True);

        scene.RequestDeleteCollection(0);
        scene.ConfirmDeleteCollection();
        Assert.That(library.GetCollections("1 Artist - Title"), Is.Empty);
    }

    [Test]
    public void DeleteBeatmapRemovesSelectedSet()
    {
        var library = new FakeLibrary(CreateSnapshot());
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.OpenProperties();
        scene.RequestDeleteBeatmap();
        scene.ConfirmDeleteBeatmap();

        Assert.That(library.Snapshot.Sets, Is.Empty);
        Assert.That(scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.Any(element => element.Id == "songselect-properties-panel"), Is.False);
    }

    private static BeatmapLibrarySnapshot CreateSnapshot(string? background = null)
    {
        var easy = CreateBeatmap("Easy", background, 2.4f);
        var insane = CreateBeatmap("Insane", background, 4.8f);
        return new BeatmapLibrarySnapshot([new BeatmapSetInfo(1, "1 Artist - Title", [easy, insane])]);
    }

    private static BeatmapInfo CreateBeatmap(string version, string? background, float stars, int? status = null, int setId = 1, string setDirectory = "1 Artist - Title", string title = "Title", string artist = "Artist") => new(
        Filename: version + ".osu",
        SetDirectory: setDirectory,
        Md5: $"{setDirectory}:{version}",
        Id: null,
        AudioFilename: "audio.mp3",
        BackgroundFilename: background,
        Status: status,
        SetId: setId,
        Title: title,
        TitleUnicode: string.Empty,
        Artist: artist,
        ArtistUnicode: string.Empty,
        Creator: "Mapper",
        Version: version,
        Tags: string.Empty,
        Source: string.Empty,
        DateImported: 0,
        ApproachRate: 7,
        OverallDifficulty: 6,
        CircleSize: 4,
        HpDrainRate: 5,
        DroidStarRating: stars,
        StandardStarRating: stars + 0.1f,
        BpmMax: 180,
        BpmMin: 180,
        MostCommonBpm: 180,
        Length: 123000,
        PreviewTime: 45000,
        HitCircleCount: 100,
        SliderCount: 50,
        SpinnerCount: 1,
        MaxCombo: 200,
        EpilepsyWarning: false);

    private static string CreateSongsRoot(params string[] files)
    {
        var root = Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString("N"));
        var set = Path.Combine(root, "1 Artist - Title");
        Directory.CreateDirectory(set);
        foreach (var file in files)
            File.WriteAllBytes(Path.Combine(set, file), [1]);
        return root;
    }

    private static void SpinUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
                return;

            Thread.Sleep(10);
        }

        Assert.Fail("Condition was not met.");
    }

    private sealed class FakeLibrary(BeatmapLibrarySnapshot snapshot) : IBeatmapLibrary
    {
        private readonly Dictionary<string, BeatmapOptions> options = new(StringComparer.Ordinal);
        private readonly Dictionary<string, HashSet<string>> collections = new(StringComparer.Ordinal);
        private BeatmapLibrarySnapshot snapshot = snapshot;

        public BeatmapLibrarySnapshot Snapshot => snapshot;

        public int ScanCallCount { get; private set; }

        public TimeSpan ScanDelay { get; set; }

        public bool NeedsRefresh { get; set; }

        public BeatmapLibrarySnapshot Load() => snapshot;

        public BeatmapLibrarySnapshot Scan(IReadOnlySet<string>? forceUpdateDirectories = null)
        {
            ScanCallCount++;
            NeedsRefresh = false;
            if (ScanDelay > TimeSpan.Zero)
                Thread.Sleep(ScanDelay);
            return snapshot;
        }

        public bool NeedsScanRefresh() => NeedsRefresh;

        public BeatmapOptions GetOptions(string setDirectory) => options.TryGetValue(setDirectory, out var value) ? value : new BeatmapOptions(setDirectory);

        public void SaveOptions(BeatmapOptions nextOptions) => options[nextOptions.SetDirectory] = nextOptions;

        public IReadOnlyList<BeatmapCollection> GetCollections(string? selectedSetDirectory = null) => collections
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => new BeatmapCollection(pair.Key, pair.Value.Count, selectedSetDirectory is not null && pair.Value.Contains(selectedSetDirectory)))
            .ToArray();

        public IReadOnlySet<string> GetCollectionSetDirectories(string name) => collections.TryGetValue(name, out var sets)
            ? new HashSet<string>(sets, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);

        public bool CreateCollection(string name)
        {
            if (collections.ContainsKey(name))
                return false;

            collections[name] = new HashSet<string>(StringComparer.Ordinal);
            return true;
        }

        public void DeleteCollection(string name) => collections.Remove(name);

        public void ToggleCollectionMembership(string name, string setDirectory)
        {
            if (!collections.TryGetValue(name, out var sets))
                collections[name] = sets = new HashSet<string>(StringComparer.Ordinal);

            if (!sets.Add(setDirectory))
                sets.Remove(setDirectory);
        }

        public void DeleteBeatmapSet(string directory)
        {
            snapshot = new BeatmapLibrarySnapshot(snapshot.Sets.Where(set => !string.Equals(set.Directory, directory, StringComparison.Ordinal)).ToArray());
            options.Remove(directory);
            foreach (var sets in collections.Values)
                sets.Remove(directory);
        }
    }

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

    private sealed class ConfirmingPreviewPlayer : IBeatmapPreviewPlayer
    {
        public bool IsPlaying { get; private set; }

        public int PositionMilliseconds { get; private set; }

        public void Play(string audioPath, int previewTimeMilliseconds)
        {
            IsPlaying = true;
            PositionMilliseconds = previewTimeMilliseconds;
        }

        public void Play(Uri previewUri) => IsPlaying = true;

        public void PausePreview() => IsPlaying = false;

        public void ResumePreview() => IsPlaying = true;

        public void StopPreview()
        {
            IsPlaying = false;
            PositionMilliseconds = 0;
        }

        public bool TryReadSpectrum1024(float[] destination) => false;
    }
}
