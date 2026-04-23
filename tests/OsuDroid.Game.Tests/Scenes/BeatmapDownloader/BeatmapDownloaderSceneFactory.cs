using System.Reflection;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{
    private static BeatmapDownloaderScene CreateScene(ITextInputService? textInput = null, IBeatmapDownloadService? downloadService = null) => new(
        new OsuDirectMirrorClient(new HttpClient(new EmptyHandler())),
        downloadService ?? new NoOpDownloadService(),
        textInput ?? new NoOpTextInputService(),
        new NoOpBeatmapPreviewPlayer(),
        Path.Combine(Path.GetTempPath(), "osudroid-tests", Guid.NewGuid().ToString("N")));

    private static void SetSets(BeatmapDownloaderScene scene, IReadOnlyList<BeatmapMirrorSet> sets) => typeof(BeatmapDownloaderScene).GetField("_sets", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(scene, sets);

    private static int ElementIndex(UiFrameSnapshot frame, string id)
    {
        for (int index = 0; index < frame.Elements.Count; index++)
        {
            if (frame.Elements[index].Id == id)
            {
                return index;
            }
        }

        Assert.Fail($"Missing element {id}");
        return -1;
    }

    private static BeatmapMirrorSet CreateSet() => new(
        BeatmapMirrorKind.OsuDirect,
        100,
        "Title",
        "Title",
        "Artist",
        "Artist",
        BeatmapRankedStatus.Qualified,
        "Mapper",
        null,
        false,
        [
            new BeatmapMirrorBeatmap(1, "Normal", 2.4f, 5, 4, 5, 5, 120, 90, 10, 20, 0, 0),
            new BeatmapMirrorBeatmap(2, "Hard", 5.1f, 8, 4, 6, 8, 180, 120, 50, 40, 1, 0),
        ]);

    private static BeatmapMirrorSet CreateSetWithDifficulties(int difficultyCount) => new(
        BeatmapMirrorKind.OsuDirect,
        100,
        "Title",
        "Title",
        "Artist",
        "Artist",
        BeatmapRankedStatus.Qualified,
        "Mapper",
        null,
        false,
        Enumerable.Range(0, difficultyCount)
            .Select(index => new BeatmapMirrorBeatmap(index + 1, $"Diff {index + 1}", 0.5f + index * 0.5f, 5, 4, 5, 5, 120, 90, 10, 20, 0, 0))
            .ToArray());
}
