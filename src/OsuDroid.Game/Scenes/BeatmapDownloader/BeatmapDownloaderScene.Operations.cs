using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class BeatmapDownloaderScene
{
    private async Task SearchAsync(bool append)
    {
        if (isSearching)
            return;

        if (!append)
        {
            searchCancellation.Cancel();
            searchCancellation.Dispose();
            searchCancellation = new CancellationTokenSource();
            offset = 0;
            scrollOffset = 0f;
            sets = [];
            hasMore = true;
        }

        if (!hasMore)
            return;

        try
        {
            isSearching = true;
            hasSearchError = false;
            message = null;
            var request = new BeatmapMirrorSearchRequest(query, offset, PageSize, sort, order, status, mirror);
            var result = await mirrorClient.SearchAsync(request, searchCancellation.Token).ConfigureAwait(false);
            sets = append ? sets.Concat(result).ToArray() : result;
            offset += result.Count;
            hasMore = result.Count >= PageSize;
            message = sets.Count == 0 ? "No beatmaps found" : null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            message = "Failed to connect to server, please check your internet connection.";
            hasSearchError = true;
            hasMore = false;
        }
        finally
        {
            isSearching = false;
        }
    }

    private async Task DownloadAsync(int index, bool withVideo)
    {
        if (index < 0 || index >= sets.Count)
            return;

        var set = sets[index];
        if (preferNoVideoDownloads && withVideo && MirrorDefinition(set.Mirror).SupportsNoVideoDownloads && set.HasVideo)
            withVideo = false;

        var downloadResult = await downloadService.DownloadAsync(set, withVideo, CancellationToken.None).ConfigureAwait(false);
        if (downloadResult.IsSuccess)
        {
            selectedSetIndex = null;
            lastImportedSetDirectory = downloadResult.ArchivePath is null ? null : Path.GetFileNameWithoutExtension(downloadResult.ArchivePath);
            message = "Beatmap downloaded";
        }
        else
        {
            message = downloadResult.ErrorMessage;
        }
    }

    private void Preview(int index)
    {
        if (index < 0 || index >= sets.Count || sets[index].Beatmaps.Count == 0)
            return;

        if (previewingSetIndex == index)
        {
            if (ownsPreviewPlayback)
                previewPlayer.StopPreview();
            previewingSetIndex = null;
            ownsPreviewPlayback = false;
            return;
        }

        if (previewPlayCount >= 2 && DateTime.UtcNow - lastPreviewStartedUtc < TimeSpan.FromSeconds(5))
            return;

        if (DateTime.UtcNow - lastPreviewStartedUtc >= TimeSpan.FromSeconds(5))
            previewPlayCount = 0;

        if (ownsPreviewPlayback)
            previewPlayer.StopPreview();
        previewPlayer.Play(mirrorClient.CreatePreviewUri(sets[index].Mirror, sets[index].Beatmaps[0].Id));
        previewingSetIndex = index;
        ownsPreviewPlayback = true;
        previewPlayCount++;
        lastPreviewStartedUtc = DateTime.UtcNow;
    }
}
