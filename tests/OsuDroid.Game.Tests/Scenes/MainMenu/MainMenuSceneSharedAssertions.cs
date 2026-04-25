using NUnit.Framework;
using OsuDroid.Game.Scenes.MainMenu;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{
    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OsuDroid.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }

    private static string ResolveContentSourcePath(string repositoryRoot, string contentName)
    {
        const string contentPrefix = "droid/";
        if (!contentName.StartsWith(contentPrefix, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Unsupported asset content name: {contentName}", nameof(contentName));
        }

        string relativePath = contentName.Replace('/', Path.DirectorySeparatorChar) + ".png";
        return Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Resources", "Raw", "assets", relativePath);
    }

    private static UiFrameSnapshot ExpandedFrame(MainMenuScene scene, VirtualViewport viewport)
    {
        scene.ToggleCookie();
        scene.Update(TimeSpan.FromMilliseconds(MainMenuScene.MenuExpandDurationMilliseconds));
        return scene.CreateSnapshot(viewport).UiFrame;
    }




































    private static void AssertMusicControl(UiElementSnapshot element, string assetName, UiAction action, float androidIndex)
    {
        Assert.That(element.Kind, Is.EqualTo(UiElementKind.Sprite));
        Assert.That(element.AssetName, Is.EqualTo(assetName));
        AssertRectClose(element.Bounds, MainMenuScene.GetAndroidMusicControlBounds(androidIndex));
        Assert.That(element.Action, Is.EqualTo(action));
        Assert.That(element.Alpha, Is.EqualTo(1f));
    }















    private static void AssertRectClose(UiRect actual, UiRect expected)
    {
        Assert.That(actual.X, Is.EqualTo(expected.X).Within(0.001f));
        Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(0.001f));
        Assert.That(actual.Width, Is.EqualTo(expected.Width).Within(0.001f));
        Assert.That(actual.Height, Is.EqualTo(expected.Height).Within(0.001f));
    }
}
