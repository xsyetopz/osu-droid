using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

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
}
