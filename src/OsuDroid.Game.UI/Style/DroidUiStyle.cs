using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.UI.Style;

public sealed record DroidUiStyle(
    UiColor Background,
    UiColor Foreground,
    float CornerRadius,
    float Padding
);
