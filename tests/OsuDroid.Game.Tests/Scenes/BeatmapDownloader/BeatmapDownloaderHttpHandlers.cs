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
    private sealed class EmptyHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("[]") });
    }

    private sealed class JsonHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) });
    }
}
