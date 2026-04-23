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
    private readonly SemaphoreSlim gate = new(1, 1);
    private BeatmapDownloadState state = new();
    private CancellationTokenSource? activeDownloadCancellation;

    public BeatmapDownloadState State => state;

    public void CancelActiveDownload() => activeDownloadCancellation?.Cancel();

    public async Task<BeatmapDownloadResult> DownloadAsync(BeatmapMirrorSet beatmapSet, bool withVideo, CancellationToken cancellationToken)
    {
        if (!await gate.WaitAsync(0, cancellationToken).ConfigureAwait(false))
            return BeatmapDownloadResult.Failed("Another beatmap is already downloading.");

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        activeDownloadCancellation = linkedCancellation;

        try
        {
            Directory.CreateDirectory(paths.Downloads);
            var archiveName = BeatmapImportService.SanitizeArchiveName($"{beatmapSet.Id} {beatmapSet.Artist} - {beatmapSet.Title}{(withVideo ? string.Empty : " [no video]")}");
            var destination = Path.Combine(paths.Downloads, archiveName + ".osz");
            if (File.Exists(destination))
                File.Delete(destination);

            state = new BeatmapDownloadState(beatmapSet.Id, archiveName, new BeatmapDownloadProgress(0, null, BeatmapDownloadPhase.Connecting), IsActive: true);
            var progress = new Progress<BeatmapDownloadProgress>(downloadProgress => state = state with { Progress = downloadProgress, IsActive = true });
            await mirrorClient.DownloadAsync(mirrorClient.CreateDownloadUri(beatmapSet.Mirror, beatmapSet.Id, withVideo), destination, progress, linkedCancellation.Token).ConfigureAwait(false);
            beatmapProcessingService?.EnqueueArchive(destination);
            state = new BeatmapDownloadState();
            return BeatmapDownloadResult.Success(destination);
        }
        catch (OperationCanceledException)
        {
            state = new BeatmapDownloadState(ErrorMessage: "Download canceled.");
            return BeatmapDownloadResult.Failed("Download canceled.");
        }
        catch (Exception exception)
        {
            state = state with { ErrorMessage = exception.Message, IsActive = false };
            return BeatmapDownloadResult.Failed(exception.Message);
        }
        finally
        {
            activeDownloadCancellation = null;
            gate.Release();
        }
    }
}
