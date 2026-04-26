namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;

internal sealed class DifficultyControlPoint(
    double time,
    double speedMultiplier,
    bool generateTicks
) : ControlPoint(time)
{
    public double SpeedMultiplier { get; } = speedMultiplier;

    public bool GenerateTicks { get; } = generateTicks;

    public override bool IsRedundant(ControlPoint existing) =>
        existing is DifficultyControlPoint difficultyControlPoint
        && SpeedMultiplier == difficultyControlPoint.SpeedMultiplier
        && GenerateTicks == difficultyControlPoint.GenerateTicks;
}
