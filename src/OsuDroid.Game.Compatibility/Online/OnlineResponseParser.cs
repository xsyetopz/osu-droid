using System.Globalization;

namespace OsuDroid.Game.Compatibility.Online;

public sealed record OnlineResponse(bool IsSuccess, IReadOnlyList<string> Lines, string? FailureMessage)
{
    public static OnlineResponse Failure(string message, IReadOnlyList<string>? lines = null) => new(false, lines ?? Array.Empty<string>(), message);
    public static OnlineResponse Success(IReadOnlyList<string> lines) => new(true, lines, null);
}

public sealed record LoginProfile(
    long UserId,
    string SessionId,
    long Rank,
    long Score,
    float PerformancePoints,
    float Accuracy,
    string Username,
    string AvatarUrl);

public static class OnlineResponseParser
{
    public static OnlineResponse ParseLines(IEnumerable<string> lines)
    {
        string[] parsed = lines.ToArray();

        return parsed.Length == 0 || parsed[0].Length == 0
            ? OnlineResponse.Failure("Got empty response", parsed)
            : !string.Equals(parsed[0], "SUCCESS", StringComparison.Ordinal)
            ? OnlineResponse.Failure(parsed.Length >= 2 ? parsed[1] : "Unknown server error", parsed)
            : OnlineResponse.Success(parsed);
    }

    public static LoginProfile ParseLoginProfile(IReadOnlyList<string> successLines)
    {
        if (successLines.Count < 2)
        {
            throw new FormatException("Invalid server response");
        }

        string[] fields = successLines[1].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        return fields.Length < 7
            ? throw new FormatException("Invalid server response")
            : new LoginProfile(
            long.Parse(fields[0], CultureInfo.InvariantCulture),
            fields[1],
            long.Parse(fields[2], CultureInfo.InvariantCulture),
            long.Parse(fields[3], CultureInfo.InvariantCulture),
            float.Parse(fields[4], CultureInfo.InvariantCulture),
            float.Parse(fields[5], CultureInfo.InvariantCulture),
            fields[6],
            fields.Length >= 8 ? fields[7] : string.Empty);
    }
}
