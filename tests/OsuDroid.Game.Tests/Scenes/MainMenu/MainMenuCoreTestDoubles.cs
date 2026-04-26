using OsuDroid.Game.Runtime.Audio;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{
    private sealed class RecordingMenuSfxPlayer : IMenuSfxPlayer
    {
        public List<string> Keys { get; } = [];

        public void Play(string key) => Keys.Add(key);

        public void SetVolume(float normalizedVolume) { }
    }
}
