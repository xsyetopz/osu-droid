using OsuDroid.Game.Runtime.Audio;

namespace OsuDroid.Game.Tests;

public sealed partial class OptionsSceneTests
{
    private sealed class RecordingPreviewPlayer : IBeatmapPreviewPlayer
    {
        public float Volume { get; private set; } = 1f;

        public bool IsPlaying => false;

        public int PositionMilliseconds => 0;

        public BeatmapPreviewPlaybackSnapshot PlaybackSnapshot { get; } = new();

        public void Play(string audioPath, int previewTimeMilliseconds) { }

        public void Play(Uri previewUri) { }

        public void PausePreview() { }

        public void ResumePreview() { }

        public void StopPreview() { }

        public void SetVolume(float normalizedVolume) => Volume = normalizedVolume;

        public bool TryReadSpectrum1024(float[] destination) => false;
    }

    private sealed class RecordingMenuSfxPlayer : IMenuSfxPlayer
    {
        public float Volume { get; private set; } = 1f;

        public void Play(string key) { }

        public void SetVolume(float normalizedVolume) => Volume = normalizedVolume;
    }
}
