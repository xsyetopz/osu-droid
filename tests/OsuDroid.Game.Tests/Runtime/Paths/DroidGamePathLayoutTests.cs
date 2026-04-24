using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class DroidGamePathLayoutTests
{
    [Test]
    public void LayoutUsesOsuDroidDirectoryNames()
    {
        string root = Path.Combine(TestContext.CurrentContext.WorkDirectory, "layout-root");
        string cache = Path.Combine(TestContext.CurrentContext.WorkDirectory, "layout-cache");
        var layout = new DroidGamePathLayout(new DroidPathRoots(root, cache));

        Assert.That(layout.CoreRoot, Is.EqualTo(Path.GetFullPath(root)));
        Assert.That(layout.CacheRoot, Is.EqualTo(Path.GetFullPath(cache)));
        Assert.That(layout.Songs, Is.EqualTo(Path.Combine(Path.GetFullPath(root), "Songs")));
        Assert.That(layout.Skin, Is.EqualTo(Path.Combine(Path.GetFullPath(root), "Skin")));
        Assert.That(layout.Scores, Is.EqualTo(Path.Combine(Path.GetFullPath(root), "Scores")));
        Assert.That(layout.Databases, Is.EqualTo(Path.Combine(Path.GetFullPath(root), "databases")));
        Assert.That(layout.Log, Is.EqualTo(Path.Combine(Path.GetFullPath(root), "Log")));
        Assert.That(layout.Downloads, Is.EqualTo(Path.Combine(Path.GetFullPath(cache), "Downloads")));
        Assert.That(layout.NoMedia, Is.EqualTo(Path.Combine(Path.GetFullPath(root), ".nomedia")));
    }

    [Test]
    public void DatabaseConstantsDelegateToPathLayout()
    {
        string root = Path.Combine(TestContext.CurrentContext.WorkDirectory, "database-layout");
        string expected = Path.Combine(Path.GetFullPath(root), "databases", "room-debug.db");

        Assert.That(DroidDatabaseConstants.GetDatabasePath(root, "debug"), Is.EqualTo(expected));
    }

    [Test]
    public void EnsureDirectoriesCreatesAndroidCompatibleRoots()
    {
        string root = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"layout-create-{Guid.NewGuid():N}");
        string cache = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"layout-cache-{Guid.NewGuid():N}");
        var layout = new DroidGamePathLayout(new DroidPathRoots(root, cache));

        layout.EnsureDirectories();

        Assert.That(Directory.Exists(layout.Songs), Is.True);
        Assert.That(Directory.Exists(layout.Skin), Is.True);
        Assert.That(Directory.Exists(layout.Scores), Is.True);
        Assert.That(Directory.Exists(layout.Databases), Is.True);
        Assert.That(Directory.Exists(layout.Log), Is.True);
        Assert.That(Directory.Exists(layout.CacheRoot), Is.True);
        Assert.That(Directory.Exists(layout.Downloads), Is.True);
        Assert.That(File.Exists(layout.NoMedia), Is.True);

        Directory.Delete(layout.CoreRoot, true);
        Directory.Delete(layout.CacheRoot, true);
    }

    [Test]
    public void AppDataRootsUseOsuDroidDirectoryNameAndMigrateOldName()
    {
        string parent = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"path-roots-{Guid.NewGuid():N}");
        string oldRoot = Path.Combine(parent, DroidPathRoots.LegacyCoreDirectoryName);
        string cache = Path.Combine(parent, "Caches");
        Directory.CreateDirectory(oldRoot);
        File.WriteAllText(Path.Combine(oldRoot, "marker.txt"), "legacy");

        var roots = DroidPathRoots.FromAppDataDirectory(parent, cache);

        Assert.That(roots.CoreRoot, Is.EqualTo(Path.Combine(parent, DroidPathRoots.CoreDirectoryName)));
        Assert.That(Directory.Exists(oldRoot), Is.False);
        Assert.That(File.Exists(Path.Combine(roots.CoreRoot, "marker.txt")), Is.True);

        Directory.Delete(parent, true);
    }
}
