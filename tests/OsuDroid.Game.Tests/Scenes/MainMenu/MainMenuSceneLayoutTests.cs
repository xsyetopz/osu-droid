using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

    [Test]
    public void MainMenuFramePlacesDroidButtonsAsTouchableSprites()
    {
        var scene = new MainMenuScene();
        var frame = ExpandedFrame(scene, VirtualViewport.FromSurface(1280, 720));

        var play = frame.Elements.Single(element => element.Id == "menu-0");
        var options = frame.Elements.Single(element => element.Id == "menu-1");
        var exit = frame.Elements.Single(element => element.Id == "menu-2");

        Assert.That(play.AssetName, Is.EqualTo(DroidAssets.Play));
        Assert.That(options.AssetName, Is.EqualTo(DroidAssets.Options));
        Assert.That(exit.AssetName, Is.EqualTo(DroidAssets.Exit));
        Assert.That(frame.HitTest(new UiPoint(play.Bounds.X + play.Bounds.Width - 10f, play.Bounds.Y + play.Bounds.Height / 2f))?.Action, Is.EqualTo(UiAction.MainMenuFirst));
    }
    [Test]
    public void MainMenuFrameIncludesReferenceShellElements()
    {
        var scene = new MainMenuScene();
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "menu-background" && element.AssetName == DroidAssets.MenuBackground), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-avatar-footer"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "version-pill"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "version-pill-text"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "beatmap-downloader" && element.AssetName == DroidAssets.BeatmapDownloader), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "music-now-playing" && element.AssetName == DroidAssets.MusicNowPlaying), Is.True);
    }
    [Test]
    public void DebugMainMenuShowsLegacyDevelopmentBuildOverlay()
    {
        var scene = new MainMenuScene(isDevelopmentBuild: true);
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "dev-build-overlay" && element.AssetName == DroidAssets.DevBuildOverlay), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "dev-build-text" && element.Text == "DEVELOPMENT BUILD"), Is.True);
    }
    [Test]
    public void MainMenuStartsCollapsedWithCenteredCookie()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var logo = frame.Elements.Single(element => element.Id == "logo");

        Assert.That(scene.IsMenuShown, Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "menu-0"), Is.False);
        Assert.That(frame.HitTest(new UiPoint(logo.Bounds.X + logo.Bounds.Width / 2f, logo.Bounds.Y + logo.Bounds.Height / 2f))?.Action, Is.EqualTo(UiAction.MainMenuCookie));
        Assert.That(logo.Bounds.X, Is.EqualTo((viewport.VirtualWidth - logo.Bounds.Width) / 2f).Within(0.01f));
    }
    [Test]
    public void MainMenuDrawsButtonsBehindLogo()
    {
        var scene = new MainMenuScene();
        var elements = ExpandedFrame(scene, VirtualViewport.FromSurface(1280, 720)).Elements.ToList();

        var menuIndex = elements.FindIndex(element => element.Id == "menu-1");
        var logoIndex = elements.FindIndex(element => element.Id == "logo");
        var overlayIndex = elements.FindIndex(element => element.Id == "logo-glow");

        Assert.That(menuIndex, Is.LessThan(logoIndex));
        Assert.That(logoIndex, Is.LessThan(overlayIndex));
    }
    [Test]
    public void MainMenuBackgroundFitsAndroidWidthAndCentersVertically()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var background = frame.Elements.Single(element => element.Id == "menu-background");
        var backgroundAsset = DroidAssets.MainMenuManifest.Get(DroidAssets.MenuBackground);
        var expectedHeight = viewport.VirtualWidth * backgroundAsset.NativeSize.Height / backgroundAsset.NativeSize.Width;

        Assert.That(background.Bounds.X, Is.Zero);
        Assert.That(background.Bounds.Width, Is.EqualTo(viewport.VirtualWidth).Within(0.001f));
        Assert.That(background.Bounds.Height, Is.EqualTo(expectedHeight).Within(0.001f));
        Assert.That(background.Bounds.Y, Is.EqualTo((viewport.VirtualHeight - expectedHeight) / 2f).Within(0.001f));
    }
    [Test]
    public void MainMenuUsesAndroidDownloaderTabGeometry()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var tabAsset = DroidAssets.MainMenuManifest.Get(DroidAssets.BeatmapDownloader);

        var tab = frame.Elements.Single(element => element.Id == "beatmap-downloader");

        Assert.That(tab.Bounds, Is.EqualTo(new UiRect(viewport.VirtualWidth - tabAsset.NativeSize.Width, (viewport.VirtualHeight - tabAsset.NativeSize.Height) / 2f, tabAsset.NativeSize.Width, tabAsset.NativeSize.Height)));
    }
    [TestCase(1280, 720)]
    [TestCase(2532, 1170)]
    [TestCase(2340, 1080)]
    public void MainMenuUsesAndroidSourceLogoAndButtonGeometry(int surfaceWidth, int surfaceHeight)
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(surfaceWidth, surfaceHeight);
        var collapsedFrame = scene.CreateSnapshot(viewport).UiFrame;
        var expandedFrame = ExpandedFrame(scene, viewport);

        var collapsedLogo = collapsedFrame.Elements.Single(element => element.Id == "logo");
        var expandedLogo = expandedFrame.Elements.Single(element => element.Id == "logo");
        var top = expandedFrame.Elements.Single(element => element.Id == "menu-0");
        var middle = expandedFrame.Elements.Single(element => element.Id == "menu-1");
        var bottom = expandedFrame.Elements.Single(element => element.Id == "menu-2");

        AssertRectClose(collapsedLogo.Bounds, MainMenuScene.GetAndroidCollapsedLogoBounds(viewport));
        AssertRectClose(expandedLogo.Bounds, MainMenuScene.GetAndroidExpandedLogoBounds(viewport));
        AssertRectClose(top.Bounds, MainMenuScene.GetAndroidMainMenuButtonBounds(0));
        AssertRectClose(middle.Bounds, MainMenuScene.GetAndroidMainMenuButtonBounds(1));
        AssertRectClose(bottom.Bounds, MainMenuScene.GetAndroidMainMenuButtonBounds(2));
        Assert.That(top.Alpha, Is.EqualTo(0.9f));
        Assert.That(middle.Alpha, Is.EqualTo(0.9f));
        Assert.That(bottom.Alpha, Is.EqualTo(0.9f));
    }
}
