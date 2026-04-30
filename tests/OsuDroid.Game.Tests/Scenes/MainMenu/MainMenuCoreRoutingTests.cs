using NUnit.Framework;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Composition;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes.MainMenu;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

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
        Assert.That(
            scene.Snapshot.UiFrame.Elements.Single(element => element.Id == "menu-0").AssetName,
            Is.EqualTo(DroidAssets.Solo)
        );
    }

    [Test]
    public void MainMenuReturnTransitionFadesPreviousBackgroundLikeAndroidSongMenuBack()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.StartReturnTransition();
        UiFrameSnapshot start = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot startFade = start.Elements.Single(element =>
            element.Id == "return-background-fade"
        );

        scene.Update(
            TimeSpan.FromMilliseconds(MainMenuScene.ReturnBackgroundFadeDurationMilliseconds / 2d)
        );
        UiFrameSnapshot midway = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot midwayFade = midway.Elements.Single(element =>
            element.Id == "return-background-fade"
        );

        scene.Update(
            TimeSpan.FromMilliseconds(MainMenuScene.ReturnBackgroundFadeDurationMilliseconds / 2d)
        );
        UiFrameSnapshot finished = scene.CreateSnapshot(viewport).UiFrame;

        Assert.That(startFade.Alpha, Is.EqualTo(1f));
        Assert.That(midwayFade.Alpha, Is.EqualTo(0.5f).Within(0.001f));
        Assert.That(
            finished.Elements.Any(element => element.Id == "return-background-fade"),
            Is.False
        );
        Assert.That(scene.IsReturnTransitionActive, Is.False);
    }

    [Test]
    public void MainMenuReturnTransitionDrawsBetweenBackgroundAndSceneShell()
    {
        var scene = new MainMenuScene();
        scene.StartReturnTransition();
        var elements = scene
            .CreateSnapshot(VirtualViewport.FromSurface(1280, 720))
            .UiFrame.Elements.ToList();

        int backgroundIndex = elements.FindIndex(element => element.Id == "menu-background");
        int fadeIndex = elements.FindIndex(element => element.Id == "return-background-fade");
        int logoIndex = elements.FindIndex(element => element.Id == "logo");

        Assert.That(fadeIndex, Is.GreaterThan(backgroundIndex));
        Assert.That(fadeIndex, Is.LessThan(logoIndex));
    }

    [Test]
    public void MainMenuExitDialogStaysAboveBeatmapBackgroundReturnFade()
    {
        var scene = new MainMenuScene();
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.StartReturnTransition("beatmap-bg.png");
        _ = ExpandedFrame(scene, viewport);
        scene.Tap(MainMenuButtonSlot.Third);

        var elements = scene.CreateSnapshot(viewport).UiFrame.Elements.ToList();
        int fadeIndex = elements.FindIndex(element => element.Id == "return-background-fade");
        int scrimIndex = elements.FindIndex(element => element.Id == "exit-dialog-scrim");
        int panelIndex = elements.FindIndex(element => element.Id == "exit-dialog-panel");

        Assert.That(fadeIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(scrimIndex, Is.GreaterThan(fadeIndex));
        Assert.That(panelIndex, Is.GreaterThan(scrimIndex));
    }

    [Test]
    public void GameCoreCanStartSongSelectBackReturnTransition()
    {
        var database = new DroidDatabase(
            Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                $"main-menu-return-{Guid.NewGuid():N}.db"
            )
        );
        database.EnsureCreated();
        var core = new OsuDroidGameCore(
            new GameServices(
                database,
                new DroidGamePathLayout(
                    DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)
                ),
                "test",
                "1.0"
            )
        );
        var viewport = VirtualViewport.FromSurface(1280, 720);

        core.BackToMainMenu();
        Assert.That(
            core.CreateFrame(viewport)
                .UiFrame.Elements.Any(element => element.Id == "return-background-fade"),
            Is.False
        );

        core.BackToMainMenu(MainMenuReturnTransition.SongSelectBack);
        Assert.That(
            core.CreateFrame(viewport)
                .UiFrame.Elements.Any(element => element.Id == "return-background-fade"),
            Is.True
        );
    }

    [Test]
    public void VersionPillOpensAboutDialogAndChangelogUrl()
    {
        var scene = new MainMenuScene("9.9");
        var viewport = VirtualViewport.FromSurface(1280, 720);
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot versionPill = frame.Elements.Single(element =>
            element.Id == "version-pill"
        );
        UiElementSnapshot versionText = frame.Elements.Single(element =>
            element.Id == "version-pill-text"
        );

        Assert.That(versionPill.Action, Is.EqualTo(UiAction.MainMenuVersionPill));
        Assert.That(versionText.Text, Is.EqualTo("osu!droid 9.9"));
        Assert.That(versionPill.Bounds, Is.EqualTo(scene.GetVersionPillBounds(viewport)));

        scene.OpenAboutDialog();
        UiFrameSnapshot about = scene.CreateSnapshot(viewport).UiFrame;

        UiElementSnapshot panel = about.Elements.Single(element => element.Id == "about-panel");
        UiElementSnapshot title = about.Elements.Single(element => element.Id == "about-title");
        UiElementSnapshot osuLink = about.Elements.Single(element =>
            element.Id == "about-osu-link"
        );
        UiElementSnapshot droidLink = about.Elements.Single(element =>
            element.Id == "about-droid-link"
        );
        UiElementSnapshot discordLink = about.Elements.Single(element =>
            element.Id == "about-discord-link"
        );

        Assert.That(scene.IsAboutDialogOpen, Is.True);
        Assert.That(panel.Bounds.Width, Is.EqualTo(500f));
        Assert.That(panel.CornerRadius, Is.EqualTo(14f));
        Assert.That(title.TextStyle?.Alignment, Is.EqualTo(UiTextAlignment.Center));
        Assert.That(osuLink.TextStyle?.Underline, Is.True);
        Assert.That(osuLink.Action, Is.EqualTo(UiAction.MainMenuAboutOsuWebsite));
        Assert.That(droidLink.Action, Is.EqualTo(UiAction.MainMenuAboutOsuDroidWebsite));
        Assert.That(discordLink.Action, Is.EqualTo(UiAction.MainMenuAboutDiscord));
        Assert.That(
            about.Elements.Single(element => element.Id == "about-changelog").Action,
            Is.EqualTo(UiAction.MainMenuAboutChangelog)
        );
        Assert.That(
            about.Elements.Single(element => element.Id == "about-close").Action,
            Is.EqualTo(UiAction.MainMenuAboutClose)
        );
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
