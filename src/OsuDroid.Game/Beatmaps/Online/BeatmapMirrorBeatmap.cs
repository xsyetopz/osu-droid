namespace OsuDroid.Game.Beatmaps.Online;

public sealed record BeatmapMirrorBeatmap(
    long Id,
    string Version,
    float StarRating,
    float ApproachRate,
    float CircleSize,
    float HpDrainRate,
    float OverallDifficulty,
    float Bpm,
    int HitLength,
    int CircleCount,
    int SliderCount,
    int SpinnerCount,
    int Mode);
