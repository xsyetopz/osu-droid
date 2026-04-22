namespace OsuDroid.Game.Runtime;

internal static class MenuMusicStateFactory
{
    public static MenuNowPlayingState Create(MenuTrack track, bool isPlaying) => new(
        track.DisplayTitle,
        isPlaying,
        Math.Clamp(track.PreviewTimeMilliseconds, 0, Math.Max(track.LengthMilliseconds, 0)),
        Math.Max(track.LengthMilliseconds, 0),
        track.Bpm,
        track.BeatmapSetDirectory,
        track.BeatmapFilename);
}
