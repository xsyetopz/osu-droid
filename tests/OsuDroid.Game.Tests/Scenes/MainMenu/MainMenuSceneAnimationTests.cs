using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

    [Test]
    public void CookieTapExpandsAndCollapsesMenu()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        var expanded = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(scene.IsMenuShown, Is.True);
        Assert.That(expanded.Elements.Any(element => element.Id == "menu-0"), Is.True);

        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuCollapseDurationMilliseconds));
        var collapsed = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(scene.IsMenuShown, Is.False);
        Assert.That(collapsed.Elements.Any(element => element.Id == "menu-0"), Is.False);
    }
    [Test]
    public void ShownMenuAutoCollapsesAfterAndroidIdleTimeout()
    {
        var scene = new MainMenuScene();
        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));

        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuIdleCollapseMilliseconds + 1d));
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuCollapseDurationMilliseconds));

        Assert.That(scene.IsMenuShown, Is.False);
        Assert.That(scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.Any(element => element.Id == "menu-0"), Is.False);
    }
    [Test]
    public void MainMenuCollapseUsesAndroidFadeAndLeftDrift()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var expanded = ExpandedFrame(scene, viewport).Elements.Single(element => element.Id == "menu-0");

        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuCollapseDurationMilliseconds / 2d));
        var collapsing = scene.CreateSnapshot(viewport).UiFrame.Elements.Single(element => element.Id == "menu-0");

        Assert.That(collapsing.Alpha, Is.LessThan(0.1f));
        Assert.That(collapsing.Bounds.X, Is.LessThan(expanded.Bounds.X));
    }
    [Test]
    public void MainMenuPressTintMatchesAndroidButtonColor()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);
        _ = ExpandedFrame(scene, viewport);

        scene.Press(UiAction.MainMenuFirst);
        var pressed = scene.CreateSnapshot(viewport).UiFrame.Elements.Single(element => element.Id == "menu-0");

        scene.ReleasePress();
        var released = scene.CreateSnapshot(viewport).UiFrame.Elements.Single(element => element.Id == "menu-0");

        Assert.That(pressed.Color.Red, Is.InRange(178, 179));
        Assert.That(pressed.Color.Green, Is.InRange(178, 179));
        Assert.That(pressed.Color.Blue, Is.InRange(178, 179));
        Assert.That(released.Color, Is.EqualTo(UiColor.Opaque(255, 255, 255)));
    }
    [Test]
    public void MainMenuCookiePressDoesNotTint()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.Press(UiAction.MainMenuCookie);
        var logo = scene.CreateSnapshot(viewport).UiFrame.Elements.Single(element => element.Id == "logo");

        Assert.That(logo.Color, Is.EqualTo(UiColor.Opaque(255, 255, 255)));
    }
    [Test]
    public void MainMenuDownloaderTabUsesAndroidPressTint()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.Press(UiAction.MainMenuBeatmapDownloader);
        var tab = scene.CreateSnapshot(viewport).UiFrame.Elements.Single(element => element.Id == "beatmap-downloader");

        Assert.That(tab.Action, Is.EqualTo(UiAction.MainMenuBeatmapDownloader));
        Assert.That(tab.Color.Red, Is.InRange(178, 179));
        Assert.That(tab.Color.Green, Is.InRange(178, 179));
        Assert.That(tab.Color.Blue, Is.InRange(178, 179));
    }
    [Test]
    public void MainMenuExitUsesAndroidFadeOutBeforeRoute()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var expanded = ExpandedFrame(scene, viewport).Elements.Single(element => element.Id == "logo");

        Assert.That(scene.Tap(MainMenuButtonSlot.Third), Is.EqualTo(MainMenuRoute.None));
        Assert.That(scene.IsExitAnimating, Is.True);
        Assert.That(scene.ConsumePendingRoute(), Is.EqualTo(MainMenuRoute.None));

        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.ExitAnimationMilliseconds / 2d));
        var midway = scene.CreateSnapshot(viewport).UiFrame;
        var logo = midway.Elements.Single(element => element.Id == "logo");
        var overlay = midway.Elements.Single(element => element.Id == "logo-glow");
        var blackout = midway.Elements.Single(element => element.Id == "exit-blackout");

        Assert.That(midway.Elements.Any(element => element.Id == "menu-0"), Is.False);
        Assert.That(blackout.Alpha, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(logo.RotationDegrees, Is.EqualTo(-7.5f).Within(0.001f));
        Assert.That(overlay.RotationDegrees, Is.EqualTo(-7.5f).Within(0.001f));
        Assert.That(logo.Bounds.Width, Is.LessThan(expanded.Bounds.Width));
        Assert.That(scene.ConsumePendingRoute(), Is.EqualTo(MainMenuRoute.None));

        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.ExitAnimationMilliseconds / 2d));

        var completed = scene.CreateSnapshot(viewport).UiFrame;
        Assert.That(completed.Elements.Any(element => element.Id == "exit-instruction" && element.Text == "Done playing? Swipe this app away to close it."), Is.True);
        Assert.That(scene.ConsumePendingRoute(), Is.EqualTo(MainMenuRoute.Exit));
        Assert.That(scene.ConsumePendingRoute(), Is.EqualTo(MainMenuRoute.None));
    }
}
