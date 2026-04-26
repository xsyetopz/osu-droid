using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.UI.Elements;

public sealed record UiElementSnapshot(
    string Id,
    UiElementKind Kind,
    UiRect Bounds,
    UiColor Color,
    float Alpha,
    string? AssetName = null,
    UiAction Action = UiAction.None,
    string? Text = null,
    UiTextStyle? TextStyle = null,
    bool IsEnabled = true,
    UiIcon? Icon = null,
    float CornerRadius = 0f,
    UiMaterialIcon? MaterialIcon = null,
    UiCornerMode CornerMode = UiCornerMode.All,
    float RotationDegrees = 0f,
    float RotationOriginX = 0.5f,
    float RotationOriginY = 0.5f,
    string? ExternalAssetPath = null,
    UiSpriteFit SpriteFit = UiSpriteFit.Stretch,
    bool ClipToBounds = false,
    UiRect? SpriteSource = null,
    UiMeasuredTextAnchor? MeasuredTextAnchor = null,
    UiProgressRing? ProgressRing = null,
    UiRect? ClipBounds = null
);

public sealed record UiMeasuredTextAnchor(
    string Text,
    UiTextStyle TextStyle,
    float RightX,
    float LeftPadding
);

public sealed record UiProgressRing(float StrokeWidth, float SweepDegrees);
