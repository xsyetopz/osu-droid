using NUnit.Framework;
using OsuDroid.Game.Localization;
using OsuDroid.Game.Scenes.Options;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Tests;

public sealed partial class OptionsSceneTests
{

    [Test]
    public void LocalizerReturnsEnglishFallbackAndMissingKey()
    {
        var localizer = new GameLocalizer();

        Assert.That(localizer["Options_Title"], Is.EqualTo("Settings"));
        Assert.That(localizer["missing.key"], Is.EqualTo("missing.key"));
    }
    [Test]
    public void OptionsSceneListsAndroidSourceSectionsAndGeneralRows()
    {
        var scene = new OptionsScene(new GameLocalizer());

        Assert.That(scene.Sections, Is.EqualTo(new[] { "General", "Gameplay", "Graphics", "Audio", "Library", "Input", "Advanced" }));
        Assert.That(scene.GeneralCategories, Is.EqualTo(new[] { "Online", "Account", "Community", "Updates", "Backup", "Localization" }));
        Assert.That(scene.GeneralRows, Is.EqualTo(new[]
        {
            "Server Connection",
            "Load Avatar",
            "Difficulty algorithm",
            "Login",
            "Password",
            "Register",
            "Receive announcements",
            "Check for updates",
            "Export options file",
            "Import options file",
            "Language",
        }));
    }

    [Test]
    public void OptionsSceneGameplayGraphicsAndInputRowsMatchAndroidSourceCatalog()
    {
        var scene = new OptionsScene(new GameLocalizer());
        var viewport = VirtualViewport.FromSurface(1280, 720);

        scene.HandleAction(UiAction.OptionsSectionGameplay, viewport);
        Assert.That(scene.ActiveRows, Does.Contain("Background brightness"));
        Assert.That(scene.ActiveRows, Does.Contain("Set playfield size"));
        Assert.That(scene.ActiveRows, Does.Contain("Horizontal position"));
        Assert.That(scene.ActiveRows, Does.Contain("Vertical position"));
        Assert.That(scene.ActiveRows, Does.Contain("Combo 1"));
        Assert.That(scene.GetIntValue("bgbrightness"), Is.EqualTo(25));
        Assert.That(scene.GetIntValue("playfieldSize"), Is.EqualTo(100));
        Assert.That(scene.GetIntValue("playfieldHorizontalPosition"), Is.EqualTo(50));
        Assert.That(scene.GetIntValue("playfieldVerticalPosition"), Is.EqualTo(50));

        scene.HandleAction(UiAction.OptionsSectionGraphics, viewport);
        Assert.That(scene.ActiveRows, Does.Contain("Select skin"));
        Assert.That(scene.ActiveRows, Does.Contain("Spinner style"));
        Assert.That(scene.ActiveRows, Does.Contain("Cursor Size"));
        Assert.That(scene.GetIntValue("cursorSize"), Is.EqualTo(50));

        scene.HandleAction(UiAction.OptionsSectionInput, viewport);
        Assert.That(scene.ActiveRows, Does.Contain("Back button press time"));
        Assert.That(scene.ActiveRows, Does.Contain("Set Intensity"));
        Assert.That(scene.GetIntValue("back_button_press_time"), Is.EqualTo(300));
        Assert.That(scene.GetIntValue("seekBarVibrateIntensity"), Is.EqualTo(127));
    }

    [Test]
    public void OptionsSceneAdvancedDefaultsMatchAndroidSource()
    {
        var scene = new OptionsScene(new GameLocalizer());

        Assert.That(scene.GetBoolValue("safebeatmapbg"), Is.False);
    }
}
