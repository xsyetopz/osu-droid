using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private void AddCover(
        List<UiElementSnapshot> elements,
        string id,
        BeatmapMirrorSet set,
        UiRect bounds,
        UiAction action
    )
    {
        elements.Add(Fill(id + "-fallback", bounds, s_coverFallback, 0.55f, action, Radius));
        string? path = GetCoverPath(set);
        if (path is not null && File.Exists(path))
        {
            elements.Add(
                new UiElementSnapshot(
                    id,
                    UiElementKind.Sprite,
                    bounds,
                    s_white,
                    0.55f,
                    Action: action,
                    ExternalAssetPath: path,
                    SpriteFit: UiSpriteFit.Cover
                )
            );
            return;
        }

        StartCoverDownload(set);
    }

    private void StartCoverDownload(BeatmapMirrorSet set)
    {
        if (string.IsNullOrWhiteSpace(set.CoverUrl))
        {
            return;
        }

        string? path = GetCoverPath(set);
        if (path is null || File.Exists(path) || !_coverDownloads.Add(path))
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                using HttpResponseMessage response = await s_coverClient
                    .GetAsync(set.CoverUrl)
                    .ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                using Stream source = await response
                    .Content.ReadAsStreamAsync()
                    .ConfigureAwait(false);
                using FileStream destination = File.Create(path);
                await source.CopyToAsync(destination).ConfigureAwait(false);
            }
            catch (Exception)
            {
                TryDelete(path);
            }
            finally
            {
                _coverDownloads.Remove(path);
            }
        });
    }

    private string? GetCoverPath(BeatmapMirrorSet set)
    {
        if (string.IsNullOrWhiteSpace(set.CoverUrl))
        {
            return null;
        }

        string extension = Path.GetExtension(new Uri(set.CoverUrl).AbsolutePath);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        return Path.Combine(
            _coverCacheDirectory,
            BeatmapImportService.SanitizeArchiveName($"{set.Mirror}-{set.Id}") + extension
        );
    }
}
