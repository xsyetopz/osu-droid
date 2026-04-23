using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{

    [Test]
    public void RandomButtonSelectsDifferentVisibleSet()
    {
        var first = new BeatmapSetInfo(1, "1 First", [CreateBeatmap("Easy", null, 1.5f, setDirectory: "1 First", title: "First", artist: "Artist")]);
        var second = new BeatmapSetInfo(2, "2 Second", [CreateBeatmap("Easy", null, 2.5f, setId: 2, setDirectory: "2 Second", title: "Second", artist: "Artist")]);
        var third = new BeatmapSetInfo(3, "3 Third", [CreateBeatmap("Easy", null, 3.5f, setId: 3, setDirectory: "3 Third", title: "Third", artist: "Artist")]);
        var controller = new NoOpMenuMusicController();
        var scene = new SongSelectScene(new FakeLibrary(new BeatmapLibrarySnapshot([first, second, third])), controller, new FakeDifficultyService(), CreateSongsRoot("audio.mp3"), randomIndexProvider: _ => 0);

        scene.Enter();
        Assert.That(scene.SelectedBeatmap?.SetDirectory, Is.EqualTo("1 First"));

        scene.SelectRandomSet();

        Assert.That(scene.SelectedBeatmap?.SetDirectory, Is.EqualTo("2 Second"));
        Assert.That(controller.State.BeatmapSetDirectory, Is.EqualTo("2 Second"));
    }
    [Test]
    public void RandomButtonHonorsBeatmapOptionsFilters()
    {
        var first = new BeatmapSetInfo(1, "1 First", [CreateBeatmap("Easy", null, 1.5f, setDirectory: "1 First", title: "First", artist: "Artist")]);
        var second = new BeatmapSetInfo(2, "2 Second", [CreateBeatmap("Easy", null, 2.5f, setId: 2, setDirectory: "2 Second", title: "Second", artist: "Artist")]);
        var library = new FakeLibrary(new BeatmapLibrarySnapshot([first, second]));
        var scene = new SongSelectScene(library, new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"), randomIndexProvider: _ => 0);

        library.SaveOptions(new BeatmapOptions("2 Second", IsFavorite: true));
        scene.Enter();
        scene.OpenBeatmapOptions();
        scene.ToggleBeatmapOptionsFavoriteOnly();
        scene.ClosePopups();
        scene.SelectRandomSet();

        Assert.That(scene.SelectedBeatmap?.SetDirectory, Is.EqualTo("2 Second"));
    }
    [Test]
    public void RandomButtonNoOpsWhenPopupOpen()
    {
        var first = new BeatmapSetInfo(1, "1 First", [CreateBeatmap("Easy", null, 1.5f, setDirectory: "1 First", title: "First", artist: "Artist")]);
        var second = new BeatmapSetInfo(2, "2 Second", [CreateBeatmap("Easy", null, 2.5f, setId: 2, setDirectory: "2 Second", title: "Second", artist: "Artist")]);
        var scene = new SongSelectScene(new FakeLibrary(new BeatmapLibrarySnapshot([first, second])), new NoOpMenuMusicController(), new FakeDifficultyService(), CreateSongsRoot("audio.mp3"), randomIndexProvider: _ => 0);

        scene.Enter();
        scene.OpenProperties();
        scene.SelectRandomSet();

        Assert.That(scene.SelectedBeatmap?.SetDirectory, Is.EqualTo("1 First"));
    }
}
