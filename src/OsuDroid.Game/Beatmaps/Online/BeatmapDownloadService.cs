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
    bool IsActive = false);

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
    private BeatmapDownloadState _state = new();
    private CancellationTokenSource? _activeDownloadCancellation;

    public BeatmapDownloadState State => _state;

    public void CancelActiveDownload() => _activeDownloadCancellation?.Cancel();

    public async Task<BeatmapDownloadResult> DownloadAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken)
    {
        if (!await _gate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
        {
            return BeatmapDownloadResult.Failed("Another beatmap is already downloading.");
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _activeDownloadCancellation = linkedCancellation;

        try
        {
            Directory.CreateDirectory(paths.Downloads);
            string archiveName = BeatmapImportService.SanitizeArchiveName($"{beatmapSet.Id} {beatmapSet.Artist} - {beatmapSet.Title}{(withVideo ? string.Empty : " [no video]")}");
            string destination = Path.Combine(paths.Downloads, archiveName + ".osz");
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }

            _state = new BeatmapDownloadState(beatmapSet.Id, archiveName, new BeatmapDownloadProgress(0, null, BeatmapDownloadPhase.Connecting), IsActive: true);
            var progress = new Progress<BeatmapDownloadProgress>(downloadProgress => _state = _state with { Progress = downloadProgress, IsActive = true });
            await mirrorClient.DownloadAsync(mirrorClient.CreateDownloadUri(beatmapSet.Mirror, beatmapSet.Id, withVideo), destination, progress, linkedCancellation.Token).ConfigureAwait(false);
            beatmapProcessingService?.EnqueueArchive(destination);
            _state = new BeatmapDownloadState();
            return BeatmapDownloadResult.Success(destination);
        }
        catch (OperationCanceledException)
        {
            _state = new BeatmapDownloadState(ErrorMessage: "Download canceled.");
            return BeatmapDownloadResult.Failed("Download canceled.");
        }
        catch (Exception exception)
        {
            _state = _state with { ErrorMessage = exception.Message, IsActive = false };
            return BeatmapDownloadResult.Failed(exception.Message);
        }
        finally
        {
            _activeDownloadCancellation = null;
            _gate.Release();
        }
    }
}
