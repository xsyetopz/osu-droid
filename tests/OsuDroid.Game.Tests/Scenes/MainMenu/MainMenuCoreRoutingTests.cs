using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

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
