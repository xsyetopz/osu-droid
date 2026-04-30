using NUnit.Framework;
using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Runtime.Settings;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed partial class BeatmapImportTests
{
    [Test]
    public void ParserReadsMetadataForSongSelect()
    {
        string root = CreateTempDirectory();
        string songs = Path.Combine(root, "Songs");
        string set = Path.Combine(songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        string osu = Path.Combine(set, "map.osu");
        File.WriteAllText(osu, SampleOsu());

        BeatmapInfo beatmap = BeatmapFileParser.Parse(osu, songs);

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
        string root = CreateTempDirectory();
        string songs = Path.Combine(root, "Songs");
        string set = Path.Combine(songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        string osu = Path.Combine(set, "map.osu");
        File.WriteAllText(osu, SampleOsu(includeMode: false));

        BeatmapInfo beatmap = BeatmapFileParser.Parse(osu, songs);

        Assert.That(beatmap.Version, Is.EqualTo("Hard"));
    }

    [Test]
    public void ImportExtractsOszIntoSongsAndIndexesSqlite()
    {
        DroidGamePathLayout roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        var importer = new BeatmapImportService(roots, library);
        string archive = Path.Combine(roots.Downloads, "123 Artist - Title.osz");
        Directory.CreateDirectory(roots.Downloads);
        CreateOsz(archive, "map.osu", SampleOsu());

        BeatmapImportResult importResult = importer.ImportOsz(archive);
        BeatmapLibrarySnapshot snapshot = repository.LoadLibrary();

        Assert.That(importResult.IsSuccess, Is.True);
        Assert.That(File.Exists(archive), Is.False);
        Assert.That(Directory.Exists(Path.Combine(roots.Songs, "123 Artist - Title")), Is.True);
        Assert.That(snapshot.Sets, Has.Count.EqualTo(1));
        Assert.That(snapshot.Sets[0].Beatmaps[0].Title, Is.EqualTo("Title"));
    }

    [Test]
    public void ImportAppliesOnlineMetadataToDownloadedDifficulties()
    {
        DroidGamePathLayout roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        var importer = new BeatmapImportService(roots, library);
        string archive = Path.Combine(roots.Downloads, "123 Artist - Title.osz");
        Directory.CreateDirectory(roots.Downloads);
        CreateOsz(archive, "map.osu", SampleOsu());
        var metadata = new BeatmapOnlineMetadata(
            123,
            BeatmapRankedStatus.Ranked,
            [new BeatmapOnlineDifficultyMetadata(456, "Hard", 3.96f)]
        );

        BeatmapImportResult importResult = importer.ImportOsz(archive, onlineMetadata: metadata);
        BeatmapInfo beatmap = repository.LoadLibrary().Sets.Single().Beatmaps.Single();

        Assert.That(importResult.IsSuccess, Is.True);
        Assert.That(beatmap.Status, Is.EqualTo((int)BeatmapRankedStatus.Ranked));
        Assert.That(beatmap.DroidStarRating, Is.Not.EqualTo(3.96f));
        Assert.That(beatmap.StandardStarRating, Is.EqualTo(3.96f));
    }

    [Test]
    public void ImportIgnoresNonStandardDifficulties()
    {
        DroidGamePathLayout roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        var importer = new BeatmapImportService(roots, library);
        string archive = Path.Combine(roots.Downloads, "123 Artist - Title.osz");
        Directory.CreateDirectory(roots.Downloads);
        CreateOsz(
            archive,
            new Dictionary<string, string>
            {
                ["standard.osu"] = SampleOsu(mode: 0, version: "Standard"),
                ["taiko.osu"] = SampleOsu(mode: 1, version: "Taiko"),
                ["catch.osu"] = SampleOsu(mode: 2, version: "Catch"),
                ["mania.osu"] = SampleOsu(mode: 3, version: "Mania"),
            }
        );

        BeatmapImportResult importResult = importer.ImportOsz(archive);
        BeatmapLibrarySnapshot snapshot = repository.LoadLibrary();

        Assert.That(importResult.IsSuccess, Is.True);
        Assert.That(
            File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "standard.osu")),
            Is.True
        );
        Assert.That(
            File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "taiko.osu")),
            Is.True
        );
        Assert.That(
            File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "catch.osu")),
            Is.True
        );
        Assert.That(
            File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "mania.osu")),
            Is.True
        );
        Assert.That(snapshot.Sets, Has.Count.EqualTo(1));
        Assert.That(snapshot.Sets[0].Beatmaps, Has.Count.EqualTo(1));
        Assert.That(snapshot.Sets[0].Beatmaps[0].Version, Is.EqualTo("Standard"));
    }

    [Test]
    public void ImportExtractsAllNonStandardFilesWithoutVisibleSongSelectRows()
    {
        DroidGamePathLayout roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        var importer = new BeatmapImportService(roots, library);
        string archive = Path.Combine(roots.Downloads, "123 Artist - Title.osz");
        Directory.CreateDirectory(roots.Downloads);
        CreateOsz(
            archive,
            new Dictionary<string, string>
            {
                ["taiko.osu"] = SampleOsu(mode: 1, version: "Taiko"),
                ["catch.osu"] = SampleOsu(mode: 2, version: "Catch"),
                ["mania.osu"] = SampleOsu(mode: 3, version: "Mania"),
            }
        );

        BeatmapImportResult importResult = importer.ImportOsz(archive);
        BeatmapLibrarySnapshot snapshot = repository.LoadLibrary();

        Assert.That(importResult.IsSuccess, Is.True);
        Assert.That(
            File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "taiko.osu")),
            Is.True
        );
        Assert.That(
            File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "catch.osu")),
            Is.True
        );
        Assert.That(
            File.Exists(Path.Combine(roots.Songs, "123 Artist - Title", "mania.osu")),
            Is.True
        );
        Assert.That(snapshot.Sets, Is.Empty);
    }

    [Test]
    public void ScanRefreshRemovesStaleNonStandardIndexRowsWithoutDeletingFiles()
    {
        DroidGamePathLayout roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        string set = Path.Combine(roots.Songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        string standardFile = Path.Combine(set, "standard.osu");
        string taikoFile = Path.Combine(set, "taiko.osu");
        File.WriteAllText(standardFile, SampleOsu(mode: 0, version: "Standard"));
        File.WriteAllText(taikoFile, SampleOsu(mode: 1, version: "Taiko"));
        BeatmapInfo standard = BeatmapFileParser.Parse(standardFile, roots.Songs);
        repository.UpsertBeatmaps([
            standard,
            standard with
            {
                Filename = "taiko.osu",
                Md5 = "stale-taiko",
                Version = "Taiko",
            },
        ]);

        BeatmapLibrarySnapshot snapshot = library.Scan();

        Assert.That(File.Exists(standardFile), Is.True);
        Assert.That(File.Exists(taikoFile), Is.True);
        Assert.That(snapshot.Sets, Has.Count.EqualTo(1));
        Assert.That(
            snapshot.Sets[0].Beatmaps.Select(beatmap => beatmap.Version),
            Is.EqualTo(new[] { "Standard" })
        );
    }

    [Test]
    public void ScanKeepsUnimportedBeatmapsByDefault()
    {
        DroidGamePathLayout roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        string set = Path.Combine(roots.Songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        string taikoFile = Path.Combine(set, "taiko.osu");
        File.WriteAllText(taikoFile, SampleOsu(mode: 1, version: "Taiko"));

        BeatmapLibrarySnapshot snapshot = library.Scan();

        Assert.That(snapshot.Sets, Is.Empty);
        Assert.That(File.Exists(taikoFile), Is.True);
        Assert.That(Directory.Exists(set), Is.True);
    }

    [Test]
    public void ScanDeletesUnimportedBeatmapsWhenOptionIsEnabled()
    {
        DroidGamePathLayout roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var settings = CreateSettings(roots);
        settings.SetBool("deleteUnimportedBeatmaps", true);
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository, settings);
        string invalidSet = Path.Combine(roots.Songs, "123 Artist - Title");
        Directory.CreateDirectory(invalidSet);
        string taikoFile = Path.Combine(invalidSet, "taiko.osu");
        File.WriteAllText(taikoFile, SampleOsu(mode: 1, version: "Taiko"));
        string mixedSet = Path.Combine(roots.Songs, "124 Artist - Mixed");
        Directory.CreateDirectory(mixedSet);
        string standardFile = Path.Combine(mixedSet, "standard.osu");
        string catchFile = Path.Combine(mixedSet, "catch.osu");
        File.WriteAllText(standardFile, SampleOsu(mode: 0, version: "Standard"));
        File.WriteAllText(catchFile, SampleOsu(mode: 2, version: "Catch"));

        BeatmapLibrarySnapshot snapshot = library.Scan();

        Assert.That(snapshot.Sets, Has.Count.EqualTo(1));
        Assert.That(Directory.Exists(invalidSet), Is.False);
        Assert.That(File.Exists(catchFile), Is.False);
        Assert.That(File.Exists(standardFile), Is.True);
    }

    [Test]
    public void ScanDeletesOnlyUnsupportedVideoFilesWhenOptionIsEnabled()
    {
        DroidGamePathLayout roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var settings = CreateSettings(roots);
        settings.SetBool("deleteUnsupportedVideos", true);
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository, settings);
        string unsupportedSet = Path.Combine(roots.Songs, "123 Artist - Unsupported");
        Directory.CreateDirectory(unsupportedSet);
        string unsupportedVideo = Path.Combine(unsupportedSet, "video.avi");
        File.WriteAllText(
            Path.Combine(unsupportedSet, "map.osu"),
            SampleOsu(videoFilename: "video.avi")
        );
        File.WriteAllText(unsupportedVideo, "video");
        string supportedSet = Path.Combine(roots.Songs, "124 Artist - Supported");
        Directory.CreateDirectory(supportedSet);
        string supportedVideo = Path.Combine(supportedSet, "video.mp4");
        File.WriteAllText(
            Path.Combine(supportedSet, "map.osu"),
            SampleOsu(videoFilename: "video.mp4")
        );
        File.WriteAllText(supportedVideo, "video");

        BeatmapLibrarySnapshot snapshot = library.Scan();

        Assert.That(snapshot.Sets, Has.Count.EqualTo(2));
        Assert.That(File.Exists(unsupportedVideo), Is.False);
        Assert.That(File.Exists(supportedVideo), Is.True);
    }

    [Test]
    public void ProcessingImportsDownloadsAndKeepsArchiveWhenOptionsRequestIt()
    {
        DroidGamePathLayout roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var settings = CreateSettings(roots);
        settings.SetBool("scandownload", true);
        settings.SetBool("deleteosz", false);
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository, settings);
        var importer = new BeatmapImportService(roots, library);
        var processing = new BeatmapProcessingService(roots, importer, library, settings);
        string archive = Path.Combine(roots.Downloads, "123 Artist - Title.osz");
        Directory.CreateDirectory(roots.Downloads);
        CreateOsz(archive, "map.osu", SampleOsu());

        processing.Start();
        BeatmapLibrarySnapshot snapshot = WaitForProcessing(processing);

        Assert.That(snapshot.Sets, Has.Count.EqualTo(1));
        Assert.That(File.Exists(archive), Is.True);
    }

    [Test]
    public void LoadHidesStaleNonStandardIndexRowsBeforeRefreshCompletes()
    {
        DroidGamePathLayout roots = CreatePathLayout();
        var database = new DroidDatabase(roots.GetDatabasePath("debug"));
        database.EnsureCreated();
        var repository = new BeatmapLibraryRepository(database);
        var library = new BeatmapLibrary(roots, repository);
        string set = Path.Combine(roots.Songs, "123 Artist - Title");
        Directory.CreateDirectory(set);
        string standardFile = Path.Combine(set, "standard.osu");
        string taikoFile = Path.Combine(set, "taiko.osu");
        File.WriteAllText(standardFile, SampleOsu(mode: 0, version: "Standard"));
        File.WriteAllText(taikoFile, SampleOsu(mode: 1, version: "Roko-Don's Taiko"));
        BeatmapInfo standard = BeatmapFileParser.Parse(standardFile, roots.Songs);
        repository.UpsertBeatmaps([
            standard,
            standard with
            {
                Filename = "taiko.osu",
                Md5 = "stale-taiko",
                Version = "Roko-Don's Taiko",
            },
        ]);

        BeatmapLibrarySnapshot snapshot = library.Load();

        Assert.That(File.Exists(taikoFile), Is.True);
        Assert.That(snapshot.Sets, Has.Count.EqualTo(1));
        Assert.That(
            snapshot.Sets[0].Beatmaps.Select(beatmap => beatmap.Version),
            Is.EqualTo(new[] { "Standard" })
        );
    }
}
