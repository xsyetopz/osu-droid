namespace OsuDroid.Game.Beatmaps.Online;

public sealed record BeatmapMirrorDefinition(
    BeatmapMirrorKind Kind,
    string HomeUrl,
    string Description,
    bool SupportsNoVideoDownloads
);
