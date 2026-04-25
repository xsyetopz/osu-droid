using OsuDroid.Game.Scenes.Options;
using OsuDroid.Game.Scenes.Startup;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Frames;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    public GameFrameSnapshot CurrentFrame => CreateFrame(VirtualViewport.AndroidReferenceLandscape);



    public GameFrameSnapshot CreateFrame(VirtualViewport viewport) => _activeScene switch
    {
        ActiveScene.Startup => _startup.CreateSnapshot(viewport),
        ActiveScene.MainMenu => _mainMenu.CreateSnapshot(viewport),
        ActiveScene.Options => _options.CreateSnapshot(viewport),
        ActiveScene.BeatmapDownloader => _beatmapDownloader.CreateSnapshot(viewport),
        ActiveScene.BeatmapProcessing => BootstrapLoadingScene.CreateSnapshot(viewport, CreateBeatmapProcessingProgress(), TimeSpan.Zero),
        ActiveScene.SongSelect => _songSelect.CreateSnapshot(viewport),
        ActiveScene.ModSelect => _modSelect.CreateSnapshot(viewport, _songSelect.CreateSnapshot(viewport).UiFrame),
        _ => throw new InvalidOperationException($"Unknown scene: {_activeScene}"),
    };



    public IReadOnlyList<UiFrameSnapshot> CreateWarmupFrames(VirtualViewport viewport)
    {
        _songSelect.PrepareForWarmup();
        var frames = new List<UiFrameSnapshot>(OptionsScene.AllSections.Count + 3)
        {
            _mainMenu.CreateSnapshot(viewport).UiFrame,
            _mainMenu.CreateAboutDialogSnapshot(viewport).UiFrame,
            _songSelect.CreateSnapshot(viewport).UiFrame,
        };

        foreach (OptionsSection section in OptionsScene.AllSections)
        {
            frames.Add(_options.CreateSnapshotForSection(section, viewport).UiFrame);
        }

        return frames;
    }


}
