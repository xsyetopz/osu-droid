using OsuDroid.Game.Runtime.Diagnostics;
namespace OsuDroid.Game.Runtime.Audio;




public sealed class PreviewMenuMusicController(IBeatmapPreviewPlayer initialPreviewPlayer) : IMenuMusicController
{
    private readonly List<MenuTrack> _queue = [];
    private readonly Dictionary<string, int> _queueIndexByIdentity = new(StringComparer.Ordinal);
    private IBeatmapPreviewPlayer _previewPlayer = initialPreviewPlayer;
    private int _currentIndex = -1;

    public MenuMusicCommand LastCommand { get; private set; }

    public MenuNowPlayingState State { get; private set; } = new();

    public void SetPreviewPlayer(IBeatmapPreviewPlayer player) => _previewPlayer = player;

    public void Queue(MenuTrack track, bool play)
    {
        long start = PerfDiagnostics.Start();
        bool wasPlayingBefore = State.IsPlaying;
        int existing = _queueIndexByIdentity.GetValueOrDefault(track.Identity, -1);
        bool wasCurrentTrack = false;
        if (existing < 0)
        {
            _queue.Add(track);
            _currentIndex = _queue.Count - 1;
            _queueIndexByIdentity[track.Identity] = _currentIndex;
        }
        else
        {
            wasCurrentTrack = existing == _currentIndex;
            _queue[existing] = track;
            _currentIndex = existing;
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

        PerfDiagnostics.Log("menuMusic._queue", start, $"play={play} _queue={_queue.Count} current={_currentIndex}");
    }

    public void SetPlaylist(IReadOnlyList<MenuTrack> tracks, int startIndex, bool play)
    {
        long start = PerfDiagnostics.Start();
        _queue.Clear();
        _queueIndexByIdentity.Clear();

        for (int i = 0; i < tracks.Count; i++)
        {
            _queue.Add(tracks[i]);
            _queueIndexByIdentity[tracks[i].Identity] = i;
        }

        _currentIndex = _queue.Count == 0 ? -1 : Math.Clamp(startIndex, 0, _queue.Count - 1);
        if (_currentIndex < 0)
        {
            State = new MenuNowPlayingState();
            return;
        }

        State = MenuMusicStateFactory.Create(_queue[_currentIndex], false);
        if (play)
        {
            PlayCurrentOrNext();
        }

        PerfDiagnostics.Log("menuMusic.setPlaylist", start, $"play={play} _queue={_queue.Count} current={_currentIndex}");
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
                BeatmapPreviewPlaybackSnapshot? playSnapshot = CurrentPlaybackSnapshot();
                if (_currentIndex >= 0 && _currentIndex < _queue.Count && !State.IsPlaying && playSnapshot is { PositionMilliseconds: > 0 })
                {
                    _previewPlayer.ResumePreview();
                    BeatmapPreviewPlaybackSnapshot? resumedSnapshot = CurrentPlaybackSnapshot();
                    State = State with
                    {
                        IsPlaying = resumedSnapshot?.IsPlaying == true,
                        PositionMilliseconds = resumedSnapshot?.PositionMilliseconds ?? State.PositionMilliseconds,
                        LengthMilliseconds = CurrentTrackLength(resumedSnapshot),
                    };
                }
                else
                {
                    PlayCurrentOrNext();
                }

                break;
            case MenuMusicCommand.Pause:
                _previewPlayer.PausePreview();
                BeatmapPreviewPlaybackSnapshot? pauseSnapshot = CurrentPlaybackSnapshot();
                State = State with
                {
                    IsPlaying = false,
                    PositionMilliseconds = pauseSnapshot?.PositionMilliseconds ?? State.PositionMilliseconds,
                    LengthMilliseconds = CurrentTrackLength(pauseSnapshot),
                };
                break;
            case MenuMusicCommand.Stop:
                _previewPlayer.StopPreview();
                State = State with { IsPlaying = false, PositionMilliseconds = 0 };
                break;
            case MenuMusicCommand.Next:
                Step(1);
                break;
            case MenuMusicCommand.None:
                break;
            default:
                break;
        }
    }

