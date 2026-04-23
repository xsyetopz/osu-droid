using OsuDroid.Game.Beatmaps.Online;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private async Task SearchAsync(bool append)
    {
        if (_isSearching)
        {
            return;
        }

        if (!append)
        {
            _searchCancellation.Cancel();
            _searchCancellation.Dispose();
            _searchCancellation = new CancellationTokenSource();
            _offset = 0;
            _scrollOffset = 0f;
            _sets = [];
            _hasMore = true;
        }

        if (!_hasMore)
        {
            return;
        }

        try
        {
            _isSearching = true;
            _hasSearchError = false;
            _message = null;
            var request = new BeatmapMirrorSearchRequest(_query, _offset, PageSize, _sort, _order, _status, _mirror);
            IReadOnlyList<BeatmapMirrorSet> result = await _mirrorClient.SearchAsync(request, _searchCancellation.Token).ConfigureAwait(false);
            _sets = append ? _sets.Concat(result).ToArray() : result;
            _offset += result.Count;
            _hasMore = result.Count >= PageSize;
            _message = _sets.Count == 0 ? _localizer["BeatmapDownloader_NoBeatmapsFound"] : null;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _message = _localizer["BeatmapDownloader_ConnectionFailed"];
            _hasSearchError = true;
            _hasMore = false;
        }
        finally
        {
            _isSearching = false;
        }
    }

    private async Task DownloadAsync(int index, bool withVideo)
    {
        if (index < 0 || index >= _sets.Count)
        {
            return;
        }

        BeatmapMirrorSet set = _sets[index];
        if (_preferNoVideoDownloads && withVideo && MirrorDefinition(set.Mirror).SupportsNoVideoDownloads && set.HasVideo)
        {
            withVideo = false;
        }

        BeatmapDownloadResult downloadResult = await _downloadService.DownloadAsync(set, withVideo, CancellationToken.None).ConfigureAwait(false);
        if (downloadResult.IsSuccess)
        {
            _selectedSetIndex = null;
            _lastImportedSetDirectory = downloadResult.ArchivePath is null ? null : Path.GetFileNameWithoutExtension(downloadResult.ArchivePath);
            _message = _localizer["BeatmapDownloader_Downloaded"];
        }
        else
        {
            _message = downloadResult.ErrorMessage;
        }
    }

    private void Preview(int index)
    {
        if (index < 0 || index >= _sets.Count || _sets[index].Beatmaps.Count == 0)
        {
            return;
        }

        if (_previewingSetIndex == index)
        {
            if (_ownsPreviewPlayback)
            {
                _previewPlayer.StopPreview();
            }

            _previewingSetIndex = null;
            _ownsPreviewPlayback = false;
            return;
        }

        if (_previewPlayCount >= 2 && DateTime.UtcNow - _lastPreviewStartedUtc < TimeSpan.FromSeconds(5))
        {
            return;
        }

        if (DateTime.UtcNow - _lastPreviewStartedUtc >= TimeSpan.FromSeconds(5))
        {
            _previewPlayCount = 0;
        }

        if (_ownsPreviewPlayback)
        {
            _previewPlayer.StopPreview();
        }

        _previewPlayer.Play(_mirrorClient.CreatePreviewUri(_sets[index].Mirror, _sets[index].Beatmaps[0].Id));
        _previewingSetIndex = index;
        _ownsPreviewPlayback = true;
        _previewPlayCount++;
        _lastPreviewStartedUtc = DateTime.UtcNow;
    }
}
