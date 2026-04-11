using System.IO;
using OsuDroid.Game.Services.Local;

namespace OsuDroid.Game.Tests;

public class FileSystemLocalBeatmapLibraryServiceTests
{
    [Test]
    public async Task RefreshParsesBeatmapMetadataFromOsuFiles()
    {
        string songsRoot = Path.Combine(Path.GetTempPath(), $"osudroid-local-test-{Guid.NewGuid():N}");
        string beatmapFolder = Path.Combine(songsRoot, "123 Artist - Title");
        Directory.CreateDirectory(beatmapFolder);

        try
        {
            string beatmapPath = Path.Combine(beatmapFolder, "map.osu");
            await File.WriteAllTextAsync(beatmapPath, """
                osu file format v14

                [Metadata]
                Title:Title
                Artist:Artist
                Creator:Mapper
                Version:Insane
                BeatmapID:12345

                [Events]
                """).ConfigureAwait(false);

            FileSystemLocalBeatmapLibraryService service = new(new TestPlatformStorage(songsRoot));

            IReadOnlyList<BeatmapCard> beatmaps = await service.RefreshAsync().ConfigureAwait(false);

            Assert.That(beatmaps, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(beatmaps[0].Id, Is.EqualTo("12345"));
                Assert.That(beatmaps[0].Artist, Is.EqualTo("Artist"));
                Assert.That(beatmaps[0].Title, Is.EqualTo("Title"));
                Assert.That(beatmaps[0].DifficultyName, Is.EqualTo("Insane"));
                Assert.That(beatmaps[0].Mapper, Is.EqualTo("Mapper"));
                Assert.That(beatmaps[0].Status, Is.EqualTo("Local"));
            });
        }
        finally
        {
            if (Directory.Exists(songsRoot))
            {
                Directory.Delete(songsRoot, recursive: true);
            }
        }
    }

    private sealed class TestPlatformStorage(string songsRoot) : IPlatformStorage
    {
        public IReadOnlyList<string> GetSongRoots() => [songsRoot];
    }
}
