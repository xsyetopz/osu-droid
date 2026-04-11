using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OsuDroid.Game.Services.Online;

public sealed class OsuDirectBeatmapBrowseService(HttpClient httpClient) : IOnlineBeatmapBrowseService
{
    private static readonly Uri SearchEndpoint = new("https://osu.direct/api/v2/search");

    public async Task<IReadOnlyList<BeatmapCard>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var builder = new UriBuilder(SearchEndpoint)
        {
            Query = $"query={Uri.EscapeDataString(query ?? string.Empty)}"
        };

        using var response = await httpClient.GetAsync(builder.Uri, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return parseResults(document.RootElement);
    }

    private static List<BeatmapCard> parseResults(JsonElement root)
    {
        var items = resolveResultArray(root);
        List<BeatmapCard> cards = [];

        foreach (var item in items)
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            var id = readString(item, "id") ?? readString(item, "setId") ?? Guid.NewGuid().ToString("N");
            var artist = readString(item, "artist") ?? readString(item, "artistUnicode") ?? "Unknown artist";
            var title = readString(item, "title") ?? readString(item, "titleUnicode") ?? "Unknown title";
            var mapper = readString(item, "creator") ?? readString(item, "mapper") ?? "Unknown mapper";
            var status = readString(item, "rankedStatus") ?? readString(item, "status") ?? "Online";
            var difficulty = readDifficultyName(item);

            cards.Add(new BeatmapCard(id, artist, title, difficulty, mapper, "osu.direct", status));
        }

        return cards;
    }

    private static JsonElement.ArrayEnumerator resolveResultArray(JsonElement root)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            return root.EnumerateArray();
        }

        foreach (var propertyName in new[] { "results", "data", "beatmaps", "beatmapsets" })
        {
            if (root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
            {
                return property.EnumerateArray();
            }
        }

        return default;
    }

    private static string readDifficultyName(JsonElement item)
    {
        foreach (var propertyName in new[] { "beatmaps", "childrenBeatmaps", "maps" })
        {
            if (!item.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var child in property.EnumerateArray())
            {
                var version = readString(child, "version") ?? readString(child, "difficulty") ?? readString(child, "name");
                if (!string.IsNullOrWhiteSpace(version))
                {
                    return version;
                }
            }
        }

        return "Online";
    }

    private static string? readString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.ToString(),
            _ => null
        };
    }
}
