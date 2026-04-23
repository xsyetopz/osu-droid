using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;
using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Compatibility.Online;

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
        ApplyChangedOptionsSetting(options.ConsumeChangedSettingKey());
        PlayPendingOptionsSfx();
        return true;
    }

    private void ApplyChangedOptionsSetting(string? key)
    {
        switch (key)
        {
            case "musicpreview":
                ApplyMusicPreviewSetting();
                break;
            case "bgmvolume":
                ApplyMusicVolumeSetting();
                break;
            case "soundvolume":
                ApplyEffectVolumeSetting();
                break;
            case "difficultyAlgorithm":
                ApplyDifficultyAlgorithmSetting();
                break;
            case "registerAcc":
                PendingExternalUrl = $"https://{OsuDroidOnlineConstants.Hostname}/user/?action=register";
                break;
            case "update":
                PendingExternalUrl = OsuDroidOnlineConstants.UpdateEndpointPrefix + CurrentUpdateLanguageCode();
                break;
            case "preferNoVideoDownloads":
                ApplyDownloadPreferenceSetting();
                break;
            case "forceromanized":
                ApplyRomanizedPreferenceSetting();
                break;
        }
    }

    private void ApplyOptionAudioVolumes()
    {
        ApplyMusicVolumeSetting();
        ApplyEffectVolumeSetting();
    }

    private void ApplyMusicVolumeSetting() => previewPlayer.SetVolume(options.GetIntValue("bgmvolume") / 100f);

    private void ApplyEffectVolumeSetting() => activeMenuSfxPlayer.SetVolume(options.GetIntValue("soundvolume") / 100f);

    private void ApplyDifficultyAlgorithmSetting()
    {
        var algorithm = options.GetIntValue("difficultyAlgorithm") == 1 ? DifficultyAlgorithm.Standard : DifficultyAlgorithm.Droid;
        songSelect.SetDisplayAlgorithm(algorithm);
    }

    private void ApplyRomanizedPreferenceSetting()
    {
        var forceRomanized = options.GetBoolValue("forceromanized");
        songSelect.SetForceRomanized(forceRomanized);
        beatmapDownloader.SetForceRomanized(forceRomanized);
    }

    private void ApplyDownloadPreferenceSetting() => beatmapDownloader.SetPreferNoVideoDownloads(options.GetBoolValue("preferNoVideoDownloads"));

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
        pendingSongSelectBeatmapSetDirectory = musicController.State.BeatmapSetDirectory;
        pendingSongSelectBeatmapFilename = musicController.State.BeatmapFilename;

        if (beatmapProcessingService.HasPendingWork())
        {
            beatmapProcessingService.Start();
            return ActiveScene.BeatmapProcessing;
        }

        songSelect.Enter(pendingSongSelectBeatmapSetDirectory, pendingSongSelectBeatmapFilename);
        pendingSongSelectBeatmapSetDirectory = null;
        pendingSongSelectBeatmapFilename = null;
        return ActiveScene.SongSelect;
    }
}
