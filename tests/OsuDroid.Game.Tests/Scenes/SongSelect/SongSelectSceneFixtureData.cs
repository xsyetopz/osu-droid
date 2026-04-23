using OsuDroid.Game.Beatmaps;

namespace OsuDroid.Game.Tests;

public sealed partial class SongSelectSceneTests
{
    private static BeatmapLibrarySnapshot CreateSnapshot(string? background = null)
    {
        BeatmapInfo easy = CreateBeatmap("Easy", background, 2.4f);
        BeatmapInfo insane = CreateBeatmap("Insane", background, 4.8f);
        return new BeatmapLibrarySnapshot([new BeatmapSetInfo(1, "1 Artist - Title", [easy, insane])]);
    }

    private static BeatmapInfo CreateBeatmap(string version, string? background, float stars, int? status = null, int setId = 1, string setDirectory = "1 Artist - Title", string title = "Title", string artist = "Artist") => new(
        Filename: version + ".osu",
        SetDirectory: setDirectory,
        Md5: $"{setDirectory}:{version}",
        Id: null,
        AudioFilename: "audio.mp3",
        BackgroundFilename: background,
        Status: status,
        SetId: setId,
        Title: title,
        TitleUnicode: string.Empty,
        Artist: artist,
        ArtistUnicode: string.Empty,
        Creator: "Mapper",
        Version: version,
        Tags: string.Empty,
        Source: string.Empty,
        DateImported: 0,
        ApproachRate: 7,
        OverallDifficulty: 6,
        CircleSize: 4,
        HpDrainRate: 5,
        DroidStarRating: stars,
        StandardStarRating: stars + 0.1f,
        BpmMax: 180,
        BpmMin: 180,
        MostCommonBpm: 180,
        Length: 123000,
        PreviewTime: 45000,
        HitCircleCount: 100,
        SliderCount: 50,
        SpinnerCount: 1,
        MaxCombo: 200,
        EpilepsyWarning: false);

    private static string CreateSongsRoot(params string[] files)
    {
        string root = Path.Combine(TestContext.CurrentContext.WorkDirectory, Guid.NewGuid().ToString("N"));
        string set = Path.Combine(root, "1 Artist - Title");
        Directory.CreateDirectory(set);
        foreach (string file in files)
        {
            File.WriteAllBytes(Path.Combine(set, file), [1]);
        }

        return root;
    }

    private static void SpinUntil(Func<bool> condition)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(2);
        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            Thread.Sleep(10);
        }

        Assert.Fail("Condition was not met.");
    }
}
