using OsuDroid.Game.Localization;

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
        Assert.That(scene.GeneralRows, Does.Contain("Server Connection"));
        Assert.That(scene.GeneralRows, Does.Contain("Load Avatar"));
        Assert.That(scene.GeneralRows, Does.Contain("Difficulty algorithm"));
        Assert.That(scene.GeneralRows, Does.Contain("Language"));
    }
}
