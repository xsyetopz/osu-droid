using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Runtime.Audio;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{
    private sealed class RecordingMenuMusicController : IMenuMusicController
    {
        public List<bool> SetPlaylistPlayFlags { get; } = [];

        public int PlayCommands { get; private set; }

        public MenuMusicCommand LastCommand { get; private set; }

        public MenuNowPlayingState State { get; private set; } = new();

        public void SetPreviewPlayer(IBeatmapPreviewPlayer player) { }

        public void Queue(MenuTrack track, bool play) =>
            State = new MenuNowPlayingState(track.DisplayTitle, play);

        public void SetPlaylist(IReadOnlyList<MenuTrack> tracks, int startIndex, bool play)
        {
            SetPlaylistPlayFlags.Add(play);
            if (tracks.Count > 0)
            {
                State = new MenuNowPlayingState(
                    tracks[Math.Clamp(startIndex, 0, tracks.Count - 1)].DisplayTitle,
                    play
                );
            }
        }

        public void Execute(MenuMusicCommand command)
        {
            LastCommand = command;
            if (command == MenuMusicCommand.Play)
            {
                PlayCommands++;
            }
        }

        public void Update(TimeSpan elapsed) { }

        public bool TryReadSpectrum1024(float[] destination) => false;
    }

    private sealed class StartupMusicLibrary : IBeatmapLibrary
    {
        private readonly BeatmapLibrarySnapshot _snapshot = new([
            new BeatmapSetInfo(
                1,
                "1 Artist - Title",
                [
                    new BeatmapInfo(
                        "Easy.osu",
                        "1 Artist - Title",
                        "md5",
                        null,
                        "audio.mp3",
                        null,
                        null,
                        1,
                        "Title",
                        string.Empty,
                        "Artist",
                        string.Empty,
                        "Mapper",
                        "Easy",
                        string.Empty,
                        string.Empty,
                        0,
                        5,
                        5,
                        5,
                        5,
                        1,
                        1,
                        120,
                        120,
                        120,
                        1000,
                        0,
                        1,
                        0,
                        0,
                        1,
                        false
                    ),
                ]
            ),
        ]);

        public BeatmapLibrarySnapshot Snapshot => _snapshot;

        public BeatmapLibrarySnapshot Load() => _snapshot;

        public BeatmapLibrarySnapshot Scan(IReadOnlySet<string>? forceUpdateDirectories = null) =>
            _snapshot;

        public void ApplyOnlineMetadata(string setDirectory, BeatmapOnlineMetadata metadata) { }

        public bool NeedsScanRefresh() => false;

        public BeatmapOptions GetOptions(string setDirectory) => new(setDirectory);

        public void SaveOptions(BeatmapOptions options) { }

        public IReadOnlyList<BeatmapCollection> GetCollections(
            string? selectedSetDirectory = null
        ) => [];

        public IReadOnlySet<string> GetCollectionSetDirectories(string name) =>
            new HashSet<string>(StringComparer.Ordinal);

        public bool CreateCollection(string name) => true;

        public void DeleteCollection(string name) { }

        public void ToggleCollectionMembership(string name, string setDirectory) { }

        public void DeleteBeatmapSet(string directory) { }

        public void ClearBeatmapCache() { }

        public void ClearProperties() { }
    }
}
