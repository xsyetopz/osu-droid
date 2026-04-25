using NUnit.Framework;
using OsuDroid.Game.Compatibility.Online;

namespace OsuDroid.Game.Tests;

[TestFixture]
public sealed class OnlineProtocolTests
{
    [Test]
    public void LoginRequestMatchesOsuDroidEndpointAndFields()
    {
        OnlineRequest request = OnlineProtocol.CreateLoginRequest("player", "password");

        Assert.That(request.Url, Is.EqualTo("https://osudroid.moe/api/login.php"));
        Assert.That(request.Fields["username"], Is.EqualTo("player"));
        Assert.That(request.Fields["version"], Is.EqualTo("60"));
        Assert.That(request.Fields["password"], Is.EqualTo(OnlinePasswordHasher.HashPassword("password")));
        Assert.That(request.Fields["password"], Has.Length.EqualTo(32));
    }

    [Test]
    public void ResponseParserKeepsOsuDroidSuccessContract()
    {
        OnlineResponse response = OnlineResponseParser.ParseLines([
            "SUCCESS",
            "123 ssid 4 5000 12.5 0.987 player https://avatar",
        ]);

        Assert.That(response.IsSuccess, Is.True);

        LoginProfile profile = OnlineResponseParser.ParseLoginProfile(response.Lines);
        Assert.That(profile.UserId, Is.EqualTo(123));
        Assert.That(profile.SessionId, Is.EqualTo("ssid"));
        Assert.That(profile.Username, Is.EqualTo("player"));
    }

    [Test]
    public void ReplayUrlsMatchOsuDroidLeaderboardMode()
    {
        Assert.That(OnlineProtocol.GetReplayUrl(42, BeatmapLeaderboardScoringMode.Score), Is.EqualTo("https://osudroid.moe/api/upload/42.odr"));
        Assert.That(OnlineProtocol.GetReplayUrl(42, BeatmapLeaderboardScoringMode.PerformancePoints), Is.EqualTo("https://osudroid.moe/api/bestpp/42.odr"));
    }
}
