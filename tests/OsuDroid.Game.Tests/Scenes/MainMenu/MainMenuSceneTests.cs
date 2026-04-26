using NUnit.Framework;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Scenes.MainMenu;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class MainMenuSceneTests
{
    [Test]
    public void MainMenuSwitchesToOsuDroidSecondMenu()
    {
        var scene = new MainMenuScene();
        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        MainMenuRoute route = scene.Handle(MainMenuAction.Activate);

        Assert.That(route, Is.EqualTo(MainMenuRoute.None));
        Assert.That(scene.Snapshot.IsSecondMenu, Is.True);
        Assert.That(
            scene.Snapshot.MenuEntries,
            Is.EqualTo(new[] { "Solo", "Multiplayer", "Back" })
        );
    }

    [Test]
    public void SoloRouteActivatesFromSecondMenu()
    {
        var scene = new MainMenuScene();
        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        scene.Handle(MainMenuAction.Activate);

        Assert.That(scene.Handle(MainMenuAction.Activate), Is.EqualTo(MainMenuRoute.Solo));
    }

    [Test]
    public void CoreCreatesDatabaseBackedShell()
    {
        string path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            $"core-{Guid.NewGuid():N}"
        );
        try
        {
            var core = OsuDroidGameCore.Create(path, "debug");
            Assert.That(
                File.Exists(DroidDatabaseConstants.GetDatabasePath(path, "debug")),
                Is.True
            );
            Assert.That(core.CurrentFrame.Scene, Is.EqualTo("MainMenu"));
        }
        finally
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}
