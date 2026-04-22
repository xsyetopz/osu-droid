namespace OsuDroid.Game.Runtime;

public sealed class NoOpMenuMusicController : IMenuMusicController
{
    public MenuMusicCommand LastCommand { get; private set; }

    public MenuNowPlayingState State { get; private set; } = new();

    public void SetPreviewPlayer(IBeatmapPreviewPlayer player)
    {
    }

    public void Queue(MenuTrack track, bool play) => State = MenuMusicStateFactory.Create(track, false);

    public void SetPlaylist(IReadOnlyList<MenuTrack> tracks, int startIndex, bool play)
    {
        if (tracks.Count > 0)
            State = MenuMusicStateFactory.Create(tracks[Math.Clamp(startIndex, 0, tracks.Count - 1)], false);
    }

    public void Execute(MenuMusicCommand command) => LastCommand = command;

    public void Update(TimeSpan elapsed)
    {
    }

    public bool TryReadSpectrum1024(float[] destination) => false;
}
