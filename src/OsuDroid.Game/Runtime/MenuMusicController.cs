namespace OsuDroid.Game.Runtime;




public sealed class PreviewMenuMusicController(IBeatmapPreviewPlayer initialPreviewPlayer) : IMenuMusicController
{
    private readonly List<MenuTrack> queue = [];
    private readonly Dictionary<string, int> queueIndexByIdentity = new(StringComparer.Ordinal);
    private IBeatmapPreviewPlayer previewPlayer = initialPreviewPlayer;
    private int currentIndex = -1;

    public MenuMusicCommand LastCommand { get; private set; }

    public MenuNowPlayingState State { get; private set; } = new();

    public void SetPreviewPlayer(IBeatmapPreviewPlayer player) => previewPlayer = player;

    public void Queue(MenuTrack track, bool play)
    {
        var start = PerfDiagnostics.Start();
        var wasPlayingBefore = State.IsPlaying;
        var existing = queueIndexByIdentity.GetValueOrDefault(track.Identity, -1);
        var wasCurrentTrack = false;
        if (existing < 0)
        {
            queue.Add(track);
            currentIndex = queue.Count - 1;
            queueIndexByIdentity[track.Identity] = currentIndex;
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

            PlayCurrentOrNext();
        }

        PerfDiagnostics.Log("menuMusic.queue", start, $"play={play} queue={queue.Count} current={currentIndex}");
    }

    public void SetPlaylist(IReadOnlyList<MenuTrack> tracks, int startIndex, bool play)
    {
        var start = PerfDiagnostics.Start();
        queue.Clear();
        queueIndexByIdentity.Clear();

        for (var i = 0; i < tracks.Count; i++)
        {
            queue.Add(tracks[i]);
            queueIndexByIdentity[tracks[i].Identity] = i;
        }

        currentIndex = queue.Count == 0 ? -1 : Math.Clamp(startIndex, 0, queue.Count - 1);
        if (currentIndex < 0)
        {
            State = new MenuNowPlayingState();
            return;
        }

        State = MenuMusicStateFactory.Create(queue[currentIndex], false);
        if (play)
            PlayCurrentOrNext();

        PerfDiagnostics.Log("menuMusic.setPlaylist", start, $"play={play} queue={queue.Count} current={currentIndex}");
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
                var playSnapshot = CurrentPlaybackSnapshot();
                if (currentIndex >= 0 && currentIndex < queue.Count && !State.IsPlaying && playSnapshot is { PositionMilliseconds: > 0 })
                {
                    previewPlayer.ResumePreview();
                    var resumedSnapshot = CurrentPlaybackSnapshot();
                    State = State with
                    {
                        IsPlaying = resumedSnapshot?.IsPlaying == true,
                        PositionMilliseconds = resumedSnapshot?.PositionMilliseconds ?? State.PositionMilliseconds,
                        LengthMilliseconds = CurrentTrackLength(resumedSnapshot),
                    };
                }
                else
                    PlayCurrentOrNext();
                break;
            case MenuMusicCommand.Pause:
                previewPlayer.PausePreview();
                var pauseSnapshot = CurrentPlaybackSnapshot();
                State = State with
                {
                    IsPlaying = false,
                    PositionMilliseconds = pauseSnapshot?.PositionMilliseconds ?? State.PositionMilliseconds,
                    LengthMilliseconds = CurrentTrackLength(pauseSnapshot),
                };
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
        var snapshot = CurrentPlaybackSnapshot();
        if (!State.IsPlaying)
        {
            if (currentIndex >= 0 && currentIndex < queue.Count && snapshot is { IsPlaying: true })
            {
                var track = queue[currentIndex];
                State = State with
                {
                    IsPlaying = true,
                    PositionMilliseconds = snapshot.PositionMilliseconds,
                    ArtistTitle = track.DisplayTitle,
                    LengthMilliseconds = CurrentTrackLength(snapshot),
                    Bpm = track.Bpm,
                    BeatmapSetDirectory = track.BeatmapSetDirectory,
                    BeatmapFilename = track.BeatmapFilename,
                };
            }

            return;
        }

        if (snapshot is not { IsPlaying: true })
        {
            if (HasCurrentTrackEnded(snapshot) && queue.Count > 1)
                Step(1);
            else
                State = State with
                {
                    IsPlaying = false,
                    PositionMilliseconds = snapshot?.PositionMilliseconds ?? State.PositionMilliseconds,
                    LengthMilliseconds = CurrentTrackLength(snapshot),
                };
            return;
        }

        var position = snapshot.PositionMilliseconds;
        if (position <= 0)
            position = State.PositionMilliseconds + (int)Math.Max(0d, elapsed.TotalMilliseconds);
        var length = CurrentTrackLength(snapshot);
        if (length > 0 && position >= length)
        {
            if (queue.Count > 1)
            {
                Step(1);
                return;
            }

            position = length;
        }

        State = State with { PositionMilliseconds = position, LengthMilliseconds = length };
    }

