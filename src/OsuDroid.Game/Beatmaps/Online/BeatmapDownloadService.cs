using System.IO.Compression;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Beatmaps.Online;

public interface IBeatmapDownloadService
{
    BeatmapDownloadState State { get; }

    Task<BeatmapDownloadResult> DownloadAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken);

    void CancelActiveDownload();
}

public sealed record BeatmapDownloadState(
    long? ActiveSetId = null,
    string? Filename = null,
    BeatmapDownloadProgress? Progress = null,
    string? ErrorMessage = null,
    bool IsActive = false,
    long SessionId = 0);

public sealed record BeatmapDownloadResult(bool IsSuccess, string? ArchivePath, string? ErrorMessage)
{
    public static BeatmapDownloadResult Success(string archivePath) => new(true, archivePath, null);

    public static BeatmapDownloadResult Failed(string errorMessage) => new(false, null, errorMessage);
}

public sealed class BeatmapDownloadService(
    DroidGamePathLayout paths,
    IBeatmapMirrorClient mirrorClient,
    IBeatmapProcessingService? beatmapProcessingService = null) : IBeatmapDownloadService
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly object _stateGate = new();
    private BeatmapDownloadState _state = new();
    private CancellationTokenSource? _activeDownloadCancellation;
    private long _nextSessionId;

    public BeatmapDownloadState State
    {
        get
        {
            lock (_stateGate)
            {
                return _state;
            }
        }
    }

    public void CancelActiveDownload()
    {
        CancellationTokenSource? activeDownloadCancellation;
        lock (_stateGate)
        {
            activeDownloadCancellation = _activeDownloadCancellation;
        }

        try
        {
            activeDownloadCancellation?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    public async Task<BeatmapDownloadResult> DownloadAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken)
    {
        if (!await _gate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            return BeatmapDownloadResult.Failed("Another beatmap is already downloading.");
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        string? downloadPath = null;
        SetActiveDownload(linkedCancellation);
        long sessionId = NextSessionId();

        try
        {
            Directory.CreateDirectory(paths.Downloads);
            string archiveName = BeatmapImportService.SanitizeArchiveName($"{beatmapSet.Id} {beatmapSet.Artist} - {beatmapSet.Title}{(withVideo ? string.Empty : " [no video]")}");
            string destination = Path.Combine(paths.Downloads, archiveName + ".osz");
            downloadPath = destination + ".download";
            TryDeleteFile(downloadPath);

            SetState(new BeatmapDownloadState(beatmapSet.Id, archiveName, new BeatmapDownloadProgress(0, null, BeatmapDownloadPhase.Connecting), IsActive: true, SessionId: sessionId));
            var progress = new InlineDownloadProgress(downloadProgress => UpdateActiveProgress(sessionId, downloadProgress));
            await mirrorClient.DownloadAsync(mirrorClient.CreateDownloadUri(beatmapSet.Mirror, beatmapSet.Id, withVideo), downloadPath, progress, linkedCancellation.Token).ConfigureAwait(false);
            if (!IsValidOsz(downloadPath))
            {
                TryDeleteFile(downloadPath);
                SetTerminalState(sessionId, new BeatmapDownloadState(ErrorMessage: "Downloaded beatmap archive is invalid.", SessionId: sessionId));
                return BeatmapDownloadResult.Failed("Downloaded beatmap archive is invalid.");
            }

            SetState(new BeatmapDownloadState(beatmapSet.Id, archiveName, new BeatmapDownloadProgress(0, null, BeatmapDownloadPhase.Importing), IsActive: true, SessionId: sessionId));
            File.Move(downloadPath, destination, overwrite: true);
            beatmapProcessingService?.EnqueueArchive(destination, CreateOnlineMetadata(beatmapSet));
            SetTerminalState(sessionId, new BeatmapDownloadState(SessionId: sessionId));
            return BeatmapDownloadResult.Success(destination);
        }
        catch (OperationCanceledException)
        {
            TryDeleteFile(downloadPath);
            SetTerminalState(sessionId, new BeatmapDownloadState(ErrorMessage: "Download canceled.", SessionId: sessionId));
            return BeatmapDownloadResult.Failed("Download canceled.");
        }
        catch (Exception exception)
        {
            TryDeleteFile(downloadPath);
            SetTerminalState(sessionId, new BeatmapDownloadState(ErrorMessage: exception.Message, SessionId: sessionId));
            return BeatmapDownloadResult.Failed(exception.Message);
        }
        finally
        {
            SetActiveDownload(null);
            _gate.Release();
        }
    }

    private void SetState(BeatmapDownloadState state)
    {
        lock (_stateGate)
        {
            _state = state;
        }
    }

    private void UpdateActiveProgress(long sessionId, BeatmapDownloadProgress progress)
    {
        lock (_stateGate)
        {
            if (_state.SessionId == sessionId && _state.IsActive)
            {
                _state = _state with { Progress = progress };
            }
        }
    }

    private void SetTerminalState(long sessionId, BeatmapDownloadState state)
    {
        lock (_stateGate)
        {
            if (_state.SessionId == sessionId)
            {
                _state = state;
            }
        }
    }

    private void SetActiveDownload(CancellationTokenSource? activeDownloadCancellation)
    {
        lock (_stateGate)
        {
            _activeDownloadCancellation = activeDownloadCancellation;
        }
    }

    private long NextSessionId()
    {
        lock (_stateGate)
        {
            return ++_nextSessionId;
        }
    }

    private static BeatmapOnlineMetadata CreateOnlineMetadata(BeatmapMirrorSet beatmapSet) => new(
        beatmapSet.Id,
        beatmapSet.Status,
        beatmapSet.Beatmaps
            .Where(beatmap => beatmap.Mode == 0 && beatmap.StarRating > 0)
            .Select(beatmap => new BeatmapOnlineDifficultyMetadata(beatmap.Id, beatmap.Version, beatmap.StarRating))
            .ToArray());

    private static bool IsValidOsz(string archivePath)
    {
        try
        {
            using ZipArchive archive = ZipFile.OpenRead(archivePath);
            return archive.Entries.Any(entry => entry.FullName.EndsWith(".osu", StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static void TryDeleteFile(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception)
        {
        }
    }

    private sealed class InlineDownloadProgress(Action<BeatmapDownloadProgress> report) : IProgress<BeatmapDownloadProgress>
    {
        public void Report(BeatmapDownloadProgress progress) => report(progress);
    }
}
