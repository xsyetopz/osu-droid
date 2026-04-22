using OsuDroid.Game.Beatmaps;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    private void ApplyMusicPreviewSetting()
    {
        menuMusicPreviewEnabled = options.GetBoolValue("musicpreview");
        if (!menuMusicPreviewEnabled)
        {
            musicController.Execute(MenuMusicCommand.Stop);
            mainMenu.SetNowPlaying(musicController.State);
            return;
        }

        musicController.Execute(MenuMusicCommand.Play);
        if (!musicController.State.IsPlaying)
            QueueStartupPlaylist(beatmapLibrary);

        mainMenu.SetNowPlaying(musicController.State);
    }

    private void QueueStartupPlaylist(IBeatmapLibrary library)
    {
        if (!menuMusicPreviewEnabled)
            return;

        var snapshot = library.Snapshot;
        if (snapshot.Sets.Count == 0)
            snapshot = library.Load();
        if (snapshot.Sets.Count == 0)
            return;

        var tracks = CreateMenuPlaylist(snapshot).ToArray();
        if (tracks.Length == 0)
            return;

        musicController.SetPlaylist(tracks, random.Next(tracks.Length), true);
    }

    private IEnumerable<MenuTrack> CreateMenuPlaylist(BeatmapLibrarySnapshot snapshot) => snapshot.Sets
        .SelectMany(set => set.Beatmaps
            .Where(static beatmap => !string.IsNullOrWhiteSpace(beatmap.AudioFilename))
            .Select(beatmap => (Set: set, Beatmap: beatmap)))
        .Select(pair => CreateMenuTrack(pair.Set, pair.Beatmap))
        .Where(track => File.Exists(track.AudioPath))
        .OrderBy(_ => random.Next());

    private MenuTrack CreateMenuTrack(BeatmapSetInfo set, BeatmapInfo beatmap)
    {
        var audioPath = beatmap.GetAudioPath(Services.Paths.Songs);
        return new MenuTrack(
            $"beatmap:{set.Directory}/{beatmap.Filename}",
            $"{DisplayArtist(beatmap)} - {DisplayTitle(beatmap)}",
            audioPath,
            beatmap.EffectivePreviewTime,
            (int)Math.Clamp(beatmap.Length, 0L, int.MaxValue),
            beatmap.MostCommonBpm,
            set.Directory,
            beatmap.Filename);
    }

    private void PreserveDownloaderMusic()
    {
        preservedDownloaderMusicState = musicController.State;
    }

    private void RestoreDownloaderMusic()
    {
        if (!menuMusicPreviewEnabled || preservedDownloaderMusicState is not { IsPlaying: true } state)
            return;

        preservedDownloaderMusicState = null;
        if (TryQueueBeatmapPreview(state.BeatmapSetDirectory, state.BeatmapFilename, true))
            mainMenu.SetNowPlaying(musicController.State);
    }

    private bool TryQueueBeatmapPreview(string? setDirectory, string? beatmapFilename, bool play)
    {
        if (string.IsNullOrWhiteSpace(setDirectory) || string.IsNullOrWhiteSpace(beatmapFilename))
            return false;

        var snapshot = beatmapLibrary.Snapshot;
        if (snapshot.Sets.Count == 0)
            snapshot = beatmapLibrary.Load();

        var set = snapshot.Sets.FirstOrDefault(candidate => string.Equals(candidate.Directory, setDirectory, StringComparison.Ordinal));
        var beatmap = set?.Beatmaps.FirstOrDefault(candidate => string.Equals(candidate.Filename, beatmapFilename, StringComparison.Ordinal));
        if (set is null || beatmap is null)
            return false;

        var audioPath = beatmap.GetAudioPath(Services.Paths.Songs);
        if (!File.Exists(audioPath))
            return false;

        musicController.Queue(CreateMenuTrack(set, beatmap), play);
        return true;
    }

    private static string DisplayTitle(BeatmapInfo beatmap) => string.IsNullOrWhiteSpace(beatmap.TitleUnicode) ? beatmap.Title : beatmap.TitleUnicode;

    private static string DisplayArtist(BeatmapInfo beatmap) => string.IsNullOrWhiteSpace(beatmap.ArtistUnicode) ? beatmap.Artist : beatmap.ArtistUnicode;
}
