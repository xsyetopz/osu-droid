using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

    [Test]
    public void MainMenuSpectrumStaysCenteredOnAnimatedCookie()
    {
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var scene = new MainMenuScene(nowPlaying: new MenuNowPlayingState("artist - title", true));
        var spectrum = new float[512];
        Array.Fill(spectrum, 1f);
        scene.SetSpectrum(spectrum, true);
        scene.Update(TimeSpan.FromMilliseconds(16));

        var collapsed = scene.CreateSnapshot(viewport).UiFrame;
        var collapsedLogo = collapsed.Elements.Single(element => element.Id == "logo");
        var collapsedSpectrum = collapsed.Elements.First(element => element.Id.StartsWith("logo-spectrum-", StringComparison.Ordinal));
        Assert.That(collapsedSpectrum.Bounds.X, Is.EqualTo(collapsedLogo.Bounds.X + collapsedLogo.Bounds.Width / 2f).Within(0.001f));

        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        scene.SetSpectrum(spectrum, true);
        scene.Update(TimeSpan.FromMilliseconds(16));
        var expanded = scene.CreateSnapshot(viewport).UiFrame;
        var expandedLogo = expanded.Elements.Single(element => element.Id == "logo");
        var expandedSpectrum = expanded.Elements.First(element => element.Id.StartsWith("logo-spectrum-", StringComparison.Ordinal));

        Assert.That(expandedSpectrum.Bounds.X, Is.EqualTo(expandedLogo.Bounds.X + expandedLogo.Bounds.Width / 2f).Within(0.001f));
    }
    [Test]
    public void MainMenuOnlyDrawsNowPlayingTextWhenTrackStateExists()
    {
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var emptyFrame = new MainMenuScene().CreateSnapshot(viewport).UiFrame;
        var populatedFrame = new MainMenuScene(nowPlaying: new MenuNowPlayingState("artist - title", false)).CreateSnapshot(viewport).UiFrame;
        var title = populatedFrame.Elements.Single(element => element.Id == "music-title");

        Assert.That(emptyFrame.Elements.Any(element => element.Id == "music-title"), Is.False);
        Assert.That(title.Text, Is.EqualTo("artist - title"));
        Assert.That(title.TextStyle?.Alignment, Is.EqualTo(UiTextAlignment.Right));
        Assert.That(title.ClipToBounds, Is.False);
        Assert.That(title.Bounds.X, Is.Zero);
        Assert.That(title.Bounds.Right, Is.EqualTo(MainMenuScene.MusicNowPlayingTextRight));
        Assert.That(title.Bounds.Right, Is.LessThanOrEqualTo(VirtualViewport.LegacyWidth));

        var panel = populatedFrame.Elements.Single(element => element.Id == "music-now-playing");
        Assert.That(panel.MeasuredTextAnchor, Is.Not.Null);
        Assert.That(panel.MeasuredTextAnchor!.Text, Is.EqualTo("artist - title"));
        Assert.That(panel.MeasuredTextAnchor.RightX, Is.EqualTo(MainMenuScene.MusicNowPlayingTextRight));
        Assert.That(panel.MeasuredTextAnchor.LeftPadding, Is.EqualTo(MainMenuScene.MusicNowPlayingSpriteLeftPadding));
    }
    [Test]
    public void MainMenuCookieUsesAndroidHeartbeatBeat()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var baseFrame = scene.CreateSnapshot(viewport).UiFrame;
        var baseLogo = baseFrame.Elements.Single(element => element.Id == "logo");
        var baseOverlay = baseFrame.Elements.Single(element => element.Id == "logo-glow");

        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.LogoBeatMilliseconds - 1d));
        var beforeBeat = scene.CreateSnapshot(viewport).UiFrame.Elements.Single(element => element.Id == "logo");

        scene.Update(TimeSpan.FromMilliseconds(1d));
        scene.Update(TimeSpan.FromMilliseconds(450d));
        var heartbeatFrame = scene.CreateSnapshot(viewport).UiFrame;
        var heartbeat = heartbeatFrame.Elements.Single(element => element.Id == "logo");
        var heartbeatOverlay = heartbeatFrame.Elements.Single(element => element.Id == "logo-glow");

        AssertRectClose(beforeBeat.Bounds, baseLogo.Bounds);
        Assert.That(heartbeat.Bounds.Width, Is.GreaterThan(baseLogo.Bounds.Width * 1.03f));
        Assert.That(heartbeat.Bounds.Width, Is.LessThanOrEqualTo(baseLogo.Bounds.Width * 1.07f + 0.01f));
        AssertRectClose(heartbeatOverlay.Bounds, baseOverlay.Bounds);
        Assert.That(heartbeatOverlay.Alpha, Is.EqualTo(0.2f));
    }
    [TestCase(1280, 720)]
    [TestCase(2532, 1170)]
    [TestCase(2340, 1080)]
    public void MainMenuUsesAndroidMusicControlGeometry(int surfaceWidth, int surfaceHeight)
    {
        var scene = new MainMenuScene(nowPlaying: new MenuNowPlayingState("artist - title", true, 500, 1000));
        var viewport = VirtualViewport.FromSurface(surfaceWidth, surfaceHeight);
        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var elements = frame.Elements.ToList();

        var nowPlaying = elements.Single(element => element.Id == "music-now-playing");
        var progress = elements.Single(element => element.Id == "music-progress-fg");
        var previous = elements.Single(element => element.Id == DroidAssets.MusicPrevious);
        var play = elements.Single(element => element.Id == DroidAssets.MusicPlay);
        var pause = elements.Single(element => element.Id == DroidAssets.MusicPause);
        var stop = elements.Single(element => element.Id == DroidAssets.MusicStop);
        var next = elements.Single(element => element.Id == DroidAssets.MusicNext);

        Assert.That(frame.Elements.Any(element => element.Id == "music-strip"), Is.False);
        AssertRectClose(nowPlaying.Bounds, MainMenuScene.GetAndroidMusicNowPlayingBounds());
        Assert.That(nowPlaying.Alpha, Is.EqualTo(1f));
        Assert.That(progress.Color, Is.EqualTo(UiColor.Opaque(230, 230, 230)));
        Assert.That(progress.Alpha, Is.EqualTo(0.8f));
        AssertMusicControl(previous, DroidAssets.MusicPrevious, UiAction.MainMenuMusicPrevious, 6f);
        AssertMusicControl(play, DroidAssets.MusicPlay, UiAction.MainMenuMusicPlay, 5f);
        AssertMusicControl(pause, DroidAssets.MusicPause, UiAction.MainMenuMusicPause, 4f);
        AssertMusicControl(stop, DroidAssets.MusicStop, UiAction.MainMenuMusicStop, 3f);
        AssertMusicControl(next, DroidAssets.MusicNext, UiAction.MainMenuMusicNext, 2f);

        Assert.That(elements.IndexOf(previous), Is.GreaterThan(elements.IndexOf(nowPlaying)));
        Assert.That(elements.IndexOf(play), Is.GreaterThan(elements.IndexOf(nowPlaying)));
        Assert.That(elements.IndexOf(pause), Is.GreaterThan(elements.IndexOf(nowPlaying)));
        Assert.That(elements.IndexOf(stop), Is.GreaterThan(elements.IndexOf(nowPlaying)));
        Assert.That(elements.IndexOf(next), Is.GreaterThan(elements.IndexOf(nowPlaying)));
    }
}
