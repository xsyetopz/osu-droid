using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{

    [Test]
    public void SnapshotUsesCoverBackgroundAndStarColoredDifficultyButtons()
    {
        string songs = CreateSongsRoot("audio.mp3", "bg.jpg");
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot("bg.jpg")), new NoOpMenuMusicController(), new FakeDifficultyService(), songs);

        scene.Enter();
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "songselect-beatmap-background" && element.SpriteFit == UiSpriteFit.Cover), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-diff-row-0"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-diff-row-0-star-0" && element.AssetName == DroidAssets.SongSelectStar), Is.True);
        UiElementSnapshot difficultyRow = frame.Elements.Single(element => element.Id == "songselect-diff-row-0");
        UiElementSnapshot fractionalStar = frame.Elements.Single(element => element.Id == "songselect-diff-row-0-star-half");
        Assert.That(fractionalStar.Bounds.Width, Is.EqualTo(46f * 0.4f).Within(0.01f));
        Assert.That(fractionalStar.Bounds.Height, Is.EqualTo(47f * 0.4f).Within(0.01f));
        Assert.That(fractionalStar.Bounds.X, Is.EqualTo(difficultyRow.Bounds.X + 60f + 52f * 2f + (46f - 46f * 0.4f) / 2f).Within(0.01f));
        Assert.That(fractionalStar.Bounds.Y, Is.EqualTo(difficultyRow.Bounds.Y + 50f + (47f - 47f * 0.4f) / 2f).Within(0.01f));
        Assert.That(fractionalStar.SpriteSource, Is.Null);
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
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "songselect-back").AssetName, Is.EqualTo(DroidAssets.SongSelectBack));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-mods").AssetName, Is.EqualTo(DroidAssets.SongSelectMods));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-options").AssetName, Is.EqualTo(DroidAssets.SongSelectOptions));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-random").AssetName, Is.EqualTo(DroidAssets.SongSelectRandom));
        Assert.That(frame.Elements.Any(element => element.Text is "Mods" or "Beatmap Options" or "Random"), Is.False);
    }
    [Test]
    public void OnlineScorePanelIsHiddenUntilServerConnectionIsOn()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "songselect-scoring-switcher"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-score-panel"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-score-player"), Is.False);
    }

    [Test]
    public void OnlineScorePanelUsesLegacyBoundsWhenServerConnectionIsOn()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"), OnlineProfilePanelState.Connecting);

        scene.Enter();
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        UiFrameSnapshot mainMenuFrame = new MainMenuScene(onlinePanelState: OnlineProfilePanelState.Connecting).CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        UiElementSnapshot mainMenuPanel = mainMenuFrame.Elements.Single(element => element.Id == "profile-panel");
        UiElementSnapshot mainMenuAvatarFooter = mainMenuFrame.Elements.Single(element => element.Id == "profile-avatar-footer");
        UiElementSnapshot scorePanel = frame.Elements.Single(element => element.Id == "songselect-score-panel");
        UiElementSnapshot scoreAvatarFooter = frame.Elements.Single(element => element.Id == "songselect-score-avatar-footer");

        Assert.That(scorePanel.Bounds, Is.EqualTo(new UiRect(540.5f, 610f, 410f, 110f)));
        Assert.That(scorePanel.Alpha, Is.EqualTo(mainMenuPanel.Alpha));
        Assert.That(scorePanel.Color, Is.EqualTo(mainMenuPanel.Color));
        Assert.That(scoreAvatarFooter.Bounds, Is.EqualTo(new UiRect(540.5f, 610f, 110f, 110f)));
        Assert.That(scoreAvatarFooter.Alpha, Is.EqualTo(mainMenuAvatarFooter.Alpha));
        Assert.That(scoreAvatarFooter.Color, Is.EqualTo(mainMenuAvatarFooter.Color));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-score-message").Text, Is.EqualTo("Logging in..."));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-score-submessage").Text, Is.EqualTo("Connecting to server..."));
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-score-avatar"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-score-player"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-score-pp"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-score-acc"), Is.False);
    }

    [Test]
    public void LoggedInScorePanelShowsPerformanceAndAccuracy()
    {
        var state = new OnlineProfilePanelState(new OnlineProfileSnapshot("Player", DroidAssets.EmptyAvatar, Rank: 42, PerformancePoints: 12345, Accuracy: 98.76f));
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"), state);

        scene.Enter();
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "songselect-score-player").Text, Is.EqualTo("Player"));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-score-rank").Text, Is.EqualTo("#42"));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-score-pp").Text, Is.EqualTo("Performance: 12,345pp"));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-score-acc").Text, Is.EqualTo("Accuracy: 98.76%"));
    }
    [Test]
    public void ScoringSwitcherStaysDisabledUntilOnlineScoringIsEnabled()
    {
        BeatmapInfo rankedBeatmap = CreateBeatmap("Insane", null, 4.8f, 1);
        var snapshot = new BeatmapLibrarySnapshot([new BeatmapSetInfo(1, "1 Artist - Title", [rankedBeatmap])]);
        var scene = new SongSelectScene(new FakeLibrary(snapshot), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

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
        var thirdSet = new BeatmapSetInfo(3, "3 Third", [CreateBeatmap("Easy", null, 1.5f, setId: 3, setDirectory: "3 Third")]);
        var scene = new SongSelectScene(new FakeLibrary(new BeatmapLibrarySnapshot([firstSet, secondSet, thirdSet])), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.SelectSet(1);
        scene.Update(TimeSpan.FromSeconds(0.5));
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        UiRect[] rows = Enumerable.Range(0, 4)
            .Select(index => frame.Elements.Single(element => element.Id == $"songselect-diff-row-{index}").Bounds)
            .ToArray();

        Assert.That(rows[0].Y, Is.EqualTo(156f).Within(0.01f));
        Assert.That(rows[1].Y, Is.EqualTo(258f).Within(0.01f));
        Assert.That(rows[2].Y, Is.EqualTo(360f).Within(0.01f));
        Assert.That(rows[3].Y, Is.EqualTo(462f).Within(0.01f));
        Assert.That(rows.Select(row => row.X).Distinct().Count(), Is.GreaterThan(1));
        for (int index = 0; index < rows.Length - 1; index++)
        {
            Assert.That(rows[index + 1].Y - rows[index].Y, Is.EqualTo(102f).Within(0.01f));
        }

        UiRect followingSet = frame.Elements.Single(element => element.Id == "songselect-set-1").Bounds;
        Assert.That(followingSet.Y, Is.EqualTo(564f).Within(0.01f));
        Assert.That(followingSet.Y - (rows[^1].Y + rows[^1].Height), Is.EqualTo(-25f).Within(0.01f));
    }

    [Test]
    public void SelectingSetAnimatesDifficultyRowsFromCollapsedToExpanded()
    {
        var firstSet = new BeatmapSetInfo(1, "1 First", [CreateBeatmap("Easy", null, 1.5f)]);
        var secondSet = new BeatmapSetInfo(2, "2 Second", [
            CreateBeatmap("Easy", null, 1.5f, setId: 2, setDirectory: "2 Second"),
            CreateBeatmap("Normal", null, 2.5f, setId: 2, setDirectory: "2 Second"),
            CreateBeatmap("Hard", null, 3.5f, setId: 2, setDirectory: "2 Second"),
        ]);
        var scene = new SongSelectScene(new FakeLibrary(new BeatmapLibrarySnapshot([firstSet, secondSet])), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.SelectSet(1);
        float[] collapsed = Enumerable.Range(0, 3)
            .Select(index => scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.Single(element => element.Id == $"songselect-diff-row-{index}").Bounds.Y)
            .ToArray();
        scene.Update(TimeSpan.FromSeconds(0.25));
        float[] expanding = Enumerable.Range(0, 3)
            .Select(index => scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.Single(element => element.Id == $"songselect-diff-row-{index}").Bounds.Y)
            .ToArray();
        scene.Update(TimeSpan.FromSeconds(0.25));
        float[] expanded = Enumerable.Range(0, 3)
            .Select(index => scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.Single(element => element.Id == $"songselect-diff-row-{index}").Bounds.Y)
            .ToArray();

        Assert.That(collapsed[1] - collapsed[0], Is.EqualTo(0f).Within(0.01f));
        Assert.That(expanding[1] - expanding[0], Is.EqualTo(51f).Within(0.01f));
        Assert.That(expanded[1] - expanded[0], Is.EqualTo(102f).Within(0.01f));
    }

    [Test]
    public void CollapsedRowsUseVisiblePositionForLegacyWheelStaircase()
    {
        BeatmapSetInfo[] sets = Enumerable.Range(0, 9)
            .Select(index => new BeatmapSetInfo(index + 1, $"{index + 1} Set", [
                CreateBeatmap("Easy", null, 1.5f, setId: index + 1, setDirectory: $"{index + 1} Set"),
            ]))
            .ToArray();
        var scene = new SongSelectScene(new FakeLibrary(new BeatmapLibrarySnapshot(sets)), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.Enter();
        scene.SelectSet(4);
        scene.Update(TimeSpan.FromSeconds(0.5));
        UiRect[] rows = scene.CreateSnapshot(viewport).UiFrame.Elements
            .Where(element => element.Id.StartsWith("songselect-set-", StringComparison.Ordinal) && element.Kind == UiElementKind.Sprite)
            .Select(element => element.Bounds)
            .OrderBy(bounds => bounds.Y)
            .ToArray();

        Assert.That(rows.Length, Is.GreaterThanOrEqualTo(5));
        foreach (UiRect row in rows)
        {
            float centerY = row.Y + viewport.VirtualHeight * 0.5f + 97f * 0.5f;
            float expectedX = viewport.VirtualWidth / 1.85f + 200f * MathF.Abs(MathF.Cos(centerY * MathF.PI / (viewport.VirtualHeight * 2f)));
            Assert.That(row.X, Is.EqualTo(expectedX).Within(0.01f));
        }

        UiRect centerRow = rows.MinBy(row => MathF.Abs(row.Y + row.Height * 0.5f - viewport.VirtualHeight * 0.5f));
        Assert.That(centerRow.X, Is.LessThan(rows.First().X));
        Assert.That(centerRow.X, Is.LessThan(rows.Last().X));
    }

    [Test]
    public void DifficultyBackgroundSwitchesImmediatelyLikeLegacySongMenu()
    {
        string songs = CreateSongsRoot("audio.mp3", "easy.jpg", "hard.jpg");
        BeatmapInfo easy = CreateBeatmap("Easy", "easy.jpg", 1.5f);
        BeatmapInfo hard = CreateBeatmap("Hard", "hard.jpg", 3.5f);
        var scene = new SongSelectScene(
            new FakeLibrary(new BeatmapLibrarySnapshot([new BeatmapSetInfo(1, "1 Artist - Title", [easy, hard])])),
            new NoOpMenuMusicController(),
            new FakeDifficultyService(),
            songs);

        scene.Enter();
        scene.SelectDifficulty(1);
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        UiElementSnapshot background = frame.Elements.Single(element => element.Id == "songselect-beatmap-background");

        Assert.That(frame.Elements.Any(element => element.Id == "songselect-previous-beatmap-background"), Is.False);
        Assert.That(background.Alpha, Is.EqualTo(1f));
        Assert.That(background.Color, Is.EqualTo(UiColor.Opaque(0, 0, 0)));
        Assert.That(background.ExternalAssetPath, Does.EndWith("hard.jpg"));

        scene.Update(TimeSpan.FromSeconds(1));
        UiElementSnapshot fadedBackground = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.Single(element => element.Id == "songselect-beatmap-background");
        Assert.That(fadedBackground.Color, Is.EqualTo(UiColor.Opaque(255, 255, 255)));
    }
}
