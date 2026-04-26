namespace OsuDroid.Game.Beatmaps;

public sealed record BeatmapCollection(
    string Name,
    int BeatmapCount = 0,
    bool ContainsSelectedSet = false
);
