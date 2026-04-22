namespace OsuDroid.Game.Runtime;

public interface IBeatmapPreviewPlayer
{
    bool IsPlaying { get; }

    int PositionMilliseconds { get; }

    void Play(string audioPath, int previewTimeMilliseconds);

    void Play(Uri previewUri);

    void PausePreview();

    void ResumePreview();

    void StopPreview();

    bool TryReadSpectrum1024(float[] destination);
}

public sealed class NoOpBeatmapPreviewPlayer : IBeatmapPreviewPlayer
{
    public bool IsPlaying => false;

    public int PositionMilliseconds => 0;

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

    public bool TryReadSpectrum1024(float[] destination) => false;
}
