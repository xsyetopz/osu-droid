namespace OsuDroid.Game.UI;

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
    string? ExternalAssetPath = null,
    UiSpriteFit SpriteFit = UiSpriteFit.Stretch);
