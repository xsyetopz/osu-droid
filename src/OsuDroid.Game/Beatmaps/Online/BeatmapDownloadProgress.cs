namespace OsuDroid.Game.Beatmaps.Online;

public sealed record BeatmapDownloadProgress(long BytesReceived, long? TotalBytes, string State, double SpeedBytesPerSecond = 0)
{
    public double? Percent => TotalBytes is > 0 ? BytesReceived * 100d / TotalBytes.Value : null;
}
