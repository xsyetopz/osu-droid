using System.Reflection;

namespace OsuDroid.Game.Tests;

public class ResourceAndCompositionSmokeTests
{
    [Test]
    public void OsuGameResourcesAssemblyContainsRequiredMenuAssets()
    {
        Assembly assembly = typeof(osu.Game.Resources.Localisation.Web.HomeStrings).Assembly;
        string[] resourceNames = assembly.GetManifestResourceNames();

        Assert.Multiple(() =>
        {
            Assert.That(resourceNames, Has.Some.Contains("Textures.Menu.logo.png"));
            Assert.That(resourceNames, Has.Some.Contains("Textures.Menu.menu-background-1.jpg"));
            Assert.That(resourceNames, Has.Some.Contains("Samples.Menu.osu-logo-swoosh.wav"));
        });
    }

    [Test]
    public void MobileHeadsDoNotReferenceStubServices()
    {
        string root = TestContext.CurrentContext.TestDirectory;
        string repoRoot = Path.GetFullPath(Path.Combine(root, "..", "..", ".."));

        string androidHead = File.ReadAllText(Path.Combine(repoRoot, "src", "OsuDroid.Android", "MainActivity.cs"));
        string iosHead = File.ReadAllText(Path.Combine(repoRoot, "src", "OsuDroid.iOS", "AppDelegate.cs"));

        Assert.Multiple(() =>
        {
            Assert.That(androidHead, Does.Not.Contain("Services.Stubs"));
            Assert.That(androidHead, Does.Not.Contain("StubBeatmapLibraryService"));
            Assert.That(iosHead, Does.Not.Contain("Services.Stubs"));
            Assert.That(iosHead, Does.Not.Contain("StubBeatmapLibraryService"));
        });
    }
}
