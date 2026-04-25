namespace OsuDroid.Game.Beatmaps;

public sealed record BeatmapLibrarySnapshot(IReadOnlyList<BeatmapSetInfo> Sets)
{
    public static BeatmapLibrarySnapshot Empty { get; } = new([]);
}
