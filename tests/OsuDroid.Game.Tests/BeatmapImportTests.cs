using System.IO.Compression;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class BeatmapImportTests
{
    [Test]
    public void ParserReadsMetadataForSongSelect()
    {
        var root = CreateTempDirectory();
        var songs = Path.Combine(root, "Songs");
        var set = Path.Combine(songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        var osu = Path.Combine(set, "map.osu");
        File.WriteAllText(osu, SampleOsu());

        var beatmap = BeatmapFileParser.Parse(osu, songs);

        Assert.That(beatmap.SetDirectory, Is.EqualTo("123 Artist - Title"));
        Assert.That(beatmap.AudioFilename, Is.EqualTo("audio.mp3"));
        Assert.That(beatmap.BackgroundFilename, Is.EqualTo("bg.jpg"));
        Assert.That(beatmap.PreviewTime, Is.EqualTo(45000));
        Assert.That(beatmap.Title, Is.EqualTo("Title"));
        Assert.That(beatmap.Artist, Is.EqualTo("Artist"));
        Assert.That(beatmap.HitCircleCount, Is.EqualTo(1));
        Assert.That(beatmap.SliderCount, Is.EqualTo(1));
        Assert.That(beatmap.SpinnerCount, Is.EqualTo(1));
        Assert.That(beatmap.Length, Is.EqualTo(4000));
    }

    [Test]
    public void ParserTreatsMissingModeAsStandard()
    {
        var root = CreateTempDirectory();
        var songs = Path.Combine(root, "Songs");
        var set = Path.Combine(songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        var osu = Path.Combine(set, "map.osu");
        File.WriteAllText(osu, SampleOsu(includeMode: false));

        var beatmap = BeatmapFileParser.Parse(osu, songs);

        Assert.That(beatmap.Version, Is.EqualTo("Hard"));
    }

    [Test]
    public void ImportExtractsOszIntoSongsAndIndexesSqlite()
    {
        var roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        var importer = new BeatmapImportService(roots, library);
        var archive = Path.Combine(roots.Downloads, "123 Artist - Title.osz");
        Directory.CreateDirectory(roots.Downloads);
        CreateOsz(archive, "map.osu", SampleOsu());

        var importResult = importer.ImportOsz(archive);
        var snapshot = repository.LoadLibrary();

        Assert.That(importResult.IsSuccess, Is.True);
        Assert.That(File.Exists(archive), Is.False);
        Assert.That(Directory.Exists(Path.Combine(roots.Songs, "123 Artist - Title")), Is.True);
        Assert.That(snapshot.Sets, Has.Count.EqualTo(1));
        Assert.That(snapshot.Sets[0].Beatmaps[0].Title, Is.EqualTo("Title"));
    }

    [Test]
    public void ImportIgnoresNonStandardDifficulties()
    {
        var roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        var importer = new BeatmapImportService(roots, library);
        var archive = Path.Combine(roots.Downloads, "123 Artist - Title.osz");
        Directory.CreateDirectory(roots.Downloads);
        CreateOsz(archive, new Dictionary<string, string>
        {
            ["standard.osu"] = SampleOsu(mode: 0, version: "Standard"),
            ["taiko.osu"] = SampleOsu(mode: 1, version: "Taiko"),
            ["catch.osu"] = SampleOsu(mode: 2, version: "Catch"),
            ["mania.osu"] = SampleOsu(mode: 3, version: "Mania"),
        });

        var importResult = importer.ImportOsz(archive);
        var snapshot = repository.LoadLibrary();

        Assert.That(importResult.IsSuccess, Is.True);
        Assert.That(File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "standard.osu")), Is.True);
        Assert.That(File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "taiko.osu")), Is.True);
        Assert.That(File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "catch.osu")), Is.True);
        Assert.That(File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "mania.osu")), Is.True);
        Assert.That(snapshot.Sets, Has.Count.EqualTo(1));
        Assert.That(snapshot.Sets[0].Beatmaps, Has.Count.EqualTo(1));
        Assert.That(snapshot.Sets[0].Beatmaps[0].Version, Is.EqualTo("Standard"));
    }

    [Test]
    public void ImportExtractsAllNonStandardFilesWithoutVisibleSongSelectRows()
    {
        var roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        var importer = new BeatmapImportService(roots, library);
        var archive = Path.Combine(roots.Downloads, "123 Artist - Title.osz");
        Directory.CreateDirectory(roots.Downloads);
        CreateOsz(archive, new Dictionary<string, string>
        {
            ["taiko.osu"] = SampleOsu(mode: 1, version: "Taiko"),
            ["catch.osu"] = SampleOsu(mode: 2, version: "Catch"),
            ["mania.osu"] = SampleOsu(mode: 3, version: "Mania"),
        });

        var importResult = importer.ImportOsz(archive);
        var snapshot = repository.LoadLibrary();

        Assert.That(importResult.IsSuccess, Is.True);
        Assert.That(File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "taiko.osu")), Is.True);
        Assert.That(File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "catch.osu")), Is.True);
        Assert.That(File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "mania.osu")), Is.True);
        Assert.That(snapshot.Sets, Is.Empty);
    }

    [Test]
    public void ScanRefreshRemovesStaleNonStandardIndexRowsWithoutDeletingFiles()
    {
        var roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        var set = Path.Combine(roots.Songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        var standardFile = Path.Combine(set, "standard.osu");
        var taikoFile = Path.Combine(set, "taiko.osu");
        File.WriteAllText(standardFile, SampleOsu(mode: 0, version: "Standard"));
        File.WriteAllText(taikoFile, SampleOsu(mode: 1, version: "Taiko"));
        var standard = BeatmapFileParser.Parse(standardFile, roots.Songs);
        repository.UpsertBeatmaps([
            standard,
            standard with
            {
                Filename = "taiko.osu",
                Md5 = "stale-taiko",
                Version = "Taiko",
            },
        ]);

        var snapshot = library.Scan();

        Assert.That(File.Exists(standardFile), Is.True);
        Assert.That(File.Exists(taikoFile), Is.True);
        Assert.That(snapshot.Sets, Has.Count.EqualTo(1));
        Assert.That(snapshot.Sets[0].Beatmaps.Select(beatmap => beatmap.Version), Is.EqualTo(new[] { "Standard" }));
    }

    [Test]
    public void LoadHidesStaleNonStandardIndexRowsBeforeRefreshCompletes()
    {
        var roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        var set = Path.Combine(roots.Songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        var standardFile = Path.Combine(set, "standard.osu");
        var taikoFile = Path.Combine(set, "taiko.osu");
        File.WriteAllText(standardFile, SampleOsu(mode: 0, version: "Standard"));
        File.WriteAllText(taikoFile, SampleOsu(mode: 1, version: "Roko-Don's Taiko"));
        var standard = BeatmapFileParser.Parse(standardFile, roots.Songs);
        repository.UpsertBeatmaps([
            standard,
            standard with
            {
                Filename = "taiko.osu",
                Md5 = "stale-taiko",
                Version = "Roko-Don's Taiko",
            },
        ]);

        var snapshot = library.Load();

        Assert.That(File.Exists(taikoFile), Is.True);
        Assert.That(snapshot.Sets, Has.Count.EqualTo(1));
        Assert.That(snapshot.Sets[0].Beatmaps.Select(beatmap => beatmap.Version), Is.EqualTo(new[] { "Standard" }));
    }

    [Test]
    public void ParserRejectsNonStandardBeatmapMode()
    {
        var root = CreateTempDirectory();
        var songs = Path.Combine(root, "Songs");
        var set = Path.Combine(songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        var osu = Path.Combine(set, "taiko.osu");
        File.WriteAllText(osu, SampleOsu(mode: 1, version: "Taiko"));

        Assert.Throws<NotSupportedException>(() => BeatmapFileParser.Parse(osu, songs));
    }

    [Test]
    public void ParserRejectsUnknownNonZeroBeatmapMode()
    {
        var root = CreateTempDirectory();
        var songs = Path.Combine(root, "Songs");
        var set = Path.Combine(songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        var osu = Path.Combine(set, "unknown.osu");
        File.WriteAllText(osu, SampleOsu(mode: 4, version: "Unknown"));

        Assert.Throws<NotSupportedException>(() => BeatmapFileParser.Parse(osu, songs));
    }

    [Test]
    public void ImportRejectsZipSlipArchive()
    {
        var roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        var importer = new BeatmapImportService(roots, library);
        var archive = Path.Combine(roots.Downloads, "bad.osz");
        Directory.CreateDirectory(roots.Downloads);
        CreateOsz(archive, "../escaped.osu", SampleOsu());

        var importResult = importer.ImportOsz(archive);

        Assert.That(importResult.IsSuccess, Is.False);
        Assert.That(File.Exists(Path.Combine(roots.Songs, "escaped.osu")), Is.False);
    }

    [Test]
    public void SanitizerMatchesDroidInvalidCharacters()
    {
        Assert.That(BeatmapImportService.SanitizeArchiveName("a\\b/c:d*e?f\"g<h>i|j"), Is.EqualTo("a_b_c_d_e_f_g_h_i_j"));
    }

    private static DroidGamePathLayout CreatePathLayout()
    {
        var root = CreateTempDirectory();
        var layout = new DroidGamePathLayout(new DroidPathRoots(root, Path.Combine(root, "cache")));
        layout.EnsureDirectories();
        return layout;
    }

    private static void CreateOsz(string archivePath, string entryName, string osuText)
    {
        CreateOsz(archivePath, new Dictionary<string, string> { [entryName] = osuText });
    }

    private static void CreateOsz(string archivePath, IReadOnlyDictionary<string, string> entries)
    {
        using var archive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
        foreach (var (entryName, osuText) in entries)
        {
            var entry = archive.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(osuText);
        }
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string SampleOsu(int mode = 0, string version = "Hard", bool includeMode = true)
    {
        var modeLine = includeMode ? $"Mode:{mode}" : string.Empty;
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
