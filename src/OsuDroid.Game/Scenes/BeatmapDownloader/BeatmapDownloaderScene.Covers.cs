using System.Globalization;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

public sealed partial class BeatmapDownloaderScene
{
    private void AddCover(List<UiElementSnapshot> elements, string id, BeatmapMirrorSet set, UiRect bounds, UiAction action)
    {
        elements.Add(Fill(id + "-fallback", bounds, CoverFallback, 0.55f, action, Radius));
        var path = GetCoverPath(set);
        if (path is not null && File.Exists(path))
        {
            elements.Add(new UiElementSnapshot(id, UiElementKind.Sprite, bounds, White, 0.55f, Action: action, ExternalAssetPath: path, SpriteFit: UiSpriteFit.Cover));
            return;
        }

        StartCoverDownload(set);
    }

    private void StartCoverDownload(BeatmapMirrorSet set)
    {
        if (string.IsNullOrWhiteSpace(set.CoverUrl))
            return;

        var path = GetCoverPath(set);
        if (path is null || File.Exists(path) || !coverDownloads.Add(path))
            return;

        _ = Task.Run(async () =>
        {
            try
            {
                using var response = await CoverClient.GetAsync(set.CoverUrl).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                using var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                using var destination = File.Create(path);
                await source.CopyToAsync(destination).ConfigureAwait(false);
            }
            catch (Exception)
            {
                TryDelete(path);
            }
            finally
            {
                coverDownloads.Remove(path);
            }
        });
    }

    private string? GetCoverPath(BeatmapMirrorSet set)
    {
        if (string.IsNullOrWhiteSpace(set.CoverUrl))
            return null;

        var extension = Path.GetExtension(new Uri(set.CoverUrl).AbsolutePath);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".jpg";
        return Path.Combine(coverCacheDirectory, BeatmapImportService.SanitizeArchiveName($"{set.Mirror}-{set.Id}") + extension);
    }
}
