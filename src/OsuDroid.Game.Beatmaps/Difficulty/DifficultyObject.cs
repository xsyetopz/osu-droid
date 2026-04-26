namespace OsuDroid.Game.Beatmaps.Difficulty;

internal sealed record DifficultyObject(
    float X,
    float Y,
    long Time,
    DifficultyObjectKind Kind,
    float PixelLength
);
