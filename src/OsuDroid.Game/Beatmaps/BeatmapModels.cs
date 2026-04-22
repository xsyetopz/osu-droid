namespace OsuDroid.Game.Beatmaps;

public sealed record BeatmapInfo(
    string Filename,
    string SetDirectory,
    string Md5,
    long? Id,
    string AudioFilename,
    string? BackgroundFilename,
    int? Status,
    int? SetId,
    string Title,
    string TitleUnicode,
    string Artist,
    string ArtistUnicode,
    string Creator,
    string Version,
    string Tags,
    string Source,
    long DateImported,
    float ApproachRate,
    float OverallDifficulty,
    float CircleSize,
    float HpDrainRate,
    float? DroidStarRating,
    float? StandardStarRating,
    float BpmMax,
    float BpmMin,
    float MostCommonBpm,
    long Length,
    int PreviewTime,
    int HitCircleCount,
    int SliderCount,
    int SpinnerCount,
    int MaxCombo,
    bool EpilepsyWarning)
{
    public string FullBeatmapName => string.Concat(
        string.IsNullOrWhiteSpace(Artist) ? "Unknown Artist" : Artist,
        " - ",
        string.IsNullOrWhiteSpace(Title) ? "Unknown Title" : Title,
        " (",
        string.IsNullOrWhiteSpace(Creator) ? "Unknown Creator" : Creator,
        ") [",
        string.IsNullOrWhiteSpace(Version) ? "Unknown Version" : Version,
        "]");

    public string GetSetPath(string songsPath) => Path.Combine(songsPath, SetDirectory);

    public string GetAudioPath(string songsPath) => Path.Combine(GetSetPath(songsPath), AudioFilename);

    public string? GetBackgroundPath(string songsPath) => string.IsNullOrWhiteSpace(BackgroundFilename)
        ? null
        : Path.Combine(GetSetPath(songsPath), BackgroundFilename);

    public int EffectivePreviewTime => Length <= 0
        ? Math.Max(PreviewTime, 0)
        : Math.Clamp(PreviewTime < 0 ? 0 : PreviewTime, 0, (int)Math.Min(Length, int.MaxValue));
}




