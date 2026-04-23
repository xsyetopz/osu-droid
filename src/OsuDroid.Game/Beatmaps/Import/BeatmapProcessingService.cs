using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Beatmaps.Import;

public sealed record BeatmapProcessingState(
    bool IsActive = false,
    int Percent = 0,
    string StatusText = "Processing beatmaps...");

public interface IBeatmapProcessingService
{
    BeatmapProcessingState State { get; }

    void EnqueueArchive(string archivePath);

    bool HasPendingWork();

    void Start();

    bool TryConsumeCompletedSnapshot(out BeatmapLibrarySnapshot snapshot);
}

public sealed class BeatmapProcessingService(
    DroidGamePathLayout paths,
    IBeatmapImportService importService,
    IBeatmapLibrary library,
    IGameSettingsStore? settingsStore = null) : IBeatmapProcessingService
{
    private readonly object gate = new();
    private readonly HashSet<string> queuedArchives = new(StringComparer.OrdinalIgnoreCase);
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

    public void EnqueueArchive(string archivePath)
    {
        if (string.IsNullOrWhiteSpace(archivePath))
            return;

        lock (gate)
            queuedArchives.Add(Path.GetFullPath(archivePath));
    }

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
                _ = importService.ImportOsz(archive, DeleteImportedArchives());
                ClearQueuedArchive(archive);
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
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string[] queued;
        lock (gate)
            queued = queuedArchives.ToArray();

        foreach (var archive in queued.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(archive) && seen.Add(archive))
                yield return archive;
        }

        foreach (var archive in EnumeratePendingArchives(paths.Songs))
        {
            if (seen.Add(archive))
                yield return archive;
        }

        if (!ShouldScanDownloads())
            yield break;

        foreach (var archive in EnumeratePendingArchives(paths.Downloads))
        {
            if (seen.Add(archive))
                yield return archive;
        }
    }

    private static IEnumerable<string> EnumeratePendingArchives(string directory)
    {
        if (!Directory.Exists(directory))
            yield break;

        foreach (var archive in Directory.EnumerateFiles(directory, "*.osz", SearchOption.TopDirectoryOnly)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            yield return archive;
        }
    }

    private bool DeleteImportedArchives() => settingsStore?.GetBool("deleteosz", true) ?? true;

    private bool ShouldScanDownloads() => settingsStore?.GetBool("scandownload", false) ?? false;

    private void SetState(BeatmapProcessingState next)
    {
        lock (gate)
            state = next;
    }

    private void ClearQueuedArchive(string archivePath)
    {
        lock (gate)
            queuedArchives.Remove(archivePath);
    }

    private static int CalculatePercent(int completedSteps, int totalSteps)
    {
        if (totalSteps <= 0)
            return 100;

        return Math.Clamp((int)MathF.Round(completedSteps * 100f / totalSteps), 0, 100);
    }
}
