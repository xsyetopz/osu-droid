using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

    [Test]
    public void MobileProjectUsesContentPipelineForStaticDroidImages()
    {
        var repositoryRoot = FindRepositoryRoot();
        var project = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "OsuDroid.App.csproj"));
        var content = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Content", "DroidAssets.mgcb"));

        Assert.That(project, Does.Contain("<MonoGameContentReference Include=\"Content\\DroidAssets.mgcb\" />"));
        Assert.That(project, Does.Not.Contain("Resources\\Raw\\assets\\droid\\**\\*.png"));
        Assert.That(project, Does.Not.Contain("Resources\\Raw\\assets\\droid\\**\\*.jpg"));
        Assert.That(project, Does.Contain("Resources\\Raw\\assets\\droid\\**\\*.ogg"));
        Assert.That(content, Does.Contain("/build:../Resources/Raw/assets/droid/main-menu/background.png;droid/main-menu/background"));
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
}
