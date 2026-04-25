namespace OsuDroid.Game.Runtime.Audio;

public sealed record MenuNowPlayingState(
    string? ArtistTitle = null,
    bool IsPlaying = false,
    int PositionMilliseconds = 0,
    int LengthMilliseconds = 0,
    float Bpm = 0f,
    string? BeatmapSetDirectory = null,
    string? BeatmapFilename = null);
