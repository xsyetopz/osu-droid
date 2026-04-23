using System.Text.Json;
using System.Text.Json.Serialization;

namespace OsuDroid.Game.Compatibility.Multiplayer;

public static class MultiplayerProtocol
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static Uri BuildRoomListUri(RoomListQuery query)
    {
        var builder = new UriBuilder(MultiplayerConstants.Host)
        {
            Path = MultiplayerConstants.GetRoomsPath.TrimStart('/'),
        };

        if (query.Sign is null && query.Query is null)
        {
            return builder.Uri;
        }

        var parameters = new Dictionary<string, string?>
        {
            ["sign"] = query.Sign,
            ["query"] = query.Query,
            ["uid"] = query.UserId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["sessionId"] = query.SessionId,
        };

        builder.Query = string.Join("&", parameters
            .Where(pair => pair.Value is not null)
            .Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value!)}"));

        return builder.Uri;
    }

    public static Uri CreateRoomUri() => new(new Uri(MultiplayerConstants.Host), MultiplayerConstants.CreateRoomPath);

    public static CreateRoomRequestDto CreateRoomRequest(
        string name,
        RoomBeatmapDto? beatmap,
        long hostUserId,
        string sessionId,
        string? sign,
        string? password = null,
        int maxPlayers = 8) => new(
            hostUserId,
            name,
            maxPlayers,
            beatmap,
            string.IsNullOrWhiteSpace(password) ? null : password,
            MultiplayerConstants.ApiVersion,
            sessionId,
            sign);

    public static string SerializeCreateRoomRequest(CreateRoomRequestDto request) => JsonSerializer.Serialize(request, s_jsonOptions);
}
