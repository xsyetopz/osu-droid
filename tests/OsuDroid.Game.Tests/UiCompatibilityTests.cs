using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class UiCompatibilityTests
{
    private static UiFrameSnapshot ExpandedFrame(MainMenuScene scene, VirtualViewport viewport)
    {
        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        return scene.CreateSnapshot(viewport).UiFrame;
    }

    [Test]
    public void ViewportPreservesLegacyWidthAndDeviceAspect()
    {
        var viewport = VirtualViewport.FromSurface(2532, 1170);

        Assert.That(viewport.VirtualWidth, Is.EqualTo(1280f));
        Assert.That(viewport.VirtualHeight, Is.EqualTo(1170f / (2532f / 1280f)).Within(0.01f));
        Assert.That(viewport.ToVirtual(1266f, 585f).X, Is.EqualTo(640f).Within(0.01f));
    }

    [Test]
    public void MainMenuUsesLegacyAssetProvenance()
    {
        var manifest = LegacyUiAssets.MainMenuManifest;

        Assert.That(manifest.Get(LegacyUiAssets.Play).PackagePath, Is.EqualTo("legacy/play.png"));
        Assert.That(manifest.Get(LegacyUiAssets.Play).Provenance, Is.EqualTo(UiAssetProvenance.LegacyOsuDroid));
        Assert.That(manifest.Get(LegacyUiAssets.Logo).NativeSize, Is.EqualTo(new UiSize(540, 540)));
        Assert.That(manifest.Get(LegacyUiAssets.MenuBackground).PackagePath, Is.EqualTo("legacy/gfx/menu-background.png"));
        Assert.That(manifest.Get(LegacyUiAssets.BeatmapDownloader).NativeSize, Is.EqualTo(new UiSize(80, 284)));
        Assert.That(manifest.Get(LegacyUiAssets.MusicNowPlaying).NativeSize, Is.EqualTo(new UiSize(1364, 60)));
        Assert.That(manifest.Get(LegacyUiAssets.MusicPrevious).NativeSize, Is.EqualTo(new UiSize(64, 62)));
        Assert.That(manifest.Get(LegacyUiAssets.MusicPause).NativeSize, Is.EqualTo(new UiSize(66, 66)));
    }

    [Test]
    public void MainMenuFramePlacesLegacyButtonsAsTouchableSprites()
    {
        var scene = new MainMenuScene();
        var frame = ExpandedFrame(scene, VirtualViewport.FromSurface(1280, 720));

        var play = frame.Elements.Single(element => element.Id == "menu-0");
        var options = frame.Elements.Single(element => element.Id == "menu-1");
        var exit = frame.Elements.Single(element => element.Id == "menu-2");

        Assert.That(play.AssetName, Is.EqualTo(LegacyUiAssets.Play));
        Assert.That(options.AssetName, Is.EqualTo(LegacyUiAssets.Options));
        Assert.That(exit.AssetName, Is.EqualTo(LegacyUiAssets.Exit));
        Assert.That(frame.HitTest(new UiPoint(play.Bounds.X + play.Bounds.Width - 10f, play.Bounds.Y + play.Bounds.Height / 2f))?.Action, Is.EqualTo(UiAction.MainMenuFirst));
    }

    [Test]
    public void MainMenuFrameIncludesReferenceShellElements()
    {
        var scene = new MainMenuScene();
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Any(element => element.Id == "menu-background" && element.AssetName == LegacyUiAssets.MenuBackground), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-avatar-footer"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "version-pill"), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "beatmap-downloader" && element.AssetName == LegacyUiAssets.BeatmapDownloader), Is.True);
        Assert.That(frame.Elements.Any(element => element.Id == "music-now-playing" && element.AssetName == LegacyUiAssets.MusicNowPlaying), Is.True);
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
    public void MainMenuDrawsButtonsBehindLogo()
    {
        var scene = new MainMenuScene();
        var elements = ExpandedFrame(scene, VirtualViewport.FromSurface(1280, 720)).Elements.ToList();

        var menuIndex = elements.FindIndex(element => element.Id == "menu-1");
        var logoIndex = elements.FindIndex(element => element.Id == "logo");

        Assert.That(menuIndex, Is.LessThan(logoIndex));
    }


    [Test]
    public void MainMenuDrawsProfileBadgeAboveLogo()
    {
        var scene = new MainMenuScene();
        var elements = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.ToList();

        var logoIndex = elements.FindIndex(element => element.Id == "logo");
        var panelIndex = elements.FindIndex(element => element.Id == "profile-panel");
        var footerIndex = elements.FindIndex(element => element.Id == "profile-avatar-footer");

        Assert.That(panelIndex, Is.GreaterThan(logoIndex));
        Assert.That(footerIndex, Is.GreaterThan(panelIndex));
    }


    [Test]
    public void MainMenuProfileBadgeUsesAndroidOnlinePanelGeometry()
    {
        var scene = new MainMenuScene();
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        var panel = frame.Elements.Single(element => element.Id == "profile-panel");
        var avatarFooter = frame.Elements.Single(element => element.Id == "profile-avatar-footer");

        Assert.That(panel.Bounds, Is.EqualTo(new UiRect(MainMenuScene.OnlinePanelX, MainMenuScene.OnlinePanelY, MainMenuScene.OnlinePanelWidth, MainMenuScene.OnlinePanelHeight)));
        Assert.That(avatarFooter.Bounds, Is.EqualTo(new UiRect(MainMenuScene.OnlinePanelX, MainMenuScene.OnlinePanelY, MainMenuScene.OnlinePanelAvatarFooterSize, MainMenuScene.OnlinePanelAvatarFooterSize)));
        Assert.That(frame.Elements.Any(element => element.Id == "profile-name-placeholder"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-status-placeholder"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-avatar"), Is.False);
    }

    [Test]
    public void MainMenuProfileBadgeStaysTopLeftOnWidePhoneViewport()
    {
        var scene = new MainMenuScene();
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(2532, 1170)).UiFrame;

        var panel = frame.Elements.Single(element => element.Id == "profile-panel");

        Assert.That(panel.Bounds.X, Is.EqualTo(MainMenuScene.OnlinePanelX));
        Assert.That(panel.Bounds.Y, Is.EqualTo(MainMenuScene.OnlinePanelY));
        Assert.That(panel.Bounds.Width, Is.EqualTo(MainMenuScene.OnlinePanelWidth));
        Assert.That(panel.Bounds.Height, Is.EqualTo(MainMenuScene.OnlinePanelHeight));
    }


    [Test]
    public void MainMenuUsesAndroidMusicControlGeometry()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var frame = scene.CreateSnapshot(viewport).UiFrame;

        var nowPlaying = frame.Elements.Single(element => element.Id == "music-now-playing");
        var previous = frame.Elements.Single(element => element.Id == LegacyUiAssets.MusicPrevious);
        var play = frame.Elements.Single(element => element.Id == LegacyUiAssets.MusicPlay);
        var pause = frame.Elements.Single(element => element.Id == LegacyUiAssets.MusicPause);
        var stop = frame.Elements.Single(element => element.Id == LegacyUiAssets.MusicStop);
        var next = frame.Elements.Single(element => element.Id == LegacyUiAssets.MusicNext);

        Assert.That(frame.Elements.Any(element => element.Id == "music-strip"), Is.False);
        var nowPlayingAsset = LegacyUiAssets.MainMenuManifest.Get(LegacyUiAssets.MusicNowPlaying);
        var nowPlayingWidth = MainMenuScene.MusicNowPlayingHeight * nowPlayingAsset.NativeSize.Width / nowPlayingAsset.NativeSize.Height;

        Assert.That(nowPlaying.Bounds, Is.EqualTo(new UiRect(viewport.VirtualWidth - MainMenuScene.MusicNowPlayingXOffset, 0f, nowPlayingWidth, MainMenuScene.MusicNowPlayingHeight)));
        Assert.That(previous.Bounds, Is.EqualTo(MusicControlBounds(viewport.VirtualWidth, 6f)));
        Assert.That(play.Bounds, Is.EqualTo(MusicControlBounds(viewport.VirtualWidth, 5f)));
        Assert.That(pause.Bounds, Is.EqualTo(MusicControlBounds(viewport.VirtualWidth, 4f)));
        Assert.That(stop.Bounds, Is.EqualTo(MusicControlBounds(viewport.VirtualWidth, 3f)));
        Assert.That(next.Bounds, Is.EqualTo(MusicControlBounds(viewport.VirtualWidth, 2f)));
    }

    [Test]
    public void MainMenuUsesAndroidDownloaderTabGeometry()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var tabAsset = LegacyUiAssets.MainMenuManifest.Get(LegacyUiAssets.BeatmapDownloader);

        var tab = frame.Elements.Single(element => element.Id == "beatmap-downloader");

        Assert.That(tab.Bounds, Is.EqualTo(new UiRect(viewport.VirtualWidth - tabAsset.NativeSize.Width, (viewport.VirtualHeight - tabAsset.NativeSize.Height) / 2f, tabAsset.NativeSize.Width, tabAsset.NativeSize.Height)));
    }

    private static UiRect MusicControlBounds(float viewportWidth, float legacyIndex) => new(
        viewportWidth - MainMenuScene.MusicControlStep * legacyIndex + MainMenuScene.MusicControlRightOffset,
        MainMenuScene.MusicControlY,
        MainMenuScene.MusicControlSize,
        MainMenuScene.MusicControlSize);

    [Test]
    public void TouchingFirstMainMenuButtonSwitchesToSecondMenu()
    {
        var scene = new MainMenuScene();

        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));

        Assert.That(scene.Tap(MainMenuButtonSlot.First), Is.EqualTo(MainMenuRoute.None));
        Assert.That(scene.Snapshot.IsSecondMenu, Is.True);
        Assert.That(scene.Snapshot.UiFrame.Elements.Single(element => element.Id == "menu-0").AssetName, Is.EqualTo(LegacyUiAssets.Solo));
    }


    [Test]
    public void MainMenuScalesDownForWidePhoneViewport()
    {
        var scene = new MainMenuScene();
        var frame = ExpandedFrame(scene, VirtualViewport.FromSurface(2532, 1170));

        var logo = frame.Elements.Single(element => element.Id == "logo");
        var menu = frame.Elements.Single(element => element.Id == "menu-1");

        Assert.That(logo.Bounds.Height, Is.LessThan(430f));
        Assert.That(menu.Bounds.Height, Is.LessThan(75f));
        Assert.That(menu.Bounds.X, Is.GreaterThan(logo.Bounds.X + logo.Bounds.Width / 2f));
    }

    [Test]
    public void SceneStackKeepsRootScene()
    {
        var stack = new UiSceneStack("MainMenu");

        Assert.That(stack.TryPop(), Is.False);
        stack.Push("SongSelect");
        Assert.That(stack.Current, Is.EqualTo("SongSelect"));
        Assert.That(stack.TryPop(), Is.True);
        Assert.That(stack.Current, Is.EqualTo("MainMenu"));
    }
}
