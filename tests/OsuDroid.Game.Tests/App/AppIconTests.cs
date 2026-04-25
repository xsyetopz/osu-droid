using NUnit.Framework;
namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

    [Test]
    public void MobileProjectUsesContentPipelineForStaticDroidImages()
    {
        string repositoryRoot = FindRepositoryRoot();
        string project = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "OsuDroid.App.csproj"));
        string content = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Content", "DroidAssets.mgcb"));

        Assert.That(project, Does.Contain("<MonoGameContentReference Include=\"Content\\DroidAssets.mgcb\" />"));
        Assert.That(project, Does.Not.Contain("Resources\\Raw\\assets\\droid\\**\\*.png"));
        Assert.That(project, Does.Not.Contain("Resources\\Raw\\assets\\droid\\**\\*.jpg"));
        Assert.That(project, Does.Contain("Resources\\Raw\\assets\\droid\\**\\*.ogg"));
        Assert.That(content, Does.Contain("/build:../Resources/Raw/assets/droid/main-menu/background.png;droid/main-menu/background"));
    }
    [Test]
    public void AppIconUsesOriginalOsuDroidLauncherArtwork()
    {
        string repositoryRoot = FindRepositoryRoot();
        byte[] appIcon = File.ReadAllBytes(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Resources", "AppIcon", "appicon.png"));
        byte[] referenceIcon = File.ReadAllBytes(Path.Combine(repositoryRoot, "third_party", "osu-droid-legacy", "res", "drawable-xxxhdpi", "ic_launcher.png"));

        Assert.That(appIcon, Is.EqualTo(referenceIcon));
    }
    [Test]
    public void AndroidPlatformDeclaresOsuDroidLauncherIcon()
    {
        string repositoryRoot = FindRepositoryRoot();
        string mainActivity = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "Android", "MainActivity.cs"));
        string mainApplication = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "Android", "MainApplication.cs"));
        string[] densities = new[] { "ldpi", "mdpi", "hdpi", "xhdpi", "xxhdpi", "xxxhdpi" };

        Assert.That(mainActivity, Does.Contain("Icon = \"@drawable/ic_launcher\""));
        Assert.That(mainApplication, Does.Contain("[Application(Icon = \"@drawable/ic_launcher\")]"));
        foreach (string? density in densities)
        {
            string androidIcon = Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "Android", "Resources", $"drawable-{density}", "ic_launcher.png");
            string referenceIcon = Path.Combine(repositoryRoot, "third_party", "osu-droid-legacy", "res", $"drawable-{density}", "ic_launcher.png");

            Assert.That(File.Exists(androidIcon), Is.True, density);
            Assert.That(File.ReadAllBytes(androidIcon), Is.EqualTo(File.ReadAllBytes(referenceIcon)), density);
        }
    }
    [Test]
    public void MobileProjectDeclaresIconSources()
    {
        string repositoryRoot = FindRepositoryRoot();
        string project = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "OsuDroid.App.csproj"));
        string infoPlist = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "iOS", "Info.plist"));

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
        string repositoryRoot = FindRepositoryRoot();
        string icons = Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "iOS", "Icons");
        string[] expectedFiles = new[]
        {
            "Icon-60@2x.png",
            "Icon-60@3x.png",
            "Icon-76@2x.png",
            "Icon-83.5@2x.png",
        };

        foreach (string? file in expectedFiles)
        {
            string path = Path.Combine(icons, file);
            Assert.That(new FileInfo(path).Length, Is.GreaterThan(0), file);
        }
    }
    [Test]
    public void IosBundleVerifierRequiresDirectAppIcons()
    {
        string repositoryRoot = FindRepositoryRoot();
        string script = File.ReadAllText(Path.Combine(repositoryRoot, "scripts", "verify-ios-bundle.sh"));
        string infoPlist = File.ReadAllText(Path.Combine(repositoryRoot, "src", "OsuDroid.App", "Platforms", "iOS", "Info.plist"));

        Assert.That(infoPlist, Does.Contain("<key>CFBundleIcons</key>"));
        Assert.That(infoPlist, Does.Contain("<string>Icon-60</string>"));
        Assert.That(infoPlist, Does.Not.Contain("XSAppIconAssets"));
        Assert.That(script, Does.Contain("Icon-60@2x.png Icon-60@3x.png Icon-76@2x.png Icon-83.5@2x.png"));
        Assert.That(script, Does.Contain("iOS app icon file missing from app bundle"));
    }
}
