using System.Text.Json.Serialization;

namespace OsuDroid.Game.Compatibility.Multiplayer;

public sealed record RoomBeatmapDto(
    [property: JsonPropertyName("md5")] string Md5,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("artist")] string Artist,
    [property: JsonPropertyName("creator")] string Creator,
    [property: JsonPropertyName("version")] string Version);

public sealed record CreateRoomRequestDto(
    [property: JsonPropertyName("hostUid")] long HostUid,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("maxPlayers")] int MaxPlayers,
    [property: JsonPropertyName("beatmap")] RoomBeatmapDto? Beatmap,
    [property: JsonPropertyName("password")] string? Password,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("sessionId")] string SessionId,
    [property: JsonPropertyName("sign")] string? Sign);

public sealed record RoomListQuery(string? Query, long UserId, string SessionId, string? Sign);