    public void Update(TimeSpan elapsed)
    {
        BeatmapPreviewPlaybackSnapshot? snapshot = CurrentPlaybackSnapshot();
        if (!State.IsPlaying)
        {
            if (_currentIndex >= 0 && _currentIndex < _queue.Count && snapshot is { IsPlaying: true })
            {
                MenuTrack track = _queue[_currentIndex];
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
            if (HasCurrentTrackEnded(snapshot) && _queue.Count > 1)
            {
                Step(1);
            }
            else
            {
                State = State with
                {
                    IsPlaying = false,
                    PositionMilliseconds = snapshot?.PositionMilliseconds ?? State.PositionMilliseconds,
                    LengthMilliseconds = CurrentTrackLength(snapshot),
                };
            }

            return;
        }

        int position = snapshot.PositionMilliseconds;
        if (position <= 0)
        {
            position = State.PositionMilliseconds + (int)Math.Max(0d, elapsed.TotalMilliseconds);
        }

        int length = CurrentTrackLength(snapshot);
        if (length > 0 && position >= length)
        {
            if (_queue.Count > 1)
            {
                Step(1);
                return;
            }

            position = length;
        }

        State = State with { PositionMilliseconds = position, LengthMilliseconds = length };
    }

    public bool TryReadSpectrum1024(float[] destination) => _previewPlayer.TryReadSpectrum1024(destination);

    private void Step(int direction)
    {
        if (_queue.Count == 0)
        {
            return;
        }

        _currentIndex = (_currentIndex + _queue.Count + direction) % _queue.Count;
        PlayCurrentOrNext();
    }

    private void PlayCurrentOrNext()
    {
        if (_queue.Count == 0)
        {
            return;
        }

        int attempts = _queue.Count;
        while (attempts-- > 0)
        {
            if (TryPlayCurrent())
            {
                return;
            }

            if (_queue.Count <= 1)
            {
                return;
            }

            _currentIndex = (_currentIndex + 1) % _queue.Count;
        }
    }

    private bool TryPlayCurrent()
    {
        long start = PerfDiagnostics.Start();
        if (_currentIndex < 0 || _currentIndex >= _queue.Count)
        {
            return false;
        }

        MenuTrack track = _queue[_currentIndex];
        if (!File.Exists(track.AudioPath))
        {
            State = MenuMusicStateFactory.Create(track, false);
            return false;
        }

        try
        {
            _previewPlayer.Play(track.AudioPath, track.PreviewTimeMilliseconds);
        }
        catch
        {
            State = MenuMusicStateFactory.Create(track, false);
            return false;
        }

        BeatmapPreviewPlaybackSnapshot? snapshot = CurrentPlaybackSnapshot(track);
        bool isPlaying = snapshot is { IsPlaying: true };
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
        _currentIndex >= 0 && _currentIndex < _queue.Count
            ? CurrentPlaybackSnapshot(_queue[_currentIndex])
            : null;

    private BeatmapPreviewPlaybackSnapshot? CurrentPlaybackSnapshot(MenuTrack track)
    {
        BeatmapPreviewPlaybackSnapshot snapshot = _previewPlayer.PlaybackSnapshot;
        return string.Equals(snapshot.Source, track.AudioPath, StringComparison.Ordinal)
            ? snapshot
            : null;
    }

    private int CurrentTrackLength(BeatmapPreviewPlaybackSnapshot? snapshot) =>
        _currentIndex >= 0 && _currentIndex < _queue.Count
            ? TrackLength(_queue[_currentIndex], snapshot)
            : Math.Max(State.LengthMilliseconds, 0);

    private static int TrackLength(MenuTrack track, BeatmapPreviewPlaybackSnapshot? snapshot) =>
        snapshot is { DurationMilliseconds: > 0 }
            ? snapshot.DurationMilliseconds
            : Math.Max(track.LengthMilliseconds, 0);

    private bool HasCurrentTrackEnded(BeatmapPreviewPlaybackSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            return false;
        }

        int length = CurrentTrackLength(snapshot);
        return length <= 0 || Math.Max(snapshot.PositionMilliseconds, State.PositionMilliseconds) >= length - 50;
    }

}
