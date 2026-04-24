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
        try
        {
            if (index < 0 || index >= _sets.Count)
            {
                return;
            }

            BeatmapMirrorSet set = _sets[index];
            TraceDownload("started", $"set={set.Id} withVideo={withVideo}");
            if (_preferNoVideoDownloads && withVideo && MirrorDefinition(set.Mirror).SupportsNoVideoDownloads && set.HasVideo)
            {
                withVideo = false;
                TraceDownload("prefer-no-video", $"set={set.Id}");
            }

            BeatmapDownloadResult downloadResult = await _downloadService.DownloadAsync(set, withVideo, CancellationToken.None).ConfigureAwait(false);
            if (downloadResult.IsSuccess)
            {
                EnqueueDownloadCompletion(new BeatmapDownloadCompletion(true, downloadResult.ArchivePath, null));
                TraceDownload("service-success", $"set={set.Id} archive={Path.GetFileName(downloadResult.ArchivePath)}");
            }
            else
            {
                EnqueueDownloadCompletion(new BeatmapDownloadCompletion(false, null, downloadResult.ErrorMessage));
                TraceDownload("service-failure", $"set={set.Id} error={downloadResult.ErrorMessage}");
            }
        }
        catch (Exception exception)
        {
            EnqueueDownloadCompletion(new BeatmapDownloadCompletion(false, null, exception.Message));
            TraceDownload("exception", $"index={index} error={exception.GetType().Name}: {exception.Message}");
        }
    }

    private void EnqueueDownloadCompletion(BeatmapDownloadCompletion completion)
    {
        _downloadCompletions.Enqueue(completion);
        TraceDownload("queued-completion", completion.IsSuccess ? Path.GetFileName(completion.ArchivePath) : completion.ErrorMessage);
    }

    private void ApplyQueuedDownloadCompletions()
    {
        while (_downloadCompletions.TryDequeue(out BeatmapDownloadCompletion? completion))
        {
            if (completion.IsSuccess)
            {
                _selectedSetIndex = null;
                _lastImportedSetDirectory = completion.ArchivePath is null ? null : Path.GetFileNameWithoutExtension(completion.ArchivePath);
                _message = _localizer["BeatmapDownloader_Downloaded"];
                TraceDownload("applied-success", Path.GetFileName(completion.ArchivePath));
            }
            else
            {
                _message = completion.ErrorMessage;
                TraceDownload("applied-failure", completion.ErrorMessage);
            }
        }
    }

    private void TraceDownload(string phase, string? detail = null)
    {
        string message = detail is null
            ? $"osu!droid downloader {phase}"
            : $"osu!droid downloader {phase} {detail}";
        Console.WriteLine(message);

        if (string.IsNullOrWhiteSpace(_downloadTracePath))
        {
            return;
        }

        try
        {
            string? directory = Path.GetDirectoryName(_downloadTracePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.AppendAllText(_downloadTracePath, $"{DateTimeOffset.UtcNow:O} {message}{Environment.NewLine}");
        }
        catch (Exception)
        {
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
