namespace OsuDroid.Game;

public sealed record BeatmapCard(
    string Id,
    string Artist,
    string Title,
    string DifficultyName,
    string Mapper,
    string? SourceLabel = null,
    string? Status = null);
