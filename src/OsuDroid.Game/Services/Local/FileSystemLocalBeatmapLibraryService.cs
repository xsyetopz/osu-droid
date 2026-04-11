using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OsuDroid.Game.Services.Local;

public sealed class FileSystemLocalBeatmapLibraryService(IPlatformStorage platformStorage) : ILocalBeatmapLibraryService
{
    private readonly List<BeatmapCard> _beatmaps = [];

    public IReadOnlyList<BeatmapCard> Beatmaps => _beatmaps;

    public Task<IReadOnlyList<BeatmapCard>> RefreshAsync(CancellationToken cancellationToken = default)
    {
        var discovered = new List<BeatmapCard>();

        foreach (var root in platformStorage.GetSongRoots().Where(Directory.Exists))
        {
            foreach (var path in Directory.EnumerateFiles(root, "*.osu", SearchOption.AllDirectories))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var card = tryParseBeatmap(path, root);
                if (card is not null)
                {
                    discovered.Add(card);
                }
            }
        }

        _beatmaps.Clear();
        _beatmaps.AddRange(discovered
            .OrderBy(card => card.Artist, StringComparer.OrdinalIgnoreCase)
            .ThenBy(card => card.Title, StringComparer.OrdinalIgnoreCase)
            .ThenBy(card => card.DifficultyName, StringComparer.OrdinalIgnoreCase));

        return Task.FromResult<IReadOnlyList<BeatmapCard>>(_beatmaps);
    }

    private static BeatmapCard? tryParseBeatmap(string path, string root)
    {
        string? title = null;
        string? artist = null;
        string? difficulty = null;
        string? mapper = null;
        string? beatmapId = null;

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("[Events]", StringComparison.Ordinal))
            {
                break;
            }

            if (line.StartsWith("Title:", StringComparison.Ordinal))
            {
                title = line["Title:".Length..].Trim();
                continue;
            }

            if (line.StartsWith("Artist:", StringComparison.Ordinal))
            {
                artist = line["Artist:".Length..].Trim();
                continue;
            }

            if (line.StartsWith("Version:", StringComparison.Ordinal))
            {
                difficulty = line["Version:".Length..].Trim();
                continue;
            }

            if (line.StartsWith("Creator:", StringComparison.Ordinal))
            {
                mapper = line["Creator:".Length..].Trim();
                continue;
            }

            if (line.StartsWith("BeatmapID:", StringComparison.Ordinal))
            {
                beatmapId = line["BeatmapID:".Length..].Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(artist))
        {
            return null;
        }

        var relativeRoot = Path.GetRelativePath(root, Path.GetDirectoryName(path) ?? root);
        var sourceLabel = relativeRoot == "." ? Path.GetFileName(root) : relativeRoot;

        return new BeatmapCard(
            string.IsNullOrWhiteSpace(beatmapId) ? path : beatmapId,
            artist,
            title,
            string.IsNullOrWhiteSpace(difficulty) ? "Normal" : difficulty,
            string.IsNullOrWhiteSpace(mapper) ? "Unknown" : mapper,
            sourceLabel,
            "Local");
    }
}
