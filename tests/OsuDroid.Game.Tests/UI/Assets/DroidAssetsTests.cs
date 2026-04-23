namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

    [Test]
    public void MainMenuUsesDroidAssetProvenance()
    {
        UiAssetManifest manifest = DroidAssets.MainMenuManifest;

        Assert.That(manifest.Get(DroidAssets.Play).ContentName, Is.EqualTo("droid/main-menu/play-button"));
        Assert.That(manifest.Get(DroidAssets.Play).Provenance, Is.EqualTo(UiAssetProvenance.OsuDroid));
        Assert.That(manifest.Get(DroidAssets.Logo).NativeSize, Is.EqualTo(new UiSize(540, 540)));
        Assert.That(manifest.Get(DroidAssets.MenuBackground).ContentName, Is.EqualTo("droid/main-menu/background"));
        Assert.That(manifest.Get(DroidAssets.BeatmapDownloader).NativeSize, Is.EqualTo(new UiSize(80, 284)));
        Assert.That(manifest.Get(DroidAssets.MusicNowPlaying).NativeSize, Is.EqualTo(new UiSize(1364, 60)));
        Assert.That(manifest.Get(DroidAssets.MusicPrevious).NativeSize, Is.EqualTo(new UiSize(64, 62)));
        Assert.That(manifest.Get(DroidAssets.MusicPause).NativeSize, Is.EqualTo(new UiSize(66, 66)));
    }
    [Test]
    public void MainMenuContentPipelineSourcesExistOnDisk()
    {
        string repositoryRoot = FindRepositoryRoot();

        foreach (UiAssetEntry? asset in DroidAssets.MainMenuManifest.Entries.Where(entry => entry.Kind == UiAssetKind.Texture))
        {
            Assert.That(asset.ContentName, Does.StartWith("droid/"));
            Assert.That(asset.ContentName, Does.Not.Contain(".png"));
            Assert.That(asset.ContentName, Does.Not.Contain(".jpg"));
            string sourcePath = ResolveContentSourcePath(repositoryRoot, asset.ContentName);
            Assert.That(File.Exists(sourcePath), Is.True, asset.ContentName);
        }
    }
    [Test]
    public void StartupManifestIsSmallPreCoreLoadingSet()
    {
        string[] assetNames = DroidAssets.StartupManifest.Entries.Select(entry => entry.LogicalName).ToArray();

        Assert.That(assetNames, Is.EquivalentTo(new[] { DroidAssets.Loading, DroidAssets.LoadingTitle, DroidAssets.Welcome }));
    }

    [Test]
    public void DroidRuntimeAssetPathsAreCentralizedInCatalog()
    {
        Assert.That(DroidAssets.MainMenuManifest.Entries, Is.EquivalentTo(DroidAssets.Catalog));
        Assert.That(DroidAssets.StartupManifest.Entries, Is.SubsetOf(DroidAssets.Catalog));

        string repositoryRoot = FindRepositoryRoot();
        string droidAssetToken = "assets/" + "droid/";
        string allowedCatalogPath = Path.Combine(repositoryRoot, "src", "OsuDroid.Game", "UI", "Assets", "DroidAssetCatalog.cs");
        string[] offenders = Directory
            .EnumerateFiles(Path.Combine(repositoryRoot, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .Where(path => !Path.GetFullPath(path).Equals(allowedCatalogPath, StringComparison.Ordinal))
            .Where(path => File.ReadAllText(path).Contains(droidAssetToken, StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(repositoryRoot, path))
            .ToArray();

        Assert.That(offenders, Is.Empty);
    }
}
