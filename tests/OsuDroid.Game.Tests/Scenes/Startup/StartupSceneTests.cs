using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

    [Test]
    public void StartupSceneShowsOriginalLoadingAndWelcomeTiming()
    {
        var scene = new StartupScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var loading = scene.CreateSnapshot(viewport).UiFrame;
        var background = loading.Elements.Single(element => element.Id == "startup-background");
        var spinner = loading.Elements.Single(element => element.Id == "startup-loading-spinner");

        Assert.That(background.Color, Is.EqualTo(UiColor.Opaque(0, 0, 0)));
        Assert.That(spinner.AssetName, Is.EqualTo(DroidAssets.Loading));
        Assert.That(spinner.Bounds.Width, Is.EqualTo(212f * 0.4f).Within(0.01f));
        Assert.That(spinner.Bounds.Height, Is.EqualTo(212f * 0.4f).Within(0.01f));
        Assert.That(loading.Elements.Any(element => element.Id == "startup-loading-title"), Is.False);
        Assert.That(loading.Elements.Any(element => element.Id == "startup-loading-text"), Is.False);

        scene.Update(TimeSpan.FromMilliseconds(StartupScene.LoadingMilliseconds));
        var fadingLoading = scene.CreateSnapshot(viewport).UiFrame;
        Assert.That(fadingLoading.Elements.Any(element => element.Id == "startup-welcome"), Is.False);
        Assert.That(fadingLoading.Elements.Any(element => element.Id == "startup-loading-spinner"), Is.False);
        Assert.That(scene.ConsumeWelcomeSoundsRequest(), Is.False);

        scene.Update(TimeSpan.FromMilliseconds(DroidUiTimings.StartupWelcomeDelayMilliseconds - StartupScene.LoadingMilliseconds));
        var welcomeStart = scene.CreateSnapshot(viewport).UiFrame;
        var welcome = welcomeStart.Elements.Single(element => element.Id == "startup-welcome");
        Assert.That(welcome.AssetName, Is.EqualTo(DroidAssets.Welcome));
        Assert.That(welcome.Alpha, Is.EqualTo(0f));
        Assert.That(welcome.Bounds.Width, Is.EqualTo(375f).Within(0.01f));
        Assert.That(welcome.Bounds.Height, Is.EqualTo(0f).Within(0.01f));
        Assert.That(welcomeStart.Elements.Any(element => element.Id == "startup-loading-spinner"), Is.False);
        Assert.That(scene.ConsumeWelcomeSoundsRequest(), Is.True);
        Assert.That(scene.ConsumeWelcomeSoundsRequest(), Is.False);

        scene.Update(TimeSpan.FromMilliseconds(DroidUiTimings.StartupWelcomeStretchMilliseconds / 2d));
        var stretching = scene.CreateSnapshot(viewport).UiFrame.Elements.Single(element => element.Id == "startup-welcome");
        Assert.That(stretching.Alpha, Is.EqualTo(0.05f).Within(0.001f));
        Assert.That(stretching.Bounds.Height, Is.EqualTo(78f * 0.5f).Within(0.01f));

        scene.Update(TimeSpan.FromMilliseconds(DroidUiTimings.StartupWelcomeStretchMilliseconds / 2d));
        var fullHeight = scene.CreateSnapshot(viewport).UiFrame.Elements.Single(element => element.Id == "startup-welcome");
        Assert.That(fullHeight.Bounds.Height, Is.EqualTo(78f).Within(0.01f));
        Assert.That(fullHeight.Bounds.Width, Is.EqualTo(375f).Within(0.01f));

        scene.Update(TimeSpan.FromMilliseconds(StartupScene.WelcomeMilliseconds));
        var finalWelcome = scene.CreateSnapshot(viewport).UiFrame.Elements.Single(element => element.Id == "startup-welcome");
        Assert.That(finalWelcome.Bounds.Height, Is.EqualTo(78f * 1.1f).Within(0.01f));
        Assert.That(finalWelcome.Bounds.Width, Is.EqualTo(375f * 1.1f).Within(0.01f));
        Assert.That(finalWelcome.Alpha, Is.EqualTo(1f));
        Assert.That(scene.IsComplete, Is.True);
    }
    [Test]
    public void BootstrapLoadingSceneShowsOriginalLoadingAssetsOnBlackVoid()
    {
        var frame = BootstrapLoadingScene.CreateSnapshot(
            VirtualViewport.FromSurface(1280, 720),
            new BootstrapLoadingProgress(10, "Loading skin..."),
            TimeSpan.FromMilliseconds(100)).UiFrame;

        Assert.That(frame.AssetManifest, Is.SameAs(DroidAssets.StartupManifest));
        Assert.That(frame.Elements.Single(element => element.Id == "bootstrap-background").Color, Is.EqualTo(UiColor.Opaque(0, 0, 0)));
        var spinner = frame.Elements.Single(element => element.Id == "bootstrap-loading-spinner");
        Assert.That(spinner.AssetName, Is.EqualTo(DroidAssets.Loading));
        Assert.That(spinner.Bounds.Width, Is.EqualTo(212f * 0.4f).Within(0.01f));
        Assert.That(spinner.Bounds.Height, Is.EqualTo(212f * 0.4f).Within(0.01f));
        Assert.That(frame.Elements.Any(element => element.Id == "bootstrap-loading-title" && element.AssetName == DroidAssets.LoadingTitle), Is.True);
        var progress = frame.Elements.Single(element => element.Id == "bootstrap-loading-progress");
        Assert.That(progress.Text, Is.EqualTo("10 %"));
        Assert.That(progress.Bounds.Y, Is.EqualTo((720f + 212f) / 2f - 212f / 4f).Within(0.01f));
        Assert.That(progress.TextStyle?.Size, Is.EqualTo(28f * 0.5f).Within(0.01f));
        var text = frame.Elements.Single(element => element.Id == "bootstrap-loading-text");
        Assert.That(text.Text, Is.EqualTo("Loading skin..."));
        Assert.That(text.Bounds.Y, Is.EqualTo(720f - 28f * 0.6f - 20f).Within(0.01f));
        Assert.That(text.TextStyle?.Size, Is.EqualTo(28f * 0.6f).Within(0.01f));
    }
}
