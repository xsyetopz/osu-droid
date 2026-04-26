using NUnit.Framework;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Scenes.SongSelect;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{
    [Test]
    public void EnterQueuesSelectedBeatmapPreview()
    {
        string songs = CreateSongsRoot("audio.mp3");
        var controller = new PreviewMenuMusicController(new NoOpBeatmapPreviewPlayer());
        var scene = new SongSelectScene(
            new FakeLibrary(CreateSnapshot()),
            controller,
            new FakeDifficultyService(),
            songs
        );

        scene.Enter();

        Assert.That(controller.State.ArtistTitle, Is.EqualTo("Artist - Title"));
        Assert.That(controller.State.IsPlaying, Is.False);
    }

    [Test]
    public void EnterDoesNotSynchronouslyScanEmptyLibrary()
    {
        var library = new FakeLibrary(BeatmapLibrarySnapshot.Empty)
        {
            ScanDelay = TimeSpan.FromMilliseconds(500),
        };
        var scene = new SongSelectScene(
            library,
            new NoOpMenuMusicController(),
            new FakeDifficultyService(),
            CreateSongsRoot("audio.mp3")
        );
        DateTime start = DateTime.UtcNow;

        scene.Enter();

        Assert.That(DateTime.UtcNow - start, Is.LessThan(TimeSpan.FromMilliseconds(150)));
    }

    [Test]
    public void EnterStartsBackgroundRefreshWhenLibraryIndexIsStale()
    {
        var library = new FakeLibrary(CreateSnapshot()) { NeedsRefresh = true };
        var scene = new SongSelectScene(
            library,
            new NoOpMenuMusicController(),
            new FakeDifficultyService(),
            CreateSongsRoot("audio.mp3")
        );

        scene.Enter();
        SpinUntil(() => library.ScanCallCount > 0);

        Assert.That(library.ScanCallCount, Is.GreaterThan(0));
    }

    [Test]
    public void PreviewUsesClampedEffectivePreviewTime()
    {
        string songs = CreateSongsRoot("audio.mp3");
        var player = new ConfirmingPreviewPlayer();
        var controller = new PreviewMenuMusicController(player);
        BeatmapInfo beatmap = CreateBeatmap("Easy", null, 1.5f) with
        {
            PreviewTime = 999999,
            Length = 123000,
        };
        var scene = new SongSelectScene(
            new FakeLibrary(
                new BeatmapLibrarySnapshot([new BeatmapSetInfo(1, "1 Artist - Title", [beatmap])])
            ),
            controller,
            new FakeDifficultyService(),
            songs
        );

        scene.Enter();

        Assert.That(player.PositionMilliseconds, Is.EqualTo(123000));
    }

    [Test]
    public void EnterMarksPreviewPlayingOnlyAfterPlayerConfirmsPlayback()
    {
        string songs = CreateSongsRoot("audio.mp3");
        var controller = new PreviewMenuMusicController(new ConfirmingPreviewPlayer());
        var scene = new SongSelectScene(
            new FakeLibrary(CreateSnapshot()),
            controller,
            new FakeDifficultyService(),
            songs
        );

        scene.Enter();

        Assert.That(controller.State.ArtistTitle, Is.EqualTo("Artist - Title"));
        Assert.That(controller.State.IsPlaying, Is.True);
    }
}
