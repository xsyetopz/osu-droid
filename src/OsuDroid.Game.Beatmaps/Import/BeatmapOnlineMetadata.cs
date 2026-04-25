using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Beatmaps.Import;

public sealed record BeatmapOnlineMetadata(
    long SetId,
    BeatmapRankedStatus Status,
    IReadOnlyList<BeatmapOnlineDifficultyMetadata> Beatmaps);

public sealed record BeatmapOnlineDifficultyMetadata(
    long Id,
    string Version,
    float StarRating);
