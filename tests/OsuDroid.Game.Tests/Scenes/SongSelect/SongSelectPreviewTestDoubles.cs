using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{
    private sealed class ConfirmingPreviewPlayer : IBeatmapPreviewPlayer
    {
        public bool IsPlaying { get; private set; }

        public int PositionMilliseconds { get; private set; }

        public void Play(string audioPath, int previewTimeMilliseconds)
        {
            IsPlaying = true;
            PositionMilliseconds = previewTimeMilliseconds;
        }

        public void Play(Uri previewUri) => IsPlaying = true;

        public void PausePreview() => IsPlaying = false;

        public void ResumePreview() => IsPlaying = true;

        public void StopPreview()
        {
            IsPlaying = false;
            PositionMilliseconds = 0;
        }

        public bool TryReadSpectrum1024(float[] destination) => false;
    }
}
