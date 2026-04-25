using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
namespace OsuDroid.Game.Scenes.BeatmapDownloader;

internal readonly record struct DownloaderButtonSpec(string Id, UiMaterialIcon Icon, string Text, float Width, UiAction Action);
