using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Runtime.Audio;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    private void ApplyMusicPreviewSetting()
    {
        _menuMusicPreviewEnabled = _options.GetBoolValue("musicpreview");
        if (!_menuMusicPreviewEnabled)
        {
            _musicController.Execute(MenuMusicCommand.Stop);
            _mainMenu.SetNowPlaying(_musicController.State);
            return;
        }

        _musicController.Execute(MenuMusicCommand.Play);
        if (!_musicController.State.IsPlaying)
        {
            QueueStartupPlaylist(_beatmapLibrary);
        }

        _mainMenu.SetNowPlaying(_musicController.State);
    }

    private void QueueStartupPlaylist(IBeatmapLibrary library, bool play = true)
    {
        if (!_menuMusicPreviewEnabled)
        {
            return;
        }

        BeatmapLibrarySnapshot snapshot = library.Snapshot;
        if (snapshot.Sets.Count == 0)
        {
            snapshot = library.Load();
        }

        if (snapshot.Sets.Count == 0)
        {
            return;
        }

        MenuTrack[] tracks = CreateMenuPlaylist(snapshot).ToArray();
        if (tracks.Length == 0)
        {
            return;
        }

        _musicController.SetPlaylist(tracks, _random.Next(tracks.Length), play);
        if (!play)
        {
            _startMenuMusicAfterStartup = true;
        }
    }

    private IEnumerable<MenuTrack> CreateMenuPlaylist(BeatmapLibrarySnapshot snapshot) =>
        snapshot
            .Sets.SelectMany(set =>
                set.Beatmaps.Where(static beatmap =>
                        !string.IsNullOrWhiteSpace(beatmap.AudioFilename)
                    )
                    .Select(beatmap => (Set: set, Beatmap: beatmap))
            )
            .Select(pair => CreateMenuTrack(pair.Set, pair.Beatmap))
            .Where(track => File.Exists(track.AudioPath))
            .OrderBy(_ => _random.Next());

    private MenuTrack CreateMenuTrack(BeatmapSetInfo set, BeatmapInfo beatmap)
    {
        string audioPath = beatmap.GetAudioPath(Services.Paths.Songs);
        return new MenuTrack(
            $"beatmap:{set.Directory}/{beatmap.Filename}",
            $"{DisplayArtist(beatmap)} - {DisplayTitle(beatmap)}",
            audioPath,
            beatmap.EffectivePreviewTime,
            (int)Math.Clamp(beatmap.Length, 0L, int.MaxValue),
            beatmap.MostCommonBpm,
            set.Directory,
            beatmap.Filename
        );
    }

    private void PreserveDownloaderMusic() =>
        _preservedDownloaderMusicState = _musicController.State;

    private void RestoreDownloaderMusic()
    {
        if (
            !_menuMusicPreviewEnabled
            || _preservedDownloaderMusicState is not { IsPlaying: true } state
        )
        {
            return;
        }

        _preservedDownloaderMusicState = null;
        if (TryQueueBeatmapPreview(state.BeatmapSetDirectory, state.BeatmapFilename, true))
        {
            _mainMenu.SetNowPlaying(_musicController.State);
        }
    }

    private bool TryQueueBeatmapPreview(string? setDirectory, string? beatmapFilename, bool play)
    {
        if (string.IsNullOrWhiteSpace(setDirectory) || string.IsNullOrWhiteSpace(beatmapFilename))
        {
            return false;
        }

        BeatmapLibrarySnapshot snapshot = _beatmapLibrary.Snapshot;
        if (snapshot.Sets.Count == 0)
        {
            snapshot = _beatmapLibrary.Load();
        }

        BeatmapSetInfo? set = snapshot.Sets.FirstOrDefault(candidate =>
            string.Equals(candidate.Directory, setDirectory, StringComparison.Ordinal)
        );
        BeatmapInfo? beatmap = set?.Beatmaps.FirstOrDefault(candidate =>
            string.Equals(candidate.Filename, beatmapFilename, StringComparison.Ordinal)
        );
        if (set is null || beatmap is null)
        {
            return false;
        }

        string audioPath = beatmap.GetAudioPath(Services.Paths.Songs);
        if (!File.Exists(audioPath))
        {
            return false;
        }

        _musicController.Queue(CreateMenuTrack(set, beatmap), play);
        return true;
    }

    private static string DisplayTitle(BeatmapInfo beatmap) =>
        string.IsNullOrWhiteSpace(beatmap.TitleUnicode) ? beatmap.Title : beatmap.TitleUnicode;

    private static string DisplayArtist(BeatmapInfo beatmap) =>
        string.IsNullOrWhiteSpace(beatmap.ArtistUnicode) ? beatmap.Artist : beatmap.ArtistUnicode;
}
