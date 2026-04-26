namespace OsuDroid.Game.Beatmaps.Difficulty.Reference.Beatmaps.ControlPoints;

internal sealed class SampleControlPoint(
    double time,
    SampleBank sampleBank,
    int sampleVolume,
    int customSampleBank
) : ControlPoint(time)
{
    public SampleBank SampleBank { get; } = sampleBank;

    public int SampleVolume { get; } = sampleVolume;

    public int CustomSampleBank { get; } = customSampleBank;

    public override bool IsRedundant(ControlPoint existing) =>
        existing is SampleControlPoint sampleControlPoint
        && SampleBank == sampleControlPoint.SampleBank
        && SampleVolume == sampleControlPoint.SampleVolume
        && CustomSampleBank == sampleControlPoint.CustomSampleBank;
}
