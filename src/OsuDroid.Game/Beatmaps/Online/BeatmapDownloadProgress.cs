namespace OsuDroid.Game.Beatmaps.Online;

public enum BeatmapDownloadPhase
{
    Connecting,
    Downloading,
    Importing,
}

public sealed record BeatmapDownloadProgress(long BytesReceived, long? TotalBytes, BeatmapDownloadPhase Phase, double SpeedBytesPerSecond = 0)
{
    public double? Percent => TotalBytes is > 0 ? BytesReceived * 100d / TotalBytes.Value : null;
}
