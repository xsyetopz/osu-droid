namespace OsuDroid.Game.Beatmaps;

public sealed record BeatmapSetInfo(int? Id, string Directory, IReadOnlyList<BeatmapInfo> Beatmaps)
{
    public int Count => Beatmaps.Count;

    public string GetPath(string songsPath) => Path.Combine(songsPath, Directory);
}
