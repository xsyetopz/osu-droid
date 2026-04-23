using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    private bool HandleOptionsUiAction(UiAction action, VirtualViewport viewport)
    {
        if (action == UiAction.OptionsBack)
        {
            BackToMainMenu();
            return true;
        }

        if (activeScene != ActiveScene.Options || !IsOptionsAction(action))
            return false;

        options.HandleAction(action, viewport);
        PlayPendingOptionsSfx();
        if (action == UiAction.OptionsToggleMusicPreview)
            ApplyMusicPreviewSetting();
        return true;
    }

    private void ApplyRoute(MainMenuRoute route)
    {
        var start = PerfDiagnostics.Start();
        if (route == MainMenuRoute.Exit)
        {
            musicController.Execute(MenuMusicCommand.Stop);
            mainMenu.SetNowPlaying(musicController.State);
        }

        activeScene = route switch
        {
            MainMenuRoute.Settings => ActiveScene.Options,
            MainMenuRoute.Solo => EnterSongSelectScene(),
            _ => activeScene,
        };

        PerfDiagnostics.Log("core.applyRoute", start, $"route={route} active={activeScene}");
    }

    private ActiveScene EnterSongSelectScene()
    {
        songSelect.Enter(musicController.State.BeatmapSetDirectory, musicController.State.BeatmapFilename);
        return ActiveScene.SongSelect;
    }
}
