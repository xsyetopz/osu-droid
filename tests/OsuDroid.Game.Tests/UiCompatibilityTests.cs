using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class UiCompatibilityTests
{

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OsuDroid.sln")))
            directory = directory.Parent;

        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }

    private static string ResolvePackagedAssetPath(string repositoryRoot, string packagePath)
    {
        const string assetPrefix = "assets/droid/";
        if (!packagePath.StartsWith(assetPrefix, StringComparison.Ordinal))
            throw new ArgumentException($"Unsupported asset package path: {packagePath}", nameof(packagePath));

        var relativePath = packagePath.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Resources", "Raw", relativePath);
    }

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
    public void MainMenuUsesDroidAssetProvenance()
    {
        var manifest = DroidAssets.MainMenuManifest;

        Assert.That(manifest.Get(DroidAssets.Play).PackagePath, Is.EqualTo("assets/droid/main-menu/play-button.png"));
        Assert.That(manifest.Get(DroidAssets.Play).Provenance, Is.EqualTo(UiAssetProvenance.OsuDroid));
        Assert.That(manifest.Get(DroidAssets.Logo).NativeSize, Is.EqualTo(new UiSize(540, 540)));
        Assert.That(manifest.Get(DroidAssets.MenuBackground).PackagePath, Is.EqualTo("assets/droid/main-menu/background.png"));
        Assert.That(manifest.Get(DroidAssets.BeatmapDownloader).NativeSize, Is.EqualTo(new UiSize(80, 284)));
        Assert.That(manifest.Get(DroidAssets.MusicNowPlaying).NativeSize, Is.EqualTo(new UiSize(1364, 60)));
        Assert.That(manifest.Get(DroidAssets.MusicPrevious).NativeSize, Is.EqualTo(new UiSize(64, 62)));
        Assert.That(manifest.Get(DroidAssets.MusicPause).NativeSize, Is.EqualTo(new UiSize(66, 66)));
    }


    [Test]
    public void MainMenuPackagedAssetsExistOnDisk()
    {
        var repositoryRoot = FindRepositoryRoot();

        foreach (var asset in DroidAssets.MainMenuManifest.Entries.Where(entry => entry.Kind == UiAssetKind.Texture))
        {
            Assert.That(asset.PackagePath, Does.StartWith("assets/droid/"));
            var sourcePath = ResolvePackagedAssetPath(repositoryRoot, asset.PackagePath);
            Assert.That(File.Exists(sourcePath), Is.True, asset.PackagePath);
        }
    }

    [Test]
    public void AppIconUsesOriginalOsuDroidLauncherArtwork()
    {
        var repositoryRoot = FindRepositoryRoot();
        var appIcon = File.ReadAllBytes(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Resources", "AppIcon", "appicon.png"));
        var legacyIcon = File.ReadAllBytes(Path.Combine(repositoryRoot, "third_party", "osu-droid-legacy", "res", "drawable-xxxhdpi", "ic_launcher.png"));

        Assert.That(appIcon, Is.EqualTo(legacyIcon));
    }

    [Test]
    public void AndroidPlatformDeclaresLegacyLauncherIcon()
    {
        var repositoryRoot = FindRepositoryRoot();
        var mainActivity = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "Android", "MainActivity.cs"));
        var mainApplication = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "Android", "MainApplication.cs"));
        var densities = new[] { "ldpi", "mdpi", "hdpi", "xhdpi", "xxhdpi", "xxxhdpi" };

        Assert.That(mainActivity, Does.Contain("Icon = \"@drawable/ic_launcher\""));
        Assert.That(mainApplication, Does.Contain("[Application(Icon = \"@drawable/ic_launcher\")]"));
        foreach (var density in densities)
        {
            var androidIcon = Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "Android", "Resources", $"drawable-{density}", "ic_launcher.png");
            var legacyIcon = Path.Combine(repositoryRoot, "third_party", "osu-droid-legacy", "res", $"drawable-{density}", "ic_launcher.png");

            Assert.That(File.Exists(androidIcon), Is.True, density);
            Assert.That(File.ReadAllBytes(androidIcon), Is.EqualTo(File.ReadAllBytes(legacyIcon)), density);
        }
    }

    [Test]
    public void MobileProjectDeclaresIconSources()
    {
        var repositoryRoot = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "OsuDroid.App.csproj"));
        var infoPlist = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "iOS", "Info.plist"));

        Assert.That(project, Does.Contain("<MauiIcon Include=\"Resources\\AppIcon\\appicon.png\" BaseSize=\"192,192\" />"));
        Assert.That(project, Does.Not.Contain("<AppIcon"));
        Assert.That(project, Does.Contain("<BundleResource Include=\"Platforms\\iOS\\Icons\\Icon-60@2x.png\" Link=\"Icon-60@2x.png\" />"));
        Assert.That(project, Does.Not.Contain("XSAppIconAssets"));
        Assert.That(project, Does.Not.Contain("<ImageAsset Include=\"Platforms\\iOS\\Assets.xcassets\\AppIcon.appiconset\\Contents.json\" />"));
        Assert.That(infoPlist, Does.Not.Contain("XSAppIconAssets"));
    }

    [Test]
    public void IosAppIconBundleResourcesContainRequiredImages()
    {
        var repositoryRoot = FindRepositoryRoot();
        var icons = Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "iOS", "Icons");
        var expectedFiles = new[]
        {
            "Icon-60@2x.png",
            "Icon-60@3x.png",
            "Icon-76@2x.png",
            "Icon-83.5@2x.png",
        };

        foreach (var file in expectedFiles)
        {
            var path = Path.Combine(icons, file);
            Assert.That(new FileInfo(path).Length, Is.GreaterThan(0), file);
        }
    }

    [Test]
    public void IosBundleVerifierRequiresDirectAppIcons()
    {
        var repositoryRoot = FindRepositoryRoot();
        var script = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "verify-ios-bundle.sh"));
        var infoPlist = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "iOS", "Info.plist"));

        Assert.That(infoPlist, Does.Contain("<key>CFBundleIcons</key>"));
        Assert.That(infoPlist, Does.Contain("<string>Icon-60</string>"));
        Assert.That(infoPlist, Does.Not.Contain("XSAppIconAssets"));
        Assert.That(script, Does.Contain("Icon-60@2x.png Icon-60@3x.png Icon-76@2x.png Icon-83.5@2x.png"));
        Assert.That(script, Does.Contain("iOS app icon file missing from app bundle"));
    }

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
        var overlayIndex = elements.FindIndex(element => element.Id == "logo-glow");

        Assert.That(menuIndex, Is.LessThan(logoIndex));
        Assert.That(logoIndex, Is.LessThan(overlayIndex));
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
        var avatar = frame.Elements.Single(element => element.Id == "profile-avatar");

        Assert.That(avatar.Bounds, Is.EqualTo(avatarFooter.Bounds));
        Assert.That(avatar.AssetName, Is.EqualTo(DroidAssets.EmptyAvatar));
        Assert.That(frame.Elements.Single(element => element.Id == "profile-player").Text, Is.EqualTo("Guest"));
        Assert.That(frame.Elements.Any(element => element.Id == "profile-pp"), Is.False);
        Assert.That(frame.Elements.Any(element => element.Id == "profile-acc"), Is.False);
    }

    [Test]
    public void MainMenuLoggedInProfileBadgeShowsPerformanceAndAccuracy()
    {
        var scene = new MainMenuScene(profile: new OnlineProfileSnapshot("Player", DroidAssets.EmptyAvatar, PerformancePoints: 12345, Accuracy: 98.76f));
        var frame = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame;

        Assert.That(frame.Elements.Single(element => element.Id == "profile-player").Text, Is.EqualTo("Player"));
        Assert.That(frame.Elements.Single(element => element.Id == "profile-pp").Text, Does.Contain("12,345pp"));
        Assert.That(frame.Elements.Single(element => element.Id == "profile-acc").Text, Is.EqualTo("98.76%"));
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

    [TestCase(1280, 720)]
    [TestCase(2532, 1170)]
    [TestCase(2340, 1080)]
    public void MainMenuUsesAndroidMusicControlGeometry(int surfaceWidth, int surfaceHeight)
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(surfaceWidth, surfaceHeight);
        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var elements = frame.Elements.ToList();

        var nowPlaying = elements.Single(element => element.Id == "music-now-playing");
        var previous = elements.Single(element => element.Id == DroidAssets.MusicPrevious);
        var play = elements.Single(element => element.Id == DroidAssets.MusicPlay);
        var pause = elements.Single(element => element.Id == DroidAssets.MusicPause);
        var stop = elements.Single(element => element.Id == DroidAssets.MusicStop);
        var next = elements.Single(element => element.Id == DroidAssets.MusicNext);

        Assert.That(frame.Elements.Any(element => element.Id == "music-strip"), Is.False);
        AssertRectClose(nowPlaying.Bounds, MainMenuScene.GetAndroidMusicNowPlayingBounds());
        Assert.That(nowPlaying.Alpha, Is.EqualTo(1f));
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
        Assert.That(title.ClipToBounds, Is.True);
        Assert.That(title.Bounds.X, Is.EqualTo(MainMenuScene.GetAndroidMusicNowPlayingBounds().X + MainMenuScene.MusicNowPlayingTitleLeftInset));
        Assert.That(title.Bounds.Right, Is.EqualTo(MainMenuScene.MusicNowPlayingTitleRightEdge));
        Assert.That(title.Bounds.Right, Is.LessThanOrEqualTo(VirtualViewport.LegacyWidth));
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

    private static void AssertMusicControl(UiElementSnapshot element, string assetName, UiAction action, float legacyIndex)
    {
        Assert.That(element.Kind, Is.EqualTo(UiElementKind.Sprite));
        Assert.That(element.AssetName, Is.EqualTo(assetName));
        AssertRectClose(element.Bounds, MainMenuScene.GetAndroidMusicControlBounds(legacyIndex));
        Assert.That(element.Action, Is.EqualTo(action));
        Assert.That(element.Alpha, Is.EqualTo(1f));
    }

    [Test]
    public void TouchingFirstMainMenuButtonSwitchesToSecondMenu()
    {
        var scene = new MainMenuScene();

        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));

        Assert.That(scene.Tap(MainMenuButtonSlot.First), Is.EqualTo(MainMenuRoute.None));
        Assert.That(scene.Snapshot.IsSecondMenu, Is.True);
        Assert.That(scene.Snapshot.UiFrame.Elements.Single(element => element.Id == "menu-0").AssetName, Is.EqualTo(DroidAssets.Solo));
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

        Assert.That(scene.ConsumePendingRoute(), Is.EqualTo(MainMenuRoute.Exit));
        Assert.That(scene.ConsumePendingRoute(), Is.EqualTo(MainMenuRoute.None));
    }

    [Test]
    public void GameCorePublishesExitRouteAfterMainMenuExitAnimation()
    {
        var database = new DroidDatabase(Path.Combine(TestContext.CurrentContext.WorkDirectory, $"main-menu-exit-{Guid.NewGuid():N}.db"));
        database.EnsureCreated();
        var core = new OsuDroidGameCore(new GameServices(database, new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)), "test", "1.0"));

        core.HandleUiAction(UiAction.MainMenuCookie);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        core.HandleUiAction(UiAction.MainMenuThird);

        Assert.That(core.LastRoute, Is.EqualTo(MainMenuRoute.None));

        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.ExitAnimationMilliseconds));

        Assert.That(core.LastRoute, Is.EqualTo(MainMenuRoute.Exit));
    }

    [Test]
    public void MainMenuReturnTransitionFadesPreviousBackgroundLikeAndroidSongMenuBack()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.StartReturnTransition();
        var start = scene.CreateSnapshot(viewport).UiFrame;
        var startFade = start.Elements.Single(element => element.Id == "return-background-fade");

        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.ReturnBackgroundFadeDurationMilliseconds / 2d));
        var midway = scene.CreateSnapshot(viewport).UiFrame;
        var midwayFade = midway.Elements.Single(element => element.Id == "return-background-fade");

        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.ReturnBackgroundFadeDurationMilliseconds / 2d));
        var finished = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(startFade.Alpha, Is.EqualTo(1f));
        Assert.That(midwayFade.Alpha, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(finished.Elements.Any(element => element.Id == "return-background-fade"), Is.False);
        Assert.That(scene.IsReturnTransitionActive, Is.False);
    }

    [Test]
    public void MainMenuReturnTransitionDrawsBetweenBackgroundAndSceneShell()
    {
        var scene = new MainMenuScene();
        scene.StartReturnTransition();
        var elements = scene.CreateSnapshot(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.ToList();

        var backgroundIndex = elements.FindIndex(element => element.Id == "menu-background");
        var fadeIndex = elements.FindIndex(element => element.Id == "return-background-fade");
        var logoIndex = elements.FindIndex(element => element.Id == "logo");
        var profileIndex = elements.FindIndex(element => element.Id == "profile-panel");

        Assert.That(fadeIndex, Is.GreaterThan(backgroundIndex));
        Assert.That(fadeIndex, Is.LessThan(logoIndex));
        Assert.That(fadeIndex, Is.LessThan(profileIndex));
    }

    [Test]
    public void GameCoreCanStartSongSelectBackReturnTransition()
    {
        var database = new DroidDatabase(Path.Combine(TestContext.CurrentContext.WorkDirectory, $"main-menu-return-{Guid.NewGuid():N}.db"));
        database.EnsureCreated();
        var core = new OsuDroidGameCore(new GameServices(database, new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)), "test", "1.0"));
        var viewport = VirtualViewport.FromSurface(1280, 720);

        core.BackToMainMenu();
        Assert.That(core.CreateFrame(viewport).UiFrame.Elements.Any(element => element.Id == "return-background-fade"), Is.False);

        core.BackToMainMenu(MainMenuReturnTransition.SongSelectBack);
        Assert.That(core.CreateFrame(viewport).UiFrame.Elements.Any(element => element.Id == "return-background-fade"), Is.True);
    }


    private static void AssertRectClose(UiRect actual, UiRect expected)
    {
        Assert.That(actual.X, Is.EqualTo(expected.X).Within(0.001f));
        Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(0.001f));
        Assert.That(actual.Width, Is.EqualTo(expected.Width).Within(0.001f));
        Assert.That(actual.Height, Is.EqualTo(expected.Height).Within(0.001f));
    }

    [Test]
    public void VersionPillOpensAboutDialogAndChangelogUrl()
    {
        var scene = new MainMenuScene("9.9");
        var viewport = VirtualViewport.FromSurface(1280, 720);
        var frame = scene.CreateSnapshot(viewport).UiFrame;
        var versionPill = frame.Elements.Single(element => element.Id == "version-pill");
        var versionText = frame.Elements.Single(element => element.Id == "version-pill-text");

        Assert.That(versionPill.Action, Is.EqualTo(UiAction.MainMenuVersionPill));
        Assert.That(versionText.Text, Is.EqualTo("osu!droid 9.9"));
        Assert.That(versionPill.Bounds, Is.EqualTo(scene.GetVersionPillBounds(viewport)));

        scene.OpenAboutDialog();
        var about = scene.CreateSnapshot(viewport).UiFrame;

        var panel = about.Elements.Single(element => element.Id == "about-panel");
        var title = about.Elements.Single(element => element.Id == "about-title");
        var osuLink = about.Elements.Single(element => element.Id == "about-osu-link");
        var droidLink = about.Elements.Single(element => element.Id == "about-droid-link");
        var discordLink = about.Elements.Single(element => element.Id == "about-discord-link");

        Assert.That(scene.IsAboutDialogOpen, Is.True);
        Assert.That(panel.Bounds.Width, Is.EqualTo(500f));
        Assert.That(panel.CornerRadius, Is.EqualTo(14f));
        Assert.That(title.TextStyle?.Alignment, Is.EqualTo(UiTextAlignment.Center));
        Assert.That(osuLink.TextStyle?.Underline, Is.True);
        Assert.That(osuLink.Action, Is.EqualTo(UiAction.MainMenuAboutOsuWebsite));
        Assert.That(droidLink.Action, Is.EqualTo(UiAction.MainMenuAboutOsuDroidWebsite));
        Assert.That(discordLink.Action, Is.EqualTo(UiAction.MainMenuAboutDiscord));
        Assert.That(about.Elements.Single(element => element.Id == "about-changelog").Action, Is.EqualTo(UiAction.MainMenuAboutChangelog));
        Assert.That(about.Elements.Single(element => element.Id == "about-close").Action, Is.EqualTo(UiAction.MainMenuAboutClose));
    }

    [Test]
    public void MainMenuCoreRoutesMusicAndAboutActions()
    {
        var database = new DroidDatabase(Path.Combine(TestContext.CurrentContext.WorkDirectory, $"main-menu-{Guid.NewGuid():N}.db"));
        database.EnsureCreated();
        var musicController = new NoOpMenuMusicController();
        var core = new OsuDroidGameCore(new GameServices(database, new DroidGamePathLayout(DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)), "test", "1.2.3", musicController));

        core.HandleUiAction(UiAction.MainMenuMusicNext);
        Assert.That(core.LastMusicCommand, Is.EqualTo(MenuMusicCommand.Next));

        core.HandleUiAction(UiAction.MainMenuVersionPill);
        Assert.That(core.CreateFrame(VirtualViewport.FromSurface(1280, 720)).UiFrame.Elements.Any(element => element.Id == "about-panel"), Is.True);

        core.HandleUiAction(UiAction.MainMenuAboutOsuWebsite);
        Assert.That(core.ConsumePendingExternalUrl(), Is.EqualTo("https://osu.ppy.sh"));

        core.HandleUiAction(UiAction.MainMenuAboutOsuDroidWebsite);
        Assert.That(core.ConsumePendingExternalUrl(), Is.EqualTo("https://osudroid.moe"));

        core.HandleUiAction(UiAction.MainMenuAboutDiscord);
        Assert.That(core.ConsumePendingExternalUrl(), Is.EqualTo("https://discord.gg/nyD92cE"));

        core.HandleUiAction(UiAction.MainMenuAboutChangelog);
        Assert.That(core.ConsumePendingExternalUrl(), Is.EqualTo("https://osudroid.moe/changelog/latest"));
        Assert.That(core.PendingExternalUrl, Is.Null);
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
