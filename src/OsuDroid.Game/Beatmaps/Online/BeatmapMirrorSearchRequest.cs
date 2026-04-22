namespace OsuDroid.Game.Beatmaps.Online;

public sealed record BeatmapMirrorSearchRequest(
    string Query,
    int Offset = 0,
    int Limit = 50,
    BeatmapMirrorSort Sort = BeatmapMirrorSort.RankedDate,
    BeatmapMirrorOrder Order = BeatmapMirrorOrder.Descending,
    BeatmapRankedStatus? Status = null,
    BeatmapMirrorKind Mirror = BeatmapMirrorKind.OsuDirect);
