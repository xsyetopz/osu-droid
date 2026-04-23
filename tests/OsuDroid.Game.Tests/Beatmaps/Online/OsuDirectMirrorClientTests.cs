using System.Net;
using System.Reflection;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{

    [Test]
    public void OsuDirectSearchUrlMatchesLegacyEndpoint()
    {
        var client = new OsuDirectMirrorClient(new HttpClient(new EmptyHandler()));
        var uri = client.CreateSearchUri(new BeatmapMirrorSearchRequest("camellia", Offset: 50, Limit: 25));

        Assert.That(uri.GetLeftPart(UriPartial.Path), Is.EqualTo("https://osu.direct/api/v2/search"));
        Assert.That(uri.Query, Does.Contain("sort=ranked_date%3Adesc"));
        Assert.That(uri.Query, Does.Contain("mode=0"));
        Assert.That(uri.Query, Does.Contain("query=camellia"));
        Assert.That(uri.Query, Does.Contain("offset=50"));
        Assert.That(uri.Query, Does.Contain("amount=25"));
    }
    [Test]
    public void OsuDirectDownloadUrlSupportsNoVideo()
    {
        var client = new OsuDirectMirrorClient(new HttpClient(new EmptyHandler()));

        Assert.That(client.CreateDownloadUri(123, true).ToString(), Is.EqualTo("https://osu.direct/api/d/123"));
        Assert.That(client.CreateDownloadUri(123, false).ToString(), Is.EqualTo("https://osu.direct/api/d/123?noVideo=1"));
        Assert.That(client.CreatePreviewUri(BeatmapMirrorKind.OsuDirect, 456).ToString(), Is.EqualTo("https://osu.direct/api/media/preview/456"));
    }
    [Test]
    public void CatboyUrlsMatchLegacyEndpoints()
    {
        var client = new OsuDirectMirrorClient(new HttpClient(new EmptyHandler()));
        var uri = client.CreateSearchUri(new BeatmapMirrorSearchRequest("camellia", Offset: 50, Limit: 25, Mirror: BeatmapMirrorKind.Catboy, Status: BeatmapRankedStatus.Ranked));

        Assert.That(uri.GetLeftPart(UriPartial.Path), Is.EqualTo("https://catboy.best/api/v2/search"));
        Assert.That(uri.Query, Does.Contain("sort=ranked_date%3Adesc"));
        Assert.That(uri.Query, Does.Contain("mode=0"));
        Assert.That(uri.Query, Does.Contain("query=camellia"));
        Assert.That(uri.Query, Does.Contain("offset=50"));
        Assert.That(uri.Query, Does.Contain("limit=25"));
        Assert.That(uri.Query, Does.Contain("status=1"));
        Assert.That(client.CreateDownloadUri(BeatmapMirrorKind.Catboy, 123, false).ToString(), Is.EqualTo("https://catboy.best/d/123"));
        Assert.That(client.CreatePreviewUri(BeatmapMirrorKind.Catboy, 456).ToString(), Is.EqualTo("https://catboy.best/preview/audio/456"));
    }
    [Test]
    public async Task SearchParserReadsMirrorMetadata()
    {
        var client = new OsuDirectMirrorClient(new HttpClient(new JsonHandler("""
            [
              {
                "id": 123,
                "title": "Title",
                "title_unicode": "タイトル",
                "artist": "Artist",
                "artist_unicode": "アーティスト",
                "ranked": 4,
                "creator": "Mapper",
                "video": true,
                "covers": { "card": "https://example.test/card.jpg" },
                "beatmaps": [
                  { "id": 456, "version": "Hard", "difficulty_rating": 4.2, "ar": 9, "cs": 4, "drain": 5, "accuracy": 8, "bpm": 180, "hit_length": 95, "count_circles": 100, "count_sliders": 50, "count_spinners": 1 }
                ]
              }
            ]
            """)));

        var sets = await client.SearchAsync(new BeatmapMirrorSearchRequest("title"), CancellationToken.None).ConfigureAwait(false);

        Assert.That(sets, Has.Count.EqualTo(1));
        Assert.That(sets[0].Status, Is.EqualTo(BeatmapRankedStatus.Loved));
        Assert.That(sets[0].DisplayTitle, Is.EqualTo("タイトル"));
        Assert.That(sets[0].Beatmaps[0].CircleCount, Is.EqualTo(100));
        Assert.That(sets[0].Beatmaps[0].SliderCount, Is.EqualTo(50));
        Assert.That(sets[0].Beatmaps[0].SpinnerCount, Is.EqualTo(1));
    }
    [Test]
    public async Task SearchParserKeepsOnlyOsuStandardDifficulties()
    {
        var client = new OsuDirectMirrorClient(new HttpClient(new JsonHandler("""
            [
              {
                "id": 123,
                "title": "Mixed",
                "artist": "Artist",
                "ranked": 1,
                "creator": "Mapper",
                "beatmaps": [
                  { "id": 1, "version": "Standard", "mode_int": 0, "difficulty_rating": 2.1 },
                  { "id": 2, "version": "Taiko", "mode_int": 1, "difficulty_rating": 3.1 },
                  { "id": 3, "version": "Catch", "mode": "fruits", "difficulty_rating": 4.1 },
                  { "id": 4, "version": "Mania", "ruleset": "mania", "difficulty_rating": 5.1 }
                ]
              },
              {
                "id": 124,
                "title": "Nonstandard",
                "artist": "Artist",
                "ranked": 1,
                "creator": "Mapper",
                "beatmaps": [
                  { "id": 5, "version": "Taiko", "ruleset_id": 1, "difficulty_rating": 3.1 }
                ]
              }
            ]
            """)));

        var sets = await client.SearchAsync(new BeatmapMirrorSearchRequest("mixed"), CancellationToken.None).ConfigureAwait(false);

        Assert.That(sets, Has.Count.EqualTo(1));
        Assert.That(sets[0].Beatmaps, Has.Count.EqualTo(1));
        Assert.That(sets[0].Beatmaps[0].Version, Is.EqualTo("Standard"));
        Assert.That(sets[0].Beatmaps[0].Mode, Is.EqualTo(0));
    }
}
