namespace OsuDroid.Game.Runtime;

public sealed record BeatmapPreviewPlaybackSnapshot(
    string? Source = null,
    bool IsPlaying = false,
    int PositionMilliseconds = 0,
    int DurationMilliseconds = 0);

public interface IBeatmapPreviewPlayer
{
    bool IsPlaying { get; }

    int PositionMilliseconds { get; }

    BeatmapPreviewPlaybackSnapshot PlaybackSnapshot { get; }

    void Play(string audioPath, int previewTimeMilliseconds);

    void Play(Uri previewUri);

    void PausePreview();

    void ResumePreview();

    void StopPreview();

    void SetVolume(float normalizedVolume);

    bool TryReadSpectrum1024(float[] destination);
}

public sealed class NoOpBeatmapPreviewPlayer : IBeatmapPreviewPlayer
{
    public bool IsPlaying => false;

    public int PositionMilliseconds => 0;

    public BeatmapPreviewPlaybackSnapshot PlaybackSnapshot { get; } = new();

    public void Play(string audioPath, int previewTimeMilliseconds)
    {
    }

    public void Play(Uri previewUri)
    {
    }

    public void PausePreview()
    {
    }

    public void ResumePreview()
    {
    }

    public void StopPreview()
    {
    }

    public void SetVolume(float normalizedVolume)
    {
    }

    public bool TryReadSpectrum1024(float[] destination) => false;
}
