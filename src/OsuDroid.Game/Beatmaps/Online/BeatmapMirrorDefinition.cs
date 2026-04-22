namespace OsuDroid.Game.Beatmaps.Online;

public sealed record BeatmapMirrorDefinition(
    BeatmapMirrorKind Kind,
    string HomeUrl,
    string Description,
    string LogoAssetName,
    bool SupportsNoVideoDownloads);
