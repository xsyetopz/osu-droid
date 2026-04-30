using NUnit.Framework;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Composition;
using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes.MainMenu;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{
    [Test]
    public void GameCorePublishesExitRouteAfterMainMenuExitAnimation()
    {
        var database = new DroidDatabase(
            Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                $"main-menu-exit-{Guid.NewGuid():N}.db"
            )
        );
        database.EnsureCreated();
        var music = new RecordingMenuMusicController();
        var core = new OsuDroidGameCore(
            new GameServices(
                database,
                new DroidGamePathLayout(
                    DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)
                ),
                "test",
                "1.0",
                MusicController: music
            )
        );

        core.HandleUiAction(UiAction.MainMenuCookie);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        core.HandleUiAction(UiAction.MainMenuTertiaryButton);

        Assert.That(core.LastRoute, Is.EqualTo(MainMenuRoute.None));
        Assert.That(
            core.CreateFrame(VirtualViewport.FromSurface(1280, 720))
                .UiFrame.Elements.Any(element => element.Id == "exit-dialog-panel"),
            Is.True
        );

        core.HandleUiAction(UiAction.MainMenuExitConfirm);

        Assert.That(music.LastCommand, Is.EqualTo(MenuMusicCommand.Stop));
        Assert.That(core.LastRoute, Is.EqualTo(MainMenuRoute.None));

        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.ExitAnimationMilliseconds));

        Assert.That(core.LastRoute, Is.EqualTo(MainMenuRoute.Exit));
    }

    [Test]
    public void GameCoreCanCancelMainMenuExitDialog()
    {
        var database = new DroidDatabase(
            Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                $"main-menu-exit-cancel-{Guid.NewGuid():N}.db"
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

        core.HandleUiAction(UiAction.MainMenuCookie);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        core.HandleUiAction(UiAction.MainMenuTertiaryButton);
        core.HandleUiAction(UiAction.MainMenuExitCancel);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.ExitAnimationMilliseconds));

        Assert.That(core.LastRoute, Is.EqualTo(MainMenuRoute.None));
        Assert.That(
            core.CreateFrame(viewport)
                .UiFrame.Elements.Any(element => element.Id == "exit-dialog-panel"),
            Is.False
        );
        Assert.That(
            core.CreateFrame(viewport).UiFrame.Elements.Any(element => element.Id == "menu-0"),
            Is.True
        );
    }

    [Test]
    public void MainMenuTertiaryButtonButtonOpensExitDialogWithoutSeeya()
    {
        var database = new DroidDatabase(
            Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                $"main-menu-sfx-exit-dialog-{Guid.NewGuid():N}.db"
            )
        );
        database.EnsureCreated();
        var recorder = new RecordingMenuSfxPlayer();
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
        core.AttachPlatformServices(
            platformTextInputService: null,
            platformPreviewPlayer: null,
            recorder
        );
        var viewport = VirtualViewport.FromSurface(1280, 720);

        core.HandleUiAction(UiAction.MainMenuCookie);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        recorder.Keys.Clear();
        core.HandleUiAction(UiAction.MainMenuTertiaryButton);

        Assert.That(recorder.Keys, Is.EqualTo(new[] { "menuhit" }));
        Assert.That(
            core.CreateFrame(viewport)
                .UiFrame.Elements.Any(element => element.Id == "exit-dialog-panel"),
            Is.True
        );
    }

    [Test]
    public void MainMenuExitConfirmPlaysSeeyaAndStopsMusic()
    {
        var database = new DroidDatabase(
            Path.Combine(
                TestContext.CurrentContext.WorkDirectory,
                $"main-menu-sfx-exit-confirm-{Guid.NewGuid():N}.db"
            )
        );
        database.EnsureCreated();
        var recorder = new RecordingMenuSfxPlayer();
        var music = new RecordingMenuMusicController();
        var core = new OsuDroidGameCore(
            new GameServices(
                database,
                new DroidGamePathLayout(
                    DroidPathRoots.FromCoreRoot(TestContext.CurrentContext.WorkDirectory)
                ),
                "test",
                "1.0",
                MusicController: music
            )
        );
        core.AttachPlatformServices(
            platformTextInputService: null,
            platformPreviewPlayer: null,
            recorder
        );

        core.HandleUiAction(UiAction.MainMenuCookie);
        core.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        recorder.Keys.Clear();
        core.HandleUiAction(UiAction.MainMenuTertiaryButton);
        core.HandleUiAction(UiAction.MainMenuExitConfirm);

        Assert.That(recorder.Keys, Is.EqualTo(new[] { "menuhit", "seeya" }));
        Assert.That(music.LastCommand, Is.EqualTo(MenuMusicCommand.Stop));
        Assert.That(core.LastRoute, Is.EqualTo(MainMenuRoute.None));
    }
}
