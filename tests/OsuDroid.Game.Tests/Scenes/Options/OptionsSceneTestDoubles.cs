using NUnit.Framework;
using OsuDroid.Game.Compatibility.Online;
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

    private sealed class RecordingOnlineLoginClient : IOnlineLoginClient
    {
        private readonly OnlineLoginResult? _result;
        private readonly TaskCompletionSource<OnlineLoginResult> _completion = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        public RecordingOnlineLoginClient(OnlineLoginResult? result = null) => _result = result;

        public int LoginCalls { get; private set; }

        public string? LastUsername { get; private set; }

        public string? LastPassword { get; private set; }

        public Task<OnlineLoginResult> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken
        )
        {
            LoginCalls++;
            LastUsername = username;
            LastPassword = password;
            return _result is not null ? Task.FromResult(_result) : _completion.Task;
        }

        public void Complete(OnlineLoginResult result) => _completion.TrySetResult(result);
    }

    private static LoginProfile CreateLoginProfile(string username) =>
        new(123, "session", 42, 1000, 12345, 0.9876f, username, string.Empty);

    private static async Task WaitUntil(Func<bool> predicate)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(10).ConfigureAwait(false);
        }

        Assert.Fail("Condition was not reached.");
    }
}
