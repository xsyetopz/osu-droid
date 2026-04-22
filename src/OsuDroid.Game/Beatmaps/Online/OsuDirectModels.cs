namespace OsuDroid.Game.Beatmaps.Online;








public sealed record BeatmapMirrorSet(
    BeatmapMirrorKind Mirror,
    long Id,
    string Title,
    string TitleUnicode,
    string Artist,
    string ArtistUnicode,
    BeatmapRankedStatus Status,
    string Creator,
    string? CoverUrl,
    bool HasVideo,
    IReadOnlyList<BeatmapMirrorBeatmap> Beatmaps)
{
    public string DisplayTitle => string.IsNullOrWhiteSpace(TitleUnicode) ? Title : TitleUnicode;

    public string DisplayArtist => string.IsNullOrWhiteSpace(ArtistUnicode) ? Artist : ArtistUnicode;
}