    public bool TryReadSpectrum1024(float[] destination) => previewPlayer.TryReadSpectrum1024(destination);

    private void Step(int direction)
    {
        if (queue.Count == 0)
            return;

        currentIndex = (currentIndex + queue.Count + direction) % queue.Count;
        PlayCurrentOrNext();
    }

    private void PlayCurrentOrNext()
    {
        if (queue.Count == 0)
            return;

        var attempts = queue.Count;
        while (attempts-- > 0)
        {
            if (TryPlayCurrent())
                return;

            if (queue.Count <= 1)
                return;

            currentIndex = (currentIndex + 1) % queue.Count;
        }
    }

    private bool TryPlayCurrent()
    {
        var start = PerfDiagnostics.Start();
        if (currentIndex < 0 || currentIndex >= queue.Count)
            return false;

        var track = queue[currentIndex];
        if (!File.Exists(track.AudioPath))
        {
            State = MenuMusicStateFactory.Create(track, false);
            return false;
        }

        try
        {
            previewPlayer.Play(track.AudioPath, track.PreviewTimeMilliseconds);
        }
        catch
        {
            State = MenuMusicStateFactory.Create(track, false);
            return false;
        }

        var snapshot = CurrentPlaybackSnapshot(track);
        var isPlaying = snapshot is { IsPlaying: true };
        State = State with
        {
            ArtistTitle = track.DisplayTitle,
            IsPlaying = isPlaying,
            PositionMilliseconds = isPlaying ? snapshot!.PositionMilliseconds : 0,
            LengthMilliseconds = TrackLength(track, snapshot),
            Bpm = track.Bpm,
            BeatmapSetDirectory = track.BeatmapSetDirectory,
            BeatmapFilename = track.BeatmapFilename,
        };
        PerfDiagnostics.Log("menuMusic.playCurrent", start, $"identity=\"{track.Identity}\" isPlaying={isPlaying}");
        return true;
    }

    private BeatmapPreviewPlaybackSnapshot? CurrentPlaybackSnapshot() =>
        currentIndex >= 0 && currentIndex < queue.Count
            ? CurrentPlaybackSnapshot(queue[currentIndex])
            : null;

    private BeatmapPreviewPlaybackSnapshot? CurrentPlaybackSnapshot(MenuTrack track)
    {
        var snapshot = previewPlayer.PlaybackSnapshot;
        return string.Equals(snapshot.Source, track.AudioPath, StringComparison.Ordinal)
            ? snapshot
            : null;
    }

    private int CurrentTrackLength(BeatmapPreviewPlaybackSnapshot? snapshot) =>
        currentIndex >= 0 && currentIndex < queue.Count
            ? TrackLength(queue[currentIndex], snapshot)
            : Math.Max(State.LengthMilliseconds, 0);

    private static int TrackLength(MenuTrack track, BeatmapPreviewPlaybackSnapshot? snapshot) =>
        snapshot is { DurationMilliseconds: > 0 }
            ? snapshot.DurationMilliseconds
            : Math.Max(track.LengthMilliseconds, 0);

    private bool HasCurrentTrackEnded(BeatmapPreviewPlaybackSnapshot? snapshot)
    {
        if (snapshot is null)
            return false;

        var length = CurrentTrackLength(snapshot);
        return length <= 0 || Math.Max(snapshot.PositionMilliseconds, State.PositionMilliseconds) >= length - 50;
    }

}
