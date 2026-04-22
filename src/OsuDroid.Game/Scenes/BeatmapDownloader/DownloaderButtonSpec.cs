using OsuDroid.Game.UI;

namespace OsuDroid.Game.Scenes;

internal readonly record struct DownloaderButtonSpec(string Id, UiMaterialIcon Icon, string Text, float Width, UiAction Action);
