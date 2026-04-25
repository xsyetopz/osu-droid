namespace OsuDroid.Game.Tests.UI;

public sealed class DroidUiCentralizationTests
{
    private static readonly string[] s_sceneFiles =
    [
        "src/OsuDroid.Game/Scenes/BeatmapDownloader",
        "src/OsuDroid.Game/Scenes/ModSelect",
        "src/OsuDroid.Game/Scenes/Options",
        "src/OsuDroid.Game/Scenes/SongSelect",
    ];

    [Test]
    public void NonSkinSceneUiDoesNotDefineRawPaletteColors()
    {
        string root = FindRepoRoot();
        string[] offenders = s_sceneFiles
            .SelectMany(path => Directory.EnumerateFiles(Path.Combine(root, path), "*.cs", SearchOption.AllDirectories))
            .Where(path => !path.EndsWith("SongSelectScene.RowLayout.cs", StringComparison.Ordinal))
            .Where(path => File.ReadAllText(path).Contains("UiColor.Opaque(", StringComparison.Ordinal) || File.ReadAllText(path).Contains("new UiColor(", StringComparison.Ordinal))
            .Select(path => Path.GetRelativePath(root, path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.That(offenders, Is.Empty);
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OsuDroid.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
