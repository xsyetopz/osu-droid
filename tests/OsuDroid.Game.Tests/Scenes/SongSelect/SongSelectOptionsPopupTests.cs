using NUnit.Framework;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Scenes.SongSelect;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{

    [Test]
    public void BeatmapOptionsButtonOpensSearchOptionsPopup()
    {
        var library = new FakeLibrary(CreateSnapshot());
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.Enter();
        scene.OpenBeatmapOptions();

        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "songselect-properties-panel"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-beatmap-options-search"), Is.True);
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-beatmap-options-search-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Search));
        UiElementSnapshot favoriteIcon = frame.Elements.Single(element => element.Id == "songselect-beatmap-options-favorite-icon");
        Assert.That(favoriteIcon.MaterialIcon, Is.EqualTo(UiMaterialIcon.HeartOutline));
        Assert.That(favoriteIcon.Color, Is.EqualTo(UiColor.Opaque(54, 54, 83)));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-beatmap-options-algorithm-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.StarOutline));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-beatmap-options-sort-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Sort));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-beatmap-options-folder-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.FolderOutline));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-beatmap-options-folder").Text, Is.EqualTo("Default"));
    }

    [Test]
    public void BeatmapOptionsUsesOsuDroidRoundedContainerGraphics()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));
        float radius = 14f * DroidUiMetrics.DpScale;

        scene.Enter();
        scene.OpenBeatmapOptions();

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        UiElementSnapshot search = frame.Elements.Single(element => element.Id == "songselect-beatmap-options-search");
        UiElementSnapshot strip = frame.Elements.Single(element => element.Id == "songselect-beatmap-options-strip");
        UiElementSnapshot[] hitFills = frame.Elements.Where(element => element.Id.StartsWith("songselect-beatmap-options-", StringComparison.Ordinal) && element.Id.EndsWith("-hit", StringComparison.Ordinal)).ToArray();

        Assert.That(search.CornerRadius, Is.EqualTo(radius));
        Assert.That(search.Color, Is.EqualTo(UiColor.Opaque(54, 54, 83)));
        Assert.That(strip.CornerRadius, Is.EqualTo(radius));
        Assert.That(strip.Color, Is.EqualTo(UiColor.Opaque(30, 30, 46)));
        Assert.That(hitFills, Has.All.Property(nameof(UiElementSnapshot.Alpha)).EqualTo(0f));
        Assert.That(frame.Elements.Count(element => element.Id.StartsWith("songselect-beatmap-options-divider-", StringComparison.Ordinal)), Is.EqualTo(3));
    }

    [Test]
    public void BeatmapOptionsTabStripUsesAndroidWrapContentSizing()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));
        float dp = DroidUiMetrics.DpScale;

        scene.Enter();
        scene.OpenBeatmapOptions();

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        UiElementSnapshot favorite = frame.Elements.Single(element => element.Id == "songselect-beatmap-options-favorite-hit");
        UiElementSnapshot algorithm = frame.Elements.Single(element => element.Id == "songselect-beatmap-options-algorithm-hit");
        UiElementSnapshot sort = frame.Elements.Single(element => element.Id == "songselect-beatmap-options-sort-hit");
        UiElementSnapshot folder = frame.Elements.Single(element => element.Id == "songselect-beatmap-options-folder-hit");
        UiElementSnapshot strip = frame.Elements.Single(element => element.Id == "songselect-beatmap-options-strip");

        Assert.That(favorite.Bounds.Width, Is.EqualTo(56f * dp).Within(0.001f));
        Assert.That(algorithm.Bounds.Width, Is.EqualTo(ExpectedOptionsWidth("osu!droid", 16f)).Within(0.001f));
        Assert.That(sort.Bounds.Width, Is.EqualTo(ExpectedOptionsWidth("Title", 16f)).Within(0.001f));
        Assert.That(folder.Bounds.Width, Is.EqualTo(ExpectedOptionsWidth("Default", 24f)).Within(0.001f));
        Assert.That(strip.Bounds.Width, Is.EqualTo(favorite.Bounds.Width + algorithm.Bounds.Width + sort.Bounds.Width + folder.Bounds.Width + 3f * dp).Within(0.001f));
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

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Text?.Contains("Artist - Title", StringComparison.Ordinal) == true), Is.False);
        Assert.That(frame.Elements.Any(element => element.Text?.Contains("Other - Song", StringComparison.Ordinal) == true), Is.True);
        UiElementSnapshot favoriteIcon = frame.Elements.Single(element => element.Id == "songselect-beatmap-options-favorite-icon");
        Assert.That(favoriteIcon.MaterialIcon, Is.EqualTo(UiMaterialIcon.Heart));
        Assert.That(favoriteIcon.Color, Is.EqualTo(UiColor.Opaque(243, 115, 115)));
    }

    [Test]
    public void BeatmapOptionsSearchMatchesOsuDroidMetadataAndStatTokens()
    {
        BeatmapInfo filteredOut = CreateBeatmap("Easy", null, 1.5f, setDirectory: "1 Artist - Title") with
        {
            StandardStarRating = 1.5f,
            ApproachRate = 6f,
            OverallDifficulty = 5f,
            CircleSize = 4f,
            HpDrainRate = 5f,
            Id = 111,
        };
        BeatmapInfo matched = CreateBeatmap("Insane", null, 3.5f, setId: 2, setDirectory: "2 Other - Song", title: "Song", artist: "Other") with
        {
            StandardStarRating = 3.5f,
            ApproachRate = 9f,
            OverallDifficulty = 8f,
            CircleSize = 4f,
            HpDrainRate = 7f,
            Id = 222,
            Tags = "stream jump",
            Source = "source-tag",
        };
        var scene = new SongSelectScene(
            new FakeLibrary(new BeatmapLibrarySnapshot([
                new BeatmapSetInfo(1, "1 Artist - Title", [filteredOut]),
                new BeatmapSetInfo(2, "2 Other - Song", [matched]),
            ])),
            new NoOpMenuMusicController(),
            new FakeDifficultyService(),
            CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.OpenBeatmapOptions();
        scene.SetBeatmapOptionsSearchQuery("stream ar>8 od>=8 hp=7 star>3 222 insane");

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Text?.Contains("Artist - Title", StringComparison.Ordinal) == true), Is.False);
        Assert.That(frame.Elements.Any(element => element.Text?.Contains("Other - Song", StringComparison.Ordinal) == true), Is.True);
        Assert.That(scene.SelectedBeatmap?.SetDirectory, Is.EqualTo("2 Other - Song"));
    }

    [Test]
    public void BeatmapOptionsSortUsesOsuDroidSingleDifficultyRowsForStarAndLengthOrders()
    {
        BeatmapInfo low = CreateBeatmap("Low", null, 1.5f, setDirectory: "1 First", title: "First", artist: "Artist") with
        {
            BpmMax = 120f,
            DroidStarRating = 1.5f,
            StandardStarRating = 1.6f,
            Length = 120000,
        };
        BeatmapInfo high = CreateBeatmap("High", null, 4.5f, setDirectory: "1 First", title: "First", artist: "Artist") with
        {
            BpmMax = 240f,
            DroidStarRating = 4.5f,
            StandardStarRating = 4.6f,
            Length = 240000,
        };
        var scene = new SongSelectScene(
            new FakeLibrary(new BeatmapLibrarySnapshot([
                new BeatmapSetInfo(1, "1 First", [low, high]),
            ])),
            new NoOpMenuMusicController(),
            new FakeDifficultyService(),
            CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.OpenBeatmapOptions();
        for (int index = 0; index < 4; index++)
        {
            scene.CycleBeatmapOptionsSort();
        }

        scene.CycleBeatmapOptionsSort();

        UiFrameSnapshot droidStarsFrame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        Assert.That(scene.SelectedBeatmap?.Version, Is.EqualTo("High"));
        Assert.That(droidStarsFrame.Elements.Single(element => element.Id == "songselect-diff-row-0-title").Text, Does.Contain("High"));
        Assert.That(droidStarsFrame.Elements.Any(element => element.Id == "songselect-diff-row-1-title"), Is.False);
    }

    [Test]
    public void FolderFilterPopupUsesOsuDroidBottomAnchoredCollectionsPanel()
    {
        var library = new FakeLibrary(CreateSnapshot());
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));
        var viewport = VirtualViewport.FromSurface(1280, 720);
        float dp = DroidUiMetrics.DpScale;

        library.CreateCollection("Folder");
        scene.Enter();
        scene.OpenBeatmapOptions();
        scene.OpenCollectionFilter();

        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot panel = frame.Elements.Single(element => element.Id == "songselect-collections-panel");

        Assert.That(panel.Bounds.Width, Is.EqualTo(500f * dp));
        Assert.That(panel.Bounds.Y, Is.EqualTo(20f * dp).Within(0.001f));
        Assert.That(panel.Bounds.Bottom, Is.EqualTo(viewport.VirtualHeight - 20f * dp).Within(0.001f));
        Assert.That(panel.CornerRadius, Is.EqualTo(14f * dp));
        Assert.That(panel.Color, Is.EqualTo(UiColor.Opaque(19, 19, 26)));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-collections-new").Text, Is.EqualTo("Create new folder"));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-collection-0-name").Text, Is.EqualTo("Default"));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-collection-1-name").Text, Is.EqualTo("Folder"));
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-collection-0-delete"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-collection-0-toggle"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-collection-0-count"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-collection-0-selected"), Is.True);
    }

    [Test]
    public void FolderFilterPopupShowsDefaultWhenNoCustomCollectionsExist()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));
        scene.Enter();
        scene.OpenBeatmapOptions();
        scene.OpenCollectionFilter();

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "songselect-collection-0-name").Text, Is.EqualTo("Default"));
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-collection-1-name"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-collections-empty"), Is.False);
    }

    [Test]
    public void SelectingDefaultFolderClearsFolderFilterAndRestoresOverlayLabel()
    {
        var library = new FakeLibrary(CreateSnapshot());
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        library.CreateCollection("Folder");
        scene.Enter();
        scene.OpenBeatmapOptions();
        scene.OpenCollectionFilter();
        scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720));
        scene.HandleCollectionPrimaryAction(1);

        UiFrameSnapshot filteredFrame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        Assert.That(filteredFrame.Elements.Single(element => element.Id == "songselect-beatmap-options-folder").Text, Is.EqualTo("Folder"));

        scene.OpenCollectionFilter();
        scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720));
        scene.HandleCollectionPrimaryAction(0);

        UiFrameSnapshot defaultFrame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
        Assert.That(defaultFrame.Elements.Single(element => element.Id == "songselect-beatmap-options-folder").Text, Is.EqualTo("Default"));
    }

    private static float ExpectedOptionsWidth(string text, float endPaddingDp)
    {
        float dp = DroidUiMetrics.DpScale;
        return 16f * dp + 24f * dp + 12f * dp + text.Length * 14f * dp * 0.62f + endPaddingDp * dp;
    }
}
