using OsuDroid.Game.Runtime.Audio;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{
    private sealed class ConfirmingPreviewPlayer : IBeatmapPreviewPlayer
    {
        public bool IsPlaying { get; private set; }

        public int PositionMilliseconds { get; private set; }

        public string? Source { get; private set; }

        public BeatmapPreviewPlaybackSnapshot PlaybackSnapshot => new(Source, IsPlaying, PositionMilliseconds);

        public void Play(string audioPath, int previewTimeMilliseconds)
        {
            IsPlaying = true;
            Source = audioPath;
            PositionMilliseconds = previewTimeMilliseconds;
        }

        public void Play(Uri previewUri)
        {
            IsPlaying = true;
            Source = previewUri.ToString();
        }

        public void PausePreview() => IsPlaying = false;

        public void ResumePreview() => IsPlaying = true;

        public void StopPreview()
        {
            IsPlaying = false;
            Source = null;
            PositionMilliseconds = 0;
        }

        public void SetVolume(float normalizedVolume)
        {
        }

        public bool TryReadSpectrum1024(float[] destination) => false;
    }
}
