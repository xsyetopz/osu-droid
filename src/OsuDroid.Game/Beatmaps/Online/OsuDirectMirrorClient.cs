using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Beatmaps.Online;

public interface IBeatmapMirrorClient
{
    IReadOnlyList<BeatmapMirrorDefinition> Mirrors { get; }

    Uri CreateSearchUri(BeatmapMirrorSearchRequest request);

    Uri CreateDownloadUri(long beatmapSetId, bool withVideo);

    Uri CreateDownloadUri(BeatmapMirrorKind mirror, long beatmapSetId, bool withVideo);

    Uri CreatePreviewUri(BeatmapMirrorKind mirror, long beatmapId);

    Task<IReadOnlyList<BeatmapMirrorSet>> SearchAsync(BeatmapMirrorSearchRequest request, CancellationToken cancellationToken);

    Task DownloadAsync(Uri source, string destinationPath, IProgress<BeatmapDownloadProgress>? progress, CancellationToken cancellationToken);
}

public sealed class OsuDirectMirrorClient(HttpClient httpClient) : IBeatmapMirrorClient
{
    private static readonly Uri OsuDirectBaseUri = new("https://osu.direct/");
    private static readonly Uri CatboyBaseUri = new("https://catboy.best/");

    public IReadOnlyList<BeatmapMirrorDefinition> Mirrors { get; } =
    [
        new(BeatmapMirrorKind.OsuDirect, "https://osu.direct", "osu.direct", DroidAssets.BeatmapDownloaderOsuDirect, true),
        new(BeatmapMirrorKind.Catboy, "https://catboy.best", "Mino", DroidAssets.BeatmapDownloaderCatboy, false),
    ];

