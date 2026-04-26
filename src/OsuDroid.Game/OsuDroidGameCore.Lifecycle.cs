using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.Scenes.MainMenu;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    public void Update(TimeSpan elapsed)
    {
        if (_activeScene == ActiveScene.Startup)
        {
            _startup.Update(elapsed);
            if (_startup.ConsumeWelcomeSoundsRequest())
            {
                _activeMenuSfxPlayer.Play("welcome");
                _activeMenuSfxPlayer.Play("welcome_piano");
            }

            if (!_startup.IsComplete)
            {
                return;
            }

            _activeScene = ActiveScene.MainMenu;
            StartDeferredMenuMusic();
        }

        _musicController.Update(elapsed);
        _mainMenu.SetNowPlaying(_musicController.State);
        _mainMenu.SetSpectrum(
            _menuSpectrumBuffer,
            _musicController.TryReadSpectrum1024(_menuSpectrumBuffer)
        );

        if (_activeScene == ActiveScene.BeatmapDownloader)
        {
            _beatmapDownloader.Update(elapsed);
            return;
        }

        if (_activeScene == ActiveScene.BeatmapProcessing)
        {
            if (!_beatmapProcessingService.TryConsumeCompletedSnapshot(out _))
            {
                return;
            }

            _activeScene = EnterSongSelectOrDownloader();
        }

        if (_activeScene == ActiveScene.SongSelect)
        {
            _songSelect.Update(elapsed);
            return;
        }

        if (_activeScene == ActiveScene.Options)
        {
            _options.Update(elapsed);
            return;
        }

        if (_activeScene == ActiveScene.ModSelect)
        {
            _modSelect.Update(elapsed);
            return;
        }

        if (_activeScene != ActiveScene.MainMenu)
        {
            return;
        }

        _mainMenu.Update(elapsed);
        MainMenuRoute pendingRoute = _mainMenu.ConsumePendingRoute();
        if (pendingRoute == MainMenuRoute.None)
        {
            return;
        }

        LastRoute = pendingRoute;
        ApplyRoute(pendingRoute);
    }

    private void StartDeferredMenuMusic()
    {
        if (!_startMenuMusicAfterStartup)
        {
            return;
        }

        _startMenuMusicAfterStartup = false;
        if (
            !_menuMusicPreviewEnabled
            || string.IsNullOrWhiteSpace(_musicController.State.ArtistTitle)
        )
        {
            return;
        }

        _musicController.Execute(MenuMusicCommand.Play);
        _mainMenu.SetNowPlaying(_musicController.State);
    }
}
