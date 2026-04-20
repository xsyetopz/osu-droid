namespace OsuDroid.Game.Beatmaps.Online;

public enum BeatmapMirrorKind
{
    OsuDirect,
    Catboy,
}

public enum BeatmapRankedStatus
{
    Ranked = 1,
    Approved = 2,
    Qualified = 3,
    Loved = 4,
    Pending = 0,
    WorkInProgress = -1,
    Graveyard = -2,
}

public enum BeatmapMirrorSort
{
    Title,
    Artist,
    Bpm,
    DifficultyRating,
    HitLength,
    PassCount,
    PlayCount,
    TotalLength,
    FavouriteCount,
    LastUpdated,
    RankedDate,
    SubmittedDate,
}

public enum BeatmapMirrorOrder
{
    Ascending,
    Descending,
}

public sealed record BeatmapMirrorDefinition(
    BeatmapMirrorKind Kind,
    string HomeUrl,
    string Description,
    string LogoAssetName,
    bool SupportsNoVideoDownloads);

public sealed record BeatmapMirrorSearchRequest(
    string Query,
    int Offset = 0,
    int Limit = 50,
    BeatmapMirrorSort Sort = BeatmapMirrorSort.RankedDate,
    BeatmapMirrorOrder Order = BeatmapMirrorOrder.Descending,
    BeatmapRankedStatus? Status = null,
    BeatmapMirrorKind Mirror = BeatmapMirrorKind.OsuDirect);

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
    int SpinnerCount);

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

public sealed record BeatmapDownloadProgress(long BytesReceived, long? TotalBytes, string State, double SpeedBytesPerSecond = 0)
{
    public double? Percent => TotalBytes is > 0 ? BytesReceived * 100d / TotalBytes.Value : null;
}
