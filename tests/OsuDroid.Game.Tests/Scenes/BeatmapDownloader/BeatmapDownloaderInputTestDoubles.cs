using System.Net;
using System.Reflection;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{
    private sealed class CapturingTextInputService : ITextInputService
    {
        public TextInputRequest? ActiveRequest { get; private set; }
        public int HideCount { get; private set; }

        public void RequestTextInput(TextInputRequest request) => ActiveRequest = request;

        public void HideTextInput()
        {
            HideCount++;
        }
    }
}
