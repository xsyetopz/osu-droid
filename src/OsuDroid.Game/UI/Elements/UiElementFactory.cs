namespace OsuDroid.Game.UI;

public static class UiElementFactory
{
    public static UiElementSnapshot Fill(
        string id,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None,
        float cornerRadius = 0f,
        bool isEnabled = true,
        UiCornerMode cornerMode = UiCornerMode.All,
        bool clipToBounds = false) => new(
            id,
            UiElementKind.Fill,
            bounds,
            color,
            alpha,
            Action: action,
            IsEnabled: isEnabled,
            CornerRadius: cornerRadius,
            CornerMode: cornerMode,
            ClipToBounds: clipToBounds);

    public static UiElementSnapshot Sprite(
        string id,
        string assetName,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None,
        bool isEnabled = true,
        float rotationDegrees = 0f,
        float rotationOriginX = 0.5f,
        float rotationOriginY = 0.5f,
        UiSpriteFit spriteFit = UiSpriteFit.Stretch,
        UiRect? spriteSource = null) => new(
            id,
            UiElementKind.Sprite,
            bounds,
            color,
            alpha,
            AssetName: assetName,
            Action: action,
            IsEnabled: isEnabled,
            RotationDegrees: rotationDegrees,
            RotationOriginX: rotationOriginX,
            RotationOriginY: rotationOriginY,
            SpriteFit: spriteFit,
            SpriteSource: spriteSource);

    public static UiElementSnapshot MaterialIcon(
        string id,
        UiMaterialIcon icon,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None,
        bool isEnabled = true) => new(
            id,
            UiElementKind.MaterialIcon,
            bounds,
            color,
            alpha,
            Action: action,
            IsEnabled: isEnabled,
            MaterialIcon: icon);

    public static UiElementSnapshot Icon(
        string id,
        UiIcon icon,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None,
        bool isEnabled = true) => new(
            id,
            UiElementKind.Icon,
            bounds,
            color,
            alpha,
            Action: action,
            IsEnabled: isEnabled,
            Icon: icon);

    public static UiElementSnapshot Text(
        string id,
        string text,
        UiRect bounds,
        float size,
        UiColor color,
        UiAction action = UiAction.None,
        bool isEnabled = true,
        bool bold = false,
        UiTextAlignment alignment = UiTextAlignment.Left,
        bool underline = false,
        UiTextVerticalAlignment verticalAlignment = UiTextVerticalAlignment.Top,
        float alpha = 1f) => new(
            id,
            UiElementKind.Text,
            bounds,
            color,
            alpha,
            Action: action,
            Text: text,
            TextStyle: new UiTextStyle(size, bold, alignment, underline, verticalAlignment),
            IsEnabled: isEnabled);
}
