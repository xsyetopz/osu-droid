using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Runtime.Paths;

namespace OsuDroid.Game.Beatmaps.Online;

public interface IBeatmapDownloadService
{
    BeatmapDownloadState State { get; }

    Task<BeatmapImportResult> DownloadAndImportAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken);

    void CancelActiveDownload();
}

public sealed record BeatmapDownloadState(
    long? ActiveSetId = null,
    string? Filename = null,
    BeatmapDownloadProgress? Progress = null,
    string? ErrorMessage = null,
    bool IsActive = false);

public sealed class BeatmapDownloadService(
    DroidGamePathLayout paths,
    IBeatmapMirrorClient mirrorClient,
    IBeatmapImportService importService) : IBeatmapDownloadService
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private BeatmapDownloadState state = new();
    private CancellationTokenSource? activeDownloadCancellation;

    public BeatmapDownloadState State => state;

    public void CancelActiveDownload() => activeDownloadCancellation?.Cancel();

    public async Task<BeatmapImportResult> DownloadAndImportAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken)
    {
        if (!await gate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            return BeatmapImportResult.Failed("Another beatmap is already downloading.");

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        activeDownloadCancellation = linkedCancellation;

        try
        {
            var archiveName = BeatmapImportService.SanitizeArchiveName($"{beatmapSet.Id} {beatmapSet.Artist} - {beatmapSet.Title}{(withVideo ? string.Empty : " [no video]")}");
            var destination = Path.Combine(paths.Downloads, archiveName + ".osz");
            state = new BeatmapDownloadState(beatmapSet.Id, archiveName, new BeatmapDownloadProgress(0, null, BeatmapDownloadPhase.Connecting), IsActive: true);
            var progress = new Progress<BeatmapDownloadProgress>(downloadProgress => state = state with { Progress = downloadProgress, IsActive = true });
            await mirrorClient.DownloadAsync(mirrorClient.CreateDownloadUri(beatmapSet.Mirror, beatmapSet.Id, withVideo), destination, progress, linkedCancellation.Token).ConfigureAwait(false);
            state = state with { Progress = new BeatmapDownloadProgress(0, null, BeatmapDownloadPhase.Importing), IsActive = true };
            var importResult = importService.ImportOsz(destination);
            state = importResult.IsSuccess ? new BeatmapDownloadState() : state with { ErrorMessage = importResult.ErrorMessage, IsActive = false };
            return importResult;
        }
        catch (OperationCanceledException)
        {
            state = new BeatmapDownloadState(ErrorMessage: "Download canceled.");
            return BeatmapImportResult.Failed("Download canceled.");
        }
        catch (Exception exception)
        {
            state = state with { ErrorMessage = exception.Message, IsActive = false };
            return BeatmapImportResult.Failed(exception.Message);
        }
        finally
        {
            activeDownloadCancellation = null;
            gate.Release();
        }
    }
}
