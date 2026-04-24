using OsuDroid.Game.Beatmaps.Difficulty;
using OsuDroid.Game.Compatibility.Online;
using OsuDroid.Game.Runtime;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
#pragma warning disable IDE0072 // Main menu routing keeps unknown routes on the current scene.
    private bool HandleOptionsUiAction(UiAction action, VirtualViewport viewport)
    {
        if (action == UiAction.OptionsBack)
        {
            BackToMainMenu();
            return true;
        }

        if (_activeScene != ActiveScene.Options || !IsOptionsAction(action))
        {
            return false;
        }

        _options.HandleAction(action, viewport);
        ApplyChangedOptionsSetting(_options.ConsumeChangedSettingKey());
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
            case "stayOnline":
                ApplyOnlinePanelSetting();
                break;
            case "registerAcc":
                PendingExternalUrl = $"https://{OsuDroidOnlineConstants.Hostname}/user/?action=register";
                break;
            case "update":
                PendingExternalUrl = OsuDroidOnlineConstants.UpdateEndpointPrefix + CurrentUpdateLanguageCode();
                break;
            case "backup":
                _options.ShowStatusMessage(_settingsBackupService.Export()
                    ? "LegacyLanguagePack_config_backup_info_success"
                    : "LegacyLanguagePack_config_backup_info_fail");
                break;
            case "restore":
                if (_settingsBackupService.Import())
                {
                    _options.ReloadValuesFromStore();
                    ApplyRestoredOptionsSettings();
                    _options.ShowStatusMessage("LegacyLanguagePack_config_backup_restore_info_success");
                }
                else
                {
                    _options.ShowStatusMessage("LegacyLanguagePack_config_backup_restore_info_fail");
                }

                break;
            case "clear_beatmap_cache":
                _beatmapLibrary.ClearBeatmapCache();
                _options.ShowStatusMessage("LegacyLanguagePack_library_cleared");
                break;
            case "clear_properties":
                _beatmapLibrary.ClearProperties();
                break;
            case "preferNoVideoDownloads":
                ApplyDownloadPreferenceSetting();
                break;
            case "forceromanized":
                ApplyRomanizedPreferenceSetting();
                break;
            default:
                break;
        }
    }

    private void ApplyOptionAudioVolumes()
    {
        ApplyMusicVolumeSetting();
        ApplyEffectVolumeSetting();
    }

    private void ApplyRestoredOptionsSettings()
    {
        ApplyOptionAudioVolumes();
        ApplyMusicPreviewSetting();
        ApplyDifficultyAlgorithmSetting();
        ApplyRomanizedPreferenceSetting();
        ApplyDownloadPreferenceSetting();
        ApplyOnlinePanelSetting();
    }

    private void ApplyMusicVolumeSetting() => _previewPlayer.SetVolume(_options.GetIntValue("bgmvolume") / 100f);

    private void ApplyEffectVolumeSetting() => _activeMenuSfxPlayer.SetVolume(_options.GetIntValue("soundvolume") / 100f);

    private void ApplyDifficultyAlgorithmSetting()
    {
        DifficultyAlgorithm algorithm = _options.GetIntValue("difficultyAlgorithm") == 1 ? DifficultyAlgorithm.Standard : DifficultyAlgorithm.Droid;
        _songSelect.SetDisplayAlgorithm(algorithm);
    }

    private void ApplyRomanizedPreferenceSetting()
    {
        bool forceRomanized = _options.GetBoolValue("forceromanized");
        _songSelect.SetForceRomanized(forceRomanized);
        _beatmapDownloader.SetForceRomanized(forceRomanized);
    }

    private void ApplyDownloadPreferenceSetting() => _beatmapDownloader.SetPreferNoVideoDownloads(_options.GetBoolValue("preferNoVideoDownloads"));

    private void ApplyOnlinePanelSetting()
    {
        OnlineProfilePanelState? state = CreateOnlinePanelState(Services.OnlineProfile);
        _mainMenu.SetOnlinePanelState(state);
        _songSelect.SetOnlinePanelState(state);
    }

    private void ApplyRoute(MainMenuRoute route)
    {
        long start = PerfDiagnostics.Start();
        if (route == MainMenuRoute.Exit)
        {
            _musicController.Execute(MenuMusicCommand.Stop);
            _mainMenu.SetNowPlaying(_musicController.State);
        }

        _activeScene = route switch
        {
            MainMenuRoute.Settings => ActiveScene.Options,
            MainMenuRoute.Solo => EnterSongSelectScene(),
            _ => _activeScene,
        };

        PerfDiagnostics.Log("core.applyRoute", start, $"route={route} active={_activeScene}");
    }

    private ActiveScene EnterSongSelectScene()
    {
        _pendingSongSelectBeatmapSetDirectory = _musicController.State.BeatmapSetDirectory;
        _pendingSongSelectBeatmapFilename = _musicController.State.BeatmapFilename;

        if (_beatmapProcessingService.HasPendingWork())
        {
            _beatmapProcessingService.Start();
            return ActiveScene.BeatmapProcessing;
        }

        return EnterSongSelectOrDownloader();
    }

    private ActiveScene EnterSongSelectOrDownloader()
    {
        if (_beatmapLibrary.Load().Sets.Count == 0)
        {
            ClearPendingSongSelectBeatmap();
            PreserveDownloaderMusic();
            _beatmapDownloader.Enter();
            return ActiveScene.BeatmapDownloader;
        }

        _songSelect.Enter(_pendingSongSelectBeatmapSetDirectory, _pendingSongSelectBeatmapFilename);
        ClearPendingSongSelectBeatmap();
        return ActiveScene.SongSelect;
    }

    private void ClearPendingSongSelectBeatmap()
    {
        _pendingSongSelectBeatmapSetDirectory = null;
        _pendingSongSelectBeatmapFilename = null;
    }
}
