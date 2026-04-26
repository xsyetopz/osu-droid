namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps;

internal enum SliderPathType
{
    Catmull,
    Bezier,
    Linear,
    PerfectCurve,
}

internal static class SliderPathTypeExtensions
{
    public static SliderPathType Parse(char value) =>
        value switch
        {
            'C' => SliderPathType.Catmull,
            'L' => SliderPathType.Linear,
            'P' => SliderPathType.PerfectCurve,
            _ => SliderPathType.Bezier,
        };
}
