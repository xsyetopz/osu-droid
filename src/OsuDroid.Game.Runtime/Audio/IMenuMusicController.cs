namespace OsuDroid.Game.Runtime.Audio;

public interface IMenuMusicController
{
    MenuMusicCommand LastCommand { get; }

    MenuNowPlayingState State { get; }

    void SetPreviewPlayer(IBeatmapPreviewPlayer player);

    void Queue(MenuTrack track, bool play);

    void SetPlaylist(IReadOnlyList<MenuTrack> tracks, int startIndex, bool play);

    void Execute(MenuMusicCommand command);

    void Update(TimeSpan elapsed);

    bool TryReadSpectrum1024(float[] destination);
}
