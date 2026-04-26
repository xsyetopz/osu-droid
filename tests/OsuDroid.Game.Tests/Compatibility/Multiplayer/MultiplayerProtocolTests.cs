using System.Text.Json;
using NUnit.Framework;
using OsuDroid.Game.Compatibility.Multiplayer;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class MultiplayerProtocolTests
{
    [Test]
    public void RoomListUriMatchesOsuDroidHostAndQueryKeys()
    {
        Uri uri = MultiplayerProtocol.BuildRoomListUri(new RoomListQuery("abc", 12, "ssid", "sig"));

        Assert.That(
            uri.GetLeftPart(UriPartial.Path),
            Is.EqualTo("https://multi.osudroid.moe/getrooms")
        );
        Assert.That(uri.Query, Does.Contain("sign=sig"));
        Assert.That(uri.Query, Does.Contain("query=abc"));
        Assert.That(uri.Query, Does.Contain("uid=12"));
        Assert.That(uri.Query, Does.Contain("sessionId=ssid"));
    }

    [Test]
    public void CreateRoomPayloadUsesOsuDroidKeysAndApiVersion()
    {
        CreateRoomRequestDto request = MultiplayerProtocol.CreateRoomRequest(
            "room",
            new RoomBeatmapDto("md5", "title", "artist", "creator", "hard"),
            99,
            "ssid",
            "sig",
            "secret",
            6
        );

        using var json = JsonDocument.Parse(
            MultiplayerProtocol.SerializeCreateRoomRequest(request)
        );
        JsonElement root = json.RootElement;
        Assert.That(root.GetProperty("hostUid").GetInt64(), Is.EqualTo(99));
        Assert.That(root.GetProperty("name").GetString(), Is.EqualTo("room"));
        Assert.That(root.GetProperty("maxPlayers").GetInt32(), Is.EqualTo(6));
        Assert.That(root.GetProperty("version").GetInt32(), Is.EqualTo(9));
        Assert.That(root.GetProperty("sessionId").GetString(), Is.EqualTo("ssid"));
        Assert.That(root.GetProperty("beatmap").GetProperty("md5").GetString(), Is.EqualTo("md5"));
    }

    [Test]
    public void SocketEventNamesMatchOsuDroidRoomApi()
    {
        Assert.That(RoomSocketEvents.InitialConnection, Is.EqualTo("initialConnection"));
        Assert.That(RoomSocketEvents.EmitBeatmapLoadComplete, Is.EqualTo("beatmapLoadComplete"));
        Assert.That(RoomSocketEvents.EmitPlayerModsChanged, Is.EqualTo("playerModsChanged"));
    }
}