    public Uri CreateSearchUri(BeatmapMirrorSearchRequest request)
    {
        var endpoint = request.Mirror == BeatmapMirrorKind.Catboy
            ? new Uri(CatboyBaseUri, "api/v2/search")
            : new Uri(OsuDirectBaseUri, "api/v2/search");

        var parameters = new Dictionary<string, string?>
        {
            ["sort"] = $"{ToApiSort(request.Sort)}:{ToApiOrder(request.Order)}",
            ["mode"] = "0",
            ["query"] = request.Query,
            ["offset"] = request.Offset.ToString(CultureInfo.InvariantCulture),
        };

        if (request.Mirror == BeatmapMirrorKind.Catboy)
            parameters["limit"] = request.Limit.ToString(CultureInfo.InvariantCulture);
        else
            parameters["amount"] = request.Limit.ToString(CultureInfo.InvariantCulture);

        if (request.Status is not null)
            parameters["status"] = ((int)request.Status.Value).ToString(CultureInfo.InvariantCulture);

        var builder = new UriBuilder(endpoint);
        builder.Query = string.Join('&', parameters.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value ?? string.Empty)}"));
        return builder.Uri;
    }

    public Uri CreateDownloadUri(long beatmapSetId, bool withVideo) => CreateDownloadUri(BeatmapMirrorKind.OsuDirect, beatmapSetId, withVideo);

    public Uri CreateDownloadUri(BeatmapMirrorKind mirror, long beatmapSetId, bool withVideo)
    {
        if (mirror == BeatmapMirrorKind.Catboy)
            return new Uri(CatboyBaseUri, $"d/{beatmapSetId}");

        var uri = new Uri(OsuDirectBaseUri, $"api/d/{beatmapSetId}");
        return withVideo ? uri : new Uri(uri, "?noVideo=1");
    }

    public Uri CreatePreviewUri(BeatmapMirrorKind mirror, long beatmapId) => mirror == BeatmapMirrorKind.Catboy
        ? new Uri(CatboyBaseUri, $"preview/audio/{beatmapId}")
        : new Uri(OsuDirectBaseUri, $"api/media/preview/{beatmapId}");

    public async Task<IReadOnlyList<BeatmapMirrorSet>> SearchAsync(BeatmapMirrorSearchRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, CreateSearchUri(request));
        message.Headers.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "Android"));
        using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return ParseSearchResponse(request.Mirror, document.RootElement);
    }

    public async Task DownloadAsync(Uri source, string destinationPath, IProgress<BeatmapDownloadProgress>? progress, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? ".");
        using var message = new HttpRequestMessage(HttpMethod.Get, source);
        message.Headers.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "Android"));
        using var response = await httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var totalBytes = response.Content.Headers.ContentLength;
        using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var fileStream = File.Create(destinationPath);
        var buffer = new byte[81920];
        var stopwatch = Stopwatch.StartNew();
        long bytesReceived = 0;
        int read;

        while ((read = await sourceStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            bytesReceived += read;
            var speed = stopwatch.Elapsed.TotalSeconds > 0 ? bytesReceived / stopwatch.Elapsed.TotalSeconds : 0;
            progress?.Report(new BeatmapDownloadProgress(bytesReceived, totalBytes, "Downloading", speed));
        }
    }

    private static List<BeatmapMirrorSet> ParseSearchResponse(BeatmapMirrorKind mirror, JsonElement root)
    {
        var sets = new List<BeatmapMirrorSet>();
        var array = root.ValueKind == JsonValueKind.Array
            ? root
            : root.TryGetProperty("beatmapsets", out var beatmapsets) ? beatmapsets : root;

        if (array.ValueKind != JsonValueKind.Array)
            return sets;

        foreach (var setElement in array.EnumerateArray())
        {
            var id = GetInt64(setElement, "id");
            var beatmaps = new List<BeatmapMirrorBeatmap>();
            if (setElement.TryGetProperty("beatmaps", out var beatmapsElement) && beatmapsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var beatmapElement in beatmapsElement.EnumerateArray())
                {
                    beatmaps.Add(new BeatmapMirrorBeatmap(
                        Id: GetInt64(beatmapElement, "id"),
                        Version: GetString(beatmapElement, "version"),
                        StarRating: GetSingle(beatmapElement, "difficulty_rating"),
                        ApproachRate: GetSingle(beatmapElement, "ar"),
                        CircleSize: GetSingle(beatmapElement, "cs"),
                        HpDrainRate: GetSingle(beatmapElement, "drain"),
                        OverallDifficulty: GetSingle(beatmapElement, "accuracy"),
                        Bpm: GetSingle(beatmapElement, "bpm"),
                        HitLength: GetInt32(beatmapElement, "hit_length"),
                        CircleCount: GetInt32(beatmapElement, "count_circles"),
                        SliderCount: GetInt32(beatmapElement, "count_sliders"),
                        SpinnerCount: GetInt32(beatmapElement, "count_spinners")));
                }
            }

            sets.Add(new BeatmapMirrorSet(
                Mirror: mirror,
                Id: id,
                Title: GetString(setElement, "title"),
                TitleUnicode: GetString(setElement, "title_unicode"),
                Artist: GetString(setElement, "artist"),
                ArtistUnicode: GetString(setElement, "artist_unicode"),
                Status: ToRankedStatus(GetInt32(setElement, "ranked")),
                Creator: GetString(setElement, "creator"),
                CoverUrl: TryGetCoverUrl(mirror, id, setElement),
                HasVideo: GetBoolean(setElement, "video"),
                Beatmaps: beatmaps.OrderBy(beatmap => beatmap.StarRating).ToArray()));
        }

        return sets;
    }

    private static string? TryGetCoverUrl(BeatmapMirrorKind mirror, long setId, JsonElement setElement)
    {
        if (mirror == BeatmapMirrorKind.Catboy)
            return setId > 0 ? $"https://assets.ppy.sh/beatmaps/{setId}/covers/card.jpg" : null;

        if (!setElement.TryGetProperty("covers", out var covers) || covers.ValueKind != JsonValueKind.Object)
            return null;

        return covers.TryGetProperty("card", out var card) && card.ValueKind == JsonValueKind.String ? card.GetString() : null;
    }

    private static string ToApiSort(BeatmapMirrorSort sort) => sort switch
    {
        BeatmapMirrorSort.Title => "title",
        BeatmapMirrorSort.Artist => "artist",
        BeatmapMirrorSort.Bpm => "beatmaps.bpm",
        BeatmapMirrorSort.DifficultyRating => "beatmaps.difficulty_rating",
        BeatmapMirrorSort.HitLength => "beatmaps.hit_length",
        BeatmapMirrorSort.PassCount => "beatmaps.passcount",
        BeatmapMirrorSort.PlayCount => "beatmaps.playcount",
        BeatmapMirrorSort.TotalLength => "beatmaps.total_length",
        BeatmapMirrorSort.FavouriteCount => "favourite_count",
        BeatmapMirrorSort.LastUpdated => "last_updated",
        BeatmapMirrorSort.RankedDate => "ranked_date",
        BeatmapMirrorSort.SubmittedDate => "submitted_date",
        _ => "ranked_date",
    };

    private static string ToApiOrder(BeatmapMirrorOrder order) => order == BeatmapMirrorOrder.Ascending ? "asc" : "desc";

    private static BeatmapRankedStatus ToRankedStatus(int value) => value switch
    {
        1 => BeatmapRankedStatus.Ranked,
        2 => BeatmapRankedStatus.Approved,
        3 => BeatmapRankedStatus.Qualified,
        4 => BeatmapRankedStatus.Loved,
        0 => BeatmapRankedStatus.Pending,
        -1 => BeatmapRankedStatus.WorkInProgress,
        -2 => BeatmapRankedStatus.Graveyard,
        _ => BeatmapRankedStatus.Pending,
    };

    private static long GetInt64(JsonElement element, string name) => element.TryGetProperty(name, out var property) && property.TryGetInt64(out var value) ? value : 0L;

    private static int GetInt32(JsonElement element, string name) => element.TryGetProperty(name, out var property) && property.TryGetInt32(out var value) ? value : 0;

    private static float GetSingle(JsonElement element, string name) => element.TryGetProperty(name, out var property) && property.TryGetSingle(out var value) ? value : 0f;

    private static string GetString(JsonElement element, string name) => element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() ?? string.Empty : string.Empty;

    private static bool GetBoolean(JsonElement element, string name) => element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.True;
}
