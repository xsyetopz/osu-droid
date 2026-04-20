namespace OsuDroid.Game.Runtime;

public interface IBeatmapPreviewPlayer
{
    void Play(string audioPath, int previewTimeMilliseconds);

    void Play(Uri previewUri);

    void StopPreview();
}

public sealed class NoOpBeatmapPreviewPlayer : IBeatmapPreviewPlayer
{
    public void Play(string audioPath, int previewTimeMilliseconds)
    {
    }

    public void Play(Uri previewUri)
    {
    }

    public void StopPreview()
    {
    }
}
