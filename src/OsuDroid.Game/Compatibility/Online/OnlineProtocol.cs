using System.Globalization;
using System.Net.Http.Headers;

namespace OsuDroid.Game.Compatibility.Online;

public sealed record OnlineRequest(string Url, IReadOnlyDictionary<string, string> Fields)
{
    public FormUrlEncodedContent ToFormContent() => new(Fields);
}

public sealed record ReplayUploadRequest(
    string Url,
    IReadOnlyDictionary<string, string> Fields,
    string ReplayFieldName,
    string ReplayFileName,
    string ReplayPath);

public enum BeatmapLeaderboardScoringMode
{
    Score,
    PerformancePoints,
}

public static class OnlineProtocol
{
    public static OnlineRequest CreateLoginRequest(string username, string password) => new(
        OsuDroidOnlineConstants.Endpoint + "login.php",
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["username"] = username,
            ["password"] = OnlinePasswordHasher.HashPassword(password),
            ["version"] = OsuDroidOnlineConstants.OnlineVersion,
        });

    public static OnlineRequest CreateLeaderboardRequest(string beatmapMd5, long userId, BeatmapLeaderboardScoringMode mode) => new(
        OsuDroidOnlineConstants.Endpoint + "getrank.php",
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["hash"] = beatmapMd5,
            ["uid"] = userId.ToString(CultureInfo.InvariantCulture),
            ["type"] = mode == BeatmapLeaderboardScoringMode.PerformancePoints ? "pp" : "score",
        });

    public static ReplayUploadRequest CreateReplayUploadRequest(
        long userId,
        string sessionId,
        string beatmapFilename,
        string beatmapMd5,
        string scoreData,
        string replayPath,
        string replayChecksum)
    {
        var replayFileName = Path.GetFileName(replayPath);
        return new ReplayUploadRequest(
            OsuDroidOnlineConstants.Endpoint + "submit.php",
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["userID"] = userId.ToString(CultureInfo.InvariantCulture),
                ["ssid"] = sessionId,
                ["filename"] = beatmapFilename.Trim(),
                ["hash"] = beatmapMd5,
                ["data"] = scoreData,
                ["version"] = OsuDroidOnlineConstants.OnlineVersion,
                ["replayFileChecksum"] = replayChecksum,
            },
            "replayFile",
            replayFileName,
            replayPath);
    }

    public static string GetReplayUrl(int playId, BeatmapLeaderboardScoringMode mode)
    {
        var folder = mode == BeatmapLeaderboardScoringMode.PerformancePoints ? "bestpp" : "upload";
        return $"{OsuDroidOnlineConstants.Endpoint}{folder}/{playId}.odr";
    }

    public static MultipartFormDataContent ToMultipartContent(ReplayUploadRequest request, Stream replayStream)
    {
        var content = new MultipartFormDataContent();

        foreach (var field in request.Fields)
            content.Add(new StringContent(field.Value), field.Key);

        var replay = new StreamContent(replayStream);
        replay.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(replay, request.ReplayFieldName, request.ReplayFileName);
        return content;
    }
}
