namespace OsuDroid.Game.Runtime;

public sealed record MenuTrack(
    string Identity,
    string DisplayTitle,
    string AudioPath,
    int PreviewTimeMilliseconds,
    int LengthMilliseconds = 0,
    float Bpm = 0f,
    string? BeatmapSetDirectory = null,
    string? BeatmapFilename = null);
