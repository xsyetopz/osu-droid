using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{
    private sealed class CapturingTextInputService : ITextInputService
    {
        public TextInputRequest? ActiveRequest { get; private set; }
        public int HideCount { get; private set; }

        public void RequestTextInput(TextInputRequest request) => ActiveRequest = request;

        public void HideTextInput() => HideCount++;
    }
}
