namespace OsuDroid.Game.Beatmaps.Difficulty;

internal sealed record DifficultyBeatmap(
    float ApproachRate,
    float OverallDifficulty,
    float CircleSize,
    long LengthMilliseconds,
    IReadOnlyList<DifficultyObject> Objects
);
