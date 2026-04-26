using System.IO.Compression;
using NUnit.Framework;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed partial class BeatmapImportTests
{
    [Test]
    public void ParserRejectsNonStandardBeatmapMode()
    {
        string root = CreateTempDirectory();
        string songs = Path.Combine(root, "Songs");
        string set = Path.Combine(songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        string osu = Path.Combine(set, "taiko.osu");
        File.WriteAllText(osu, SampleOsu(mode: 1, version: "Taiko"));

        Assert.Throws<NotSupportedException>(() => BeatmapFileParser.Parse(osu, songs));
    }

    [Test]
    public void ParserRejectsUnknownNonZeroBeatmapMode()
    {
        string root = CreateTempDirectory();
        string songs = Path.Combine(root, "Songs");
        string set = Path.Combine(songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        string osu = Path.Combine(set, "unknown.osu");
        File.WriteAllText(osu, SampleOsu(mode: 4, version: "Unknown"));

        Assert.Throws<NotSupportedException>(() => BeatmapFileParser.Parse(osu, songs));
    }

    [Test]
    public void ImportRejectsZipSlipArchive()
    {
        DroidGamePathLayout roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        var importer = new BeatmapImportService(roots, library);
        string archive = Path.Combine(roots.Downloads, "bad.osz");
        Directory.CreateDirectory(roots.Downloads);
        CreateOsz(archive, "../escaped.osu", SampleOsu());

        BeatmapImportResult importResult = importer.ImportOsz(archive);

        Assert.That(importResult.IsSuccess, Is.False);
        Assert.That(File.Exists(Path.Combine(roots.Songs, "escaped.osu")), Is.False);
    }

    [Test]
    public void SanitizerMatchesDroidInvalidCharacters() =>
        Assert.That(
            BeatmapImportService.SanitizeArchiveName("a\\b/c:d*e?f\"g<h>i|j"),
            Is.EqualTo("a_b_c_d_e_f_g_h_i_j")
        );

    private static DroidGamePathLayout CreatePathLayout()
    {
        string root = CreateTempDirectory();
        var layout = new DroidGamePathLayout(new DroidPathRoots(root, Path.Combine(root, "cache")));
        layout.EnsureDirectories();
        return layout;
    }

    private static void CreateOsz(string archivePath, string entryName, string osuText) =>
        CreateOsz(archivePath, new Dictionary<string, string> { [entryName] = osuText });

    private static void CreateOsz(string archivePath, IReadOnlyDictionary<string, string> entries)
    {
        using ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
        foreach ((string? entryName, string? osuText) in entries)
        {
            ZipArchiveEntry entry = archive.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(osuText);
        }
    }

    private static string CreateTempDirectory()
    {
        string path = Path.Combine(
            TestContext.CurrentContext.WorkDirectory,
            Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(path);
        return path;
    }

    private static string SampleOsu(int mode = 0, string version = "Hard", bool includeMode = true)
    {
        string modeLine = includeMode ? $"Mode:{mode}" : string.Empty;
        return $$"""
osu file format v14

[General]
AudioFilename: audio.mp3
PreviewTime: 45000
{{modeLine}}

[Metadata]
Title:Title
TitleUnicode:タイトル
Artist:Artist
ArtistUnicode:アーティスト
Creator:Mapper
Version:{{version}}
BeatmapID:456
BeatmapSetID:123
Tags:test tags
Source:source

[Difficulty]
HPDrainRate:5
CircleSize:4
OverallDifficulty:7
ApproachRate:9

[Events]
0,0,"bg.jpg",0,0

[TimingPoints]
0,500,4,2,1,60,1,0
2000,400,4,2,1,60,1,0

[HitObjects]
64,192,1000,1,0,0:0:0:0:
128,192,2500,2,0,B|160:192,1,100
256,192,4000,8,0,5000
""";
    }
}
