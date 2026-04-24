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

    void EnqueueArchive(string archivePath, BeatmapOnlineMetadata? metadata = null);

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
    private readonly object _gate = new();
    private readonly Dictionary<string, BeatmapOnlineMetadata?> _queuedArchives = new(StringComparer.OrdinalIgnoreCase);
    private Task? _task;
    private BeatmapProcessingState _state = new();
    private BeatmapLibrarySnapshot? _completedSnapshot;

    public BeatmapProcessingState State
    {
        get
        {
            lock (_gate)
            {
                return _state;
            }
        }
    }

    public bool HasPendingWork() => EnumeratePendingArchives().Any() || library.NeedsScanRefresh();

    public void EnqueueArchive(string archivePath, BeatmapOnlineMetadata? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(archivePath))
        {
            return;
        }

        lock (_gate)
        {
            _queuedArchives[Path.GetFullPath(archivePath)] = metadata;
        }
    }

    public void Start()
    {
        lock (_gate)
        {
            if (_task is { IsCompleted: false } || !HasPendingWork())
            {
                return;
            }

            _state = new BeatmapProcessingState(true, 0, "Processing beatmaps...");
            _completedSnapshot = null;
            _task = Task.Run(ProcessPendingWork);
        }
    }

    public bool TryConsumeCompletedSnapshot(out BeatmapLibrarySnapshot snapshot)
    {
        lock (_gate)
        {
            if (_completedSnapshot is null)
            {
                snapshot = BeatmapLibrarySnapshot.Empty;
                return false;
            }

            snapshot = _completedSnapshot;
            _completedSnapshot = null;
            return true;
        }
    }

    private void ProcessPendingWork()
    {
        try
        {
            KeyValuePair<string, BeatmapOnlineMetadata?>[] archives = EnumeratePendingArchives().ToArray();
            bool needsScan = library.NeedsScanRefresh();
            int totalSteps = archives.Length + (needsScan ? 1 : 0);
            int completedSteps = 0;

            foreach ((string? archive, BeatmapOnlineMetadata? metadata) in archives)
            {
                SetState(new BeatmapProcessingState(true, CalculatePercent(completedSteps, totalSteps), "Importing beatmaps..."));
                _ = importService.ImportOsz(archive, DeleteImportedArchives(), metadata);
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

            lock (_gate)
            {
                _completedSnapshot = snapshot;
                _state = new BeatmapProcessingState(false, 100, "Processing beatmaps...");
            }
        }
        catch
        {
            lock (_gate)
            {
                _state = new BeatmapProcessingState(false, 100, "Processing beatmaps...");
            }
        }
    }

    private IEnumerable<KeyValuePair<string, BeatmapOnlineMetadata?>> EnumeratePendingArchives()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        KeyValuePair<string, BeatmapOnlineMetadata?>[] queued;
        lock (_gate)
        {
            queued = _queuedArchives.ToArray();
        }

        foreach ((string? archive, BeatmapOnlineMetadata? metadata) in queued.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(archive) && seen.Add(archive))
            {
                yield return new KeyValuePair<string, BeatmapOnlineMetadata?>(archive, metadata);
            }
        }

        foreach (string archive in EnumeratePendingArchives(paths.Songs))
        {
            if (seen.Add(archive))
            {
                yield return new KeyValuePair<string, BeatmapOnlineMetadata?>(archive, null);
            }
        }

        if (!ShouldScanDownloads())
        {
            yield break;
        }

        foreach (string archive in EnumeratePendingArchives(paths.Downloads))
        {
            if (seen.Add(archive))
            {
                yield return new KeyValuePair<string, BeatmapOnlineMetadata?>(archive, null);
            }
        }
    }

    private static IEnumerable<string> EnumeratePendingArchives(string directory)
    {
        if (!Directory.Exists(directory))
        {
            yield break;
        }

        foreach (string? archive in Directory.EnumerateFiles(directory, "*.osz", SearchOption.TopDirectoryOnly)
                     .OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            yield return archive;
        }
    }

    private bool DeleteImportedArchives() => settingsStore?.GetBool("deleteosz", true) ?? true;

    private bool ShouldScanDownloads() => settingsStore?.GetBool("scandownload", false) ?? false;

    private void SetState(BeatmapProcessingState next)
    {
        lock (_gate)
        {
            _state = next;
        }
    }

    private void ClearQueuedArchive(string archivePath)
    {
        lock (_gate)
        {
            _queuedArchives.Remove(archivePath);
        }
    }

    private static int CalculatePercent(int completedSteps, int totalSteps) => totalSteps <= 0 ? 100 : Math.Clamp((int)MathF.Round(completedSteps * 100f / totalSteps), 0, 100);
}
