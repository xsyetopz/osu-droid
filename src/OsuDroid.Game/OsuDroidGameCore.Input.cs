using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    public void PressUiAction(UiAction action)
    {
        if (_activeScene == ActiveScene.MainMenu)
        {
            _mainMenu.Press(action);
        }
    }



    public void ReleaseUiAction()
    {
        if (_activeScene == ActiveScene.MainMenu)
        {
            _mainMenu.ReleasePress();
        }
    }



    public bool TryBeginUiDrag(string elementId, UiPoint point, VirtualViewport viewport)
    {
        if (_activeScene != ActiveScene.Options)
        {
            return false;
        }

        bool captured = _options.TryBeginSliderDrag(elementId, point, viewport);
        if (captured)
        {
            ApplyChangedOptionsSetting(_options.ConsumeChangedSettingKey());
        }

        return captured;
    }



    public bool TryBeginSceneScrollDrag(UiPoint point, VirtualViewport viewport, double timestampSeconds) => _activeScene switch
    {
        ActiveScene.Options => _options.TryBeginScrollDrag(point, viewport, timestampSeconds),
        ActiveScene.BeatmapDownloader => _beatmapDownloader.TryBeginScrollDrag(point, viewport, timestampSeconds),
        ActiveScene.SongSelect => _songSelect.TryBeginScrollDrag(point, viewport, timestampSeconds),
        ActiveScene.ModSelect => _modSelect.TryBeginScrollDrag(point, viewport, timestampSeconds),
        ActiveScene.Startup or ActiveScene.MainMenu or ActiveScene.BeatmapProcessing => false,
        _ => false,
    };

    public bool UpdateSceneScrollDrag(UiPoint point, VirtualViewport viewport, double timestampSeconds) => _activeScene switch
    {
        ActiveScene.Options => _options.UpdateScrollDrag(point, viewport, timestampSeconds),
        ActiveScene.BeatmapDownloader => _beatmapDownloader.UpdateScrollDrag(point, viewport, timestampSeconds),
        ActiveScene.SongSelect => _songSelect.UpdateScrollDrag(point, viewport, timestampSeconds),
        ActiveScene.ModSelect => _modSelect.UpdateScrollDrag(point, viewport, timestampSeconds),
        ActiveScene.Startup or ActiveScene.MainMenu or ActiveScene.BeatmapProcessing => false,
        _ => false,
    };

    public void EndSceneScrollDrag(UiPoint point, VirtualViewport viewport, double timestampSeconds)
    {
        switch (_activeScene)
        {
            case ActiveScene.Startup:
            case ActiveScene.MainMenu:
            case ActiveScene.BeatmapProcessing:
                break;
            case ActiveScene.Options:
                _options.EndScrollDrag(point, viewport, timestampSeconds);
                break;
            case ActiveScene.BeatmapDownloader:
                _beatmapDownloader.EndScrollDrag(point, viewport, timestampSeconds);
                break;
            case ActiveScene.SongSelect:
                _songSelect.EndScrollDrag(point, viewport, timestampSeconds);
                break;
            case ActiveScene.ModSelect:
                _modSelect.EndScrollDrag(point, viewport, timestampSeconds);
                break;
            default:
                break;
        }
    }



    public void UpdateUiDrag(UiPoint point, VirtualViewport viewport)
    {
        if (_activeScene != ActiveScene.Options)
        {
            return;
        }

        if (!_options.UpdateSliderDrag(point, viewport))
        {
            return;
        }

        ApplyChangedOptionsSetting(_options.ConsumeChangedSettingKey());
    }



    public void EndUiDrag(UiPoint point, VirtualViewport viewport)
    {
        if (_activeScene != ActiveScene.Options)
        {
            return;
        }

        _options.EndSliderDrag(point, viewport);
        ApplyChangedOptionsSetting(_options.ConsumeChangedSettingKey());
    }



    public void ScrollActiveScene(float deltaY, UiPoint point, VirtualViewport viewport) => ScrollActiveScene(0f, deltaY, point, viewport);



    public void ScrollActiveScene(float deltaX, float deltaY, UiPoint point, VirtualViewport viewport)
    {
        if (_activeScene == ActiveScene.Options)
        {
            _options.Scroll(deltaY, point, viewport);
        }
        else if (_activeScene == ActiveScene.BeatmapDownloader)
        {
            _beatmapDownloader.Scroll(deltaY, viewport);
        }
        else if (_activeScene == ActiveScene.SongSelect)
        {
            _songSelect.Scroll(deltaY, point, viewport);
        }
        else if (_activeScene == ActiveScene.ModSelect)
        {
            _modSelect.Scroll(deltaX, deltaY, point, viewport);
        }
    }



    public void ScrollActiveScene(float deltaY, VirtualViewport viewport)
    {
        if (_activeScene == ActiveScene.Options)
        {
            _options.Scroll(deltaY, viewport);
        }
        else if (_activeScene == ActiveScene.BeatmapDownloader)
        {
            _beatmapDownloader.Scroll(deltaY, viewport);
        }
        else if (_activeScene == ActiveScene.SongSelect)
        {
            _songSelect.Scroll(deltaY, viewport);
        }
        else if (_activeScene == ActiveScene.ModSelect)
        {
            _modSelect.Scroll(0f, deltaY, new UiPoint(VirtualViewport.AndroidReferenceLandscape.VirtualWidth / 2f, VirtualViewport.AndroidReferenceLandscape.VirtualHeight / 2f), viewport);
        }
    }



    public void HandleUiAction(UiAction action) => HandleUiAction(action, VirtualViewport.AndroidReferenceLandscape);


}
