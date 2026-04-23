using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class MenuMusicControllerTests
{
    [Test]
    public void QueueKeepsMetadataButNotPlayingWhenPlaybackDoesNotStart()
    {
        var audioPath = CreateAudioFile();
        var controller = new PreviewMenuMusicController(new NoOpBeatmapPreviewPlayer());

        controller.Queue(CreateTrack(audioPath), true);

        Assert.That(controller.State.ArtistTitle, Is.EqualTo("Artist - Title"));
        Assert.That(controller.State.IsPlaying, Is.False);
        Assert.That(controller.State.BeatmapSetDirectory, Is.EqualTo("1 Artist - Title"));
        Assert.That(controller.State.BeatmapFilename, Is.EqualTo("Easy.osu"));
    }

    [Test]
    public void QueueMarksPlayingAfterPlatformConfirmsPlayback()
    {
        var audioPath = CreateAudioFile();
        var player = new ConfirmingPreviewPlayer();
        var controller = new PreviewMenuMusicController(player);

        controller.Queue(CreateTrack(audioPath), true);

        Assert.That(controller.State.IsPlaying, Is.True);
        Assert.That(controller.State.PositionMilliseconds, Is.EqualTo(45000));
    }

    [Test]
    public void PauseKeepsMetadataAndStopsPlayingUntilConfirmedResume()
    {
        var audioPath = CreateAudioFile();
        var player = new ConfirmingPreviewPlayer();
        var controller = new PreviewMenuMusicController(player);
        controller.Queue(CreateTrack(audioPath), true);

        controller.Execute(MenuMusicCommand.Pause);
        Assert.That(controller.State.IsPlaying, Is.False);
        Assert.That(controller.State.ArtistTitle, Is.EqualTo("Artist - Title"));

        controller.Execute(MenuMusicCommand.Play);
        Assert.That(controller.State.IsPlaying, Is.True);
    }

    [Test]
    public void PlaylistAdvancesWhenCurrentTrackFinishes()
    {
        var player = new ConfirmingPreviewPlayer();
        var controller = new PreviewMenuMusicController(player);
        var first = CreateTrack(CreateAudioFile(), "First.osu", "Artist - First", 1000);
        var second = CreateTrack(CreateAudioFile(), "Second.osu", "Artist - Second", 1000);
        controller.SetPlaylist([first, second], 0, true);

        player.PositionMilliseconds = 1000;
        controller.Update(TimeSpan.FromMilliseconds(16));

        Assert.That(controller.State.ArtistTitle, Is.EqualTo("Artist - Second"));
        Assert.That(controller.State.IsPlaying, Is.True);
    }

    [Test]
    public void PlaylistSkipsFailedTracksUntilPlayableTrackStarts()
    {
        var player = new ConfirmingPreviewPlayer();
        var controller = new PreviewMenuMusicController(player);
        var missing = CreateTrack(Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.mp3"), "Missing.osu", "Artist - Missing", 1000);
        var playable = CreateTrack(CreateAudioFile(), "Playable.osu", "Artist - Playable", 1000);

        controller.SetPlaylist([missing, playable], 0, true);

        Assert.That(controller.State.ArtistTitle, Is.EqualTo("Artist - Playable"));
        Assert.That(controller.State.IsPlaying, Is.True);
    }

    private static MenuTrack CreateTrack(string audioPath) => new(
        "beatmap:1 Artist - Title/Easy.osu",
        "Artist - Title",
        audioPath,
        45000,
        123000,
        180f,
        "1 Artist - Title",
        "Easy.osu");

    private static MenuTrack CreateTrack(string audioPath, string beatmapFilename, string displayTitle, int lengthMilliseconds) => new(
        $"beatmap:1 Artist - Title/{beatmapFilename}",
        displayTitle,
        audioPath,
        0,
        lengthMilliseconds,
        180f,
        "1 Artist - Title",
        beatmapFilename);

    private static string CreateAudioFile()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"{Guid.NewGuid():N}.mp3");
        File.WriteAllBytes(path, [1]);
        return path;
    }

    private sealed class ConfirmingPreviewPlayer : IBeatmapPreviewPlayer
    {
        public bool IsPlaying { get; private set; }

        public int PositionMilliseconds { get; set; }

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
