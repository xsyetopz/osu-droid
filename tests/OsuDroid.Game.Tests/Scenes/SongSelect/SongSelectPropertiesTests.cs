using NUnit.Framework;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Scenes.SongSelect;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{

    [Test]
    public void LongPressDifficultyOpensPropertiesForThatDifficulty()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.OpenPropertiesForDifficulty(1);

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(scene.SelectedBeatmap?.Version, Is.EqualTo("Insane"));
        Assert.That(frame.Elements.Any(element => element.Id == "songselect-properties-panel"), Is.True);
    }
    [Test]
    public void PropertiesPopupUsesMaterialIconsInsteadOfTextGlyphs()
    {
        var scene = new SongSelectScene(new FakeLibrary(CreateSnapshot()), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"));

        scene.Enter();
        scene.OpenProperties();

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "songselect-properties-favorite-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.HeartOutline));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-properties-manage-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Folder));
        Assert.That(frame.Elements.Single(element => element.Id == "songselect-properties-delete-icon").MaterialIcon, Is.EqualTo(UiMaterialIcon.Delete));
        Assert.That(frame.Elements.Where(element => element.Text is not null).Select(element => element.Text), Has.None.Contains("☐ Add to Favorites"));
        Assert.That(frame.Elements.Where(element => element.Text is not null).Select(element => element.Text), Has.None.Contains("▣ Manage Favorites"));
        Assert.That(frame.Elements.Where(element => element.Text is not null).Select(element => element.Text), Has.None.Contains("⌫ Delete beatmap"));
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

        BeatmapOptions options = library.GetOptions("1 Artist - Title");

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
        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;
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
}
