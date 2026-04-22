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
                if (currentIndex >= 0 && currentIndex < queue.Count && !State.IsPlaying && previewPlayer.PositionMilliseconds > 0)
                {
                    previewPlayer.ResumePreview();
                    State = State with { IsPlaying = previewPlayer.IsPlaying, PositionMilliseconds = previewPlayer.PositionMilliseconds };
                }
                else
                    PlayCurrentOrNext();
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
        {
            if (currentIndex >= 0 && currentIndex < queue.Count && previewPlayer.IsPlaying)
            {
                var track = queue[currentIndex];
                State = State with
                {
                    IsPlaying = true,
                    PositionMilliseconds = previewPlayer.PositionMilliseconds,
                    ArtistTitle = track.DisplayTitle,
                    LengthMilliseconds = Math.Max(track.LengthMilliseconds, 0),
                    Bpm = track.Bpm,
                    BeatmapSetDirectory = track.BeatmapSetDirectory,
                    BeatmapFilename = track.BeatmapFilename,
                };
            }

            return;
        }

        if (!previewPlayer.IsPlaying)
        {
            if (queue.Count > 1)
                Step(1);
            else
                State = State with { IsPlaying = false, PositionMilliseconds = previewPlayer.PositionMilliseconds };
            return;
        }

        var position = previewPlayer.PositionMilliseconds;
        if (position <= 0)
            position = State.PositionMilliseconds + (int)Math.Max(0d, elapsed.TotalMilliseconds);
        if (State.LengthMilliseconds > 0 && position >= State.LengthMilliseconds)
        {
            if (queue.Count > 1)
            {
                Step(1);
                return;
            }

            position = State.LengthMilliseconds;
        }

        State = State with { PositionMilliseconds = position };
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
        PerfDiagnostics.Log("menuMusic.playCurrent", start, $"identity=\"{track.Identity}\" isPlaying={isPlaying}");
        return isPlaying;
    }

}


