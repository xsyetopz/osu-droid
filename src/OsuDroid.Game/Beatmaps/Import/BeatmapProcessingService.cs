using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Beatmaps.Import;

public sealed record BeatmapProcessingState(
    bool IsActive = false,
    int Percent = 0,
    string StatusText = "Processing beatmaps...");

public interface IBeatmapProcessingService
{
    BeatmapProcessingState State { get; }

    bool HasPendingWork();

    void Start();

    bool TryConsumeCompletedSnapshot(out BeatmapLibrarySnapshot snapshot);
}

public sealed class BeatmapProcessingService(
    DroidGamePathLayout paths,
    IBeatmapImportService importService,
    IBeatmapLibrary library) : IBeatmapProcessingService
{
    private readonly object gate = new();
    private Task? task;
    private BeatmapProcessingState state = new();
    private BeatmapLibrarySnapshot? completedSnapshot;

    public BeatmapProcessingState State
    {
        get
        {
            lock (gate)
                return state;
        }
    }

    public bool HasPendingWork() => EnumeratePendingArchives().Any() || library.NeedsScanRefresh();

    public void Start()
    {
        lock (gate)
        {
            if (task is { IsCompleted: false } || !HasPendingWork())
                return;

            state = new BeatmapProcessingState(true, 0, "Processing beatmaps...");
            completedSnapshot = null;
            task = Task.Run(ProcessPendingWork);
        }
    }

    public bool TryConsumeCompletedSnapshot(out BeatmapLibrarySnapshot snapshot)
    {
        lock (gate)
        {
            if (completedSnapshot is null)
            {
                snapshot = BeatmapLibrarySnapshot.Empty;
                return false;
            }

            snapshot = completedSnapshot;
            completedSnapshot = null;
            return true;
        }
    }

    private void ProcessPendingWork()
    {
        try
        {
            var archives = EnumeratePendingArchives().ToArray();
            var needsScan = library.NeedsScanRefresh();
            var totalSteps = archives.Length + (needsScan ? 1 : 0);
            var completedSteps = 0;

            foreach (var archive in archives)
            {
                SetState(new BeatmapProcessingState(true, CalculatePercent(completedSteps, totalSteps), "Importing beatmaps..."));
                _ = importService.ImportOsz(archive);
                completedSteps++;
                SetState(new BeatmapProcessingState(true, CalculatePercent(completedSteps, totalSteps), "Importing beatmaps..."));
            }

            BeatmapLibrarySnapshot snapshot;
            if (needsScan)
            {
                SetState(new BeatmapProcessingState(true, CalculatePercent(completedSteps, totalSteps), "Scanning beatmaps..."));
                snapshot = library.Scan();
                completedSteps++;
            }
            else
            {
                snapshot = library.Load();
            }

            lock (gate)
            {
                completedSnapshot = snapshot;
                state = new BeatmapProcessingState(false, 100, "Processing beatmaps...");
            }
        }
        catch
        {
            lock (gate)
                state = new BeatmapProcessingState(false, 100, "Processing beatmaps...");
        }
    }

    private IEnumerable<string> EnumeratePendingArchives()
    {
        if (!Directory.Exists(paths.Songs))
            yield break;

        foreach (var archive in Directory.EnumerateFiles(paths.Songs, "*.osz", SearchOption.TopDirectoryOnly)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            yield return archive;
        }
    }

    private void SetState(BeatmapProcessingState next)
    {
        lock (gate)
            state = next;
    }

    private static int CalculatePercent(int completedSteps, int totalSteps)
    {
        if (totalSteps <= 0)
            return 100;

        return Math.Clamp((int)MathF.Round(completedSteps * 100f / totalSteps), 0, 100);
    }
}
