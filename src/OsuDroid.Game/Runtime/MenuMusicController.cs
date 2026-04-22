namespace OsuDroid.Game.Runtime;

public enum MenuMusicCommand
{
    None,
    Previous,
    Play,
    Pause,
    Stop,
    Next,
}

public interface IMenuMusicController
{
    MenuMusicCommand LastCommand { get; }

    MenuNowPlayingState State { get; }

    void SetPreviewPlayer(IBeatmapPreviewPlayer player);

    void Queue(MenuTrack track, bool play);

    void Execute(MenuMusicCommand command);

    void Update(TimeSpan elapsed);

    bool TryReadSpectrum1024(float[] destination);
}

public sealed class NoOpMenuMusicController : IMenuMusicController
{
    public MenuMusicCommand LastCommand { get; private set; }

    public MenuNowPlayingState State { get; private set; } = new();

    public void SetPreviewPlayer(IBeatmapPreviewPlayer player)
    {
    }

    public void Queue(MenuTrack track, bool play) => State = MenuMusicStateFactory.Create(track, false);

    public void Execute(MenuMusicCommand command) => LastCommand = command;

    public void Update(TimeSpan elapsed)
    {
    }

    public bool TryReadSpectrum1024(float[] destination) => false;
}

public sealed class PreviewMenuMusicController(IBeatmapPreviewPlayer initialPreviewPlayer) : IMenuMusicController
{
    private readonly List<MenuTrack> queue = [];
    private IBeatmapPreviewPlayer previewPlayer = initialPreviewPlayer;
    private int currentIndex = -1;

    public MenuMusicCommand LastCommand { get; private set; }

    public MenuNowPlayingState State { get; private set; } = new();

    public void SetPreviewPlayer(IBeatmapPreviewPlayer player) => previewPlayer = player;

    public void Queue(MenuTrack track, bool play)
    {
        var wasPlayingBefore = State.IsPlaying;
        var existing = queue.FindIndex(item => item.Identity == track.Identity);
        var wasCurrentTrack = false;
        if (existing < 0)
        {
            queue.Add(track);
            currentIndex = queue.Count - 1;
        }
        else
        {
            wasCurrentTrack = existing == currentIndex;
            queue[existing] = track;
            currentIndex = existing;
        }

        State = MenuMusicStateFactory.Create(track, false);

        if (play)
        {
            if (wasCurrentTrack && wasPlayingBefore)
            {
                State = State with
                {
                    ArtistTitle = track.DisplayTitle,
                    LengthMilliseconds = Math.Max(track.LengthMilliseconds, 0),
                    Bpm = track.Bpm,
                    BeatmapSetDirectory = track.BeatmapSetDirectory,
                    BeatmapFilename = track.BeatmapFilename,
                };
                return;
            }

            PlayCurrent();
        }
    }

    public void Execute(MenuMusicCommand command)
    {
        LastCommand = command;
        switch (command)
        {
            case MenuMusicCommand.Previous:
                Step(-1);
                break;
            case MenuMusicCommand.Play:
                if (currentIndex >= 0 && currentIndex < queue.Count && !State.IsPlaying && previewPlayer.PositionMilliseconds > 0)
                {
                    previewPlayer.ResumePreview();
                    State = State with { IsPlaying = previewPlayer.IsPlaying, PositionMilliseconds = previewPlayer.PositionMilliseconds };
                }
                else
                    PlayCurrent();
                break;
            case MenuMusicCommand.Pause:
                previewPlayer.PausePreview();
                State = State with { IsPlaying = false, PositionMilliseconds = previewPlayer.PositionMilliseconds };
                break;
            case MenuMusicCommand.Stop:
                previewPlayer.StopPreview();
                State = State with { IsPlaying = false, PositionMilliseconds = 0 };
                break;
            case MenuMusicCommand.Next:
                Step(1);
                break;
        }
    }

    public void Update(TimeSpan elapsed)
    {
        if (!State.IsPlaying)
            return;

        if (!previewPlayer.IsPlaying)
        {
            State = State with { IsPlaying = false, PositionMilliseconds = previewPlayer.PositionMilliseconds };
            return;
        }

        var position = previewPlayer.PositionMilliseconds;
        if (position <= 0)
            position = State.PositionMilliseconds + (int)Math.Max(0d, elapsed.TotalMilliseconds);
        if (State.LengthMilliseconds > 0 && position > State.LengthMilliseconds)
            position = State.LengthMilliseconds;

        State = State with { PositionMilliseconds = position };
    }

    public bool TryReadSpectrum1024(float[] destination) => previewPlayer.TryReadSpectrum1024(destination);

    private void Step(int direction)
    {
        if (queue.Count == 0)
            return;

        currentIndex = (currentIndex + queue.Count + direction) % queue.Count;
        PlayCurrent();
    }

    private void PlayCurrent()
    {
        if (currentIndex < 0 || currentIndex >= queue.Count)
            return;

        var track = queue[currentIndex];
        if (!File.Exists(track.AudioPath))
        {
            State = MenuMusicStateFactory.Create(track, false);
            return;
        }

        try
        {
            previewPlayer.Play(track.AudioPath, track.PreviewTimeMilliseconds);
        }
        catch
        {
            State = MenuMusicStateFactory.Create(track, false);
            return;
        }

        var isPlaying = previewPlayer.IsPlaying;
        State = State with
        {
            ArtistTitle = track.DisplayTitle,
            IsPlaying = isPlaying,
            PositionMilliseconds = isPlaying ? previewPlayer.PositionMilliseconds : 0,
            LengthMilliseconds = Math.Max(track.LengthMilliseconds, 0),
            Bpm = track.Bpm,
            BeatmapSetDirectory = track.BeatmapSetDirectory,
            BeatmapFilename = track.BeatmapFilename,
        };
    }

}

internal static class MenuMusicStateFactory
{
    public static MenuNowPlayingState Create(MenuTrack track, bool isPlaying) => new(
        track.DisplayTitle,
        isPlaying,
        Math.Clamp(track.PreviewTimeMilliseconds, 0, Math.Max(track.LengthMilliseconds, 0)),
        Math.Max(track.LengthMilliseconds, 0),
        track.Bpm,
        track.BeatmapSetDirectory,
        track.BeatmapFilename);
}

public sealed record MenuTrack(
    string Identity,
    string DisplayTitle,
    string AudioPath,
    int PreviewTimeMilliseconds,
    int LengthMilliseconds = 0,
    float Bpm = 0f,
    string? BeatmapSetDirectory = null,
    string? BeatmapFilename = null);
