using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
{
    private float AddLabeledBadge(
        List<UiElementSnapshot> elements,
        string id,
        string label,
        string value,
        float x,
        float y,
        ModStatDirection direction = ModStatDirection.Unchanged
    )
    {
        float labelWidth = TextWidth(label, 16f) + 24f;
        float valueWidth = TextWidth(value, 16f) + 24f;
        float width = labelWidth + valueWidth;
        elements.Add(
            Fill($"modselect-stat-{id}", new UiRect(x, y, width, 44f), s_badge, 1f, radius: 12f)
        );
        elements.Add(
            Fill(
                $"modselect-stat-{id}-label-bg",
                new UiRect(x, y, labelWidth, 44f),
                s_black,
                0.1f,
                radius: 12f
            )
        );
        elements.Add(
            Text(
                $"modselect-stat-{id}-label",
                label,
                new UiRect(x + 12f, y + 9f, labelWidth - 24f, 24f),
                16f,
                s_accent,
                alignment: UiTextAlignment.Center
            )
        );
        elements.Add(
            Text(
                $"modselect-stat-{id}-value",
                value,
                new UiRect(x + labelWidth + 12f, y + 9f, valueWidth - 24f, 24f),
                16f,
                StatColor(direction),
                alignment: UiTextAlignment.Center
            )
        );
        return x + width;
    }

    private static UiColor StatColor(ModStatDirection direction) =>
        direction switch
        {
            ModStatDirection.Increased => DroidUiTheme.ModMenu.StatIncreased,
            ModStatDirection.Decreased => DroidUiTheme.ModMenu.StatDecreased,
            ModStatDirection.DifficultyAdjust => DroidUiTheme.ModMenu.StatDifficultyAdjust,
            ModStatDirection.Unchanged => s_text,
            _ => s_text,
        };

    private static UiElementSnapshot WithoutAction(UiElementSnapshot element) =>
        element with
        {
            Action = UiAction.None,
        };

    private static float LabeledBadgeWidth(string label, string value) =>
        TextWidth(label, 16f) + TextWidth(value, 16f) + 48f;

    private static float BadgeWidth(string text) => TextWidth(text, 18f) + 24f;

    private static float TextWidth(string text, float size) =>
        MathF.Ceiling(text.Length * size * 0.55f);

    private static UiColor StarRatingColor(float rating) => OsuDroidColors.StarRatingBucket(rating);

    private static UiColor StarRatingTextColor(float rating) =>
        OsuDroidColors.StarRatingText(rating);

    private static UiRect SelectedModsBounds() => new(506f, 12f, 340f, 58f);

    private static UiRect SectionRailBounds(VirtualViewport viewport) =>
        new(
            0f,
            TopBarHeight,
            viewport.VirtualWidth,
            viewport.VirtualHeight - TopBarHeight - BottomBarHeight
        );

    private static UiRect ListClipBounds(UiRect sectionBounds) =>
        new(
            sectionBounds.X,
            sectionBounds.Y + SectionHeaderHeight,
            sectionBounds.Width,
            Math.Max(0f, sectionBounds.Height - SectionHeaderHeight - 12f)
        );

    private static bool IntersectsVertically(float y, float height, UiRect clipBounds) =>
        y < clipBounds.Bottom && y + height > clipBounds.Y;

    private static UiElementSnapshot Fill(
        string id,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None,
        float radius = 0f
    ) => UiElementFactory.Fill(id, bounds, color, alpha, action, radius);

    private static UiElementSnapshot MaterialIcon(
        string id,
        UiMaterialIcon icon,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None
    ) => UiElementFactory.MaterialIcon(id, icon, bounds, color, alpha, action);

    private static UiElementSnapshot Sprite(
        string id,
        string assetName,
        UiRect bounds,
        UiColor color,
        float alpha = 1f,
        UiAction action = UiAction.None
    ) => UiElementFactory.Sprite(id, assetName, bounds, color, alpha, action);

    private UiElementSnapshot Text(
        string id,
        string text,
        UiRect bounds,
        float size,
        UiColor color,
        UiAction action = UiAction.None,
        bool bold = false,
        UiTextAlignment alignment = UiTextAlignment.Left,
        float alpha = 1f,
        bool clipToBounds = false,
        bool autoScroll = false
    ) =>
        UiElementFactory.Text(
            id,
            text,
            bounds,
            size,
            color,
            action,
            bold: bold,
            alignment: alignment,
            verticalAlignment: UiTextVerticalAlignment.Middle,
            alpha: alpha,
            clipToBounds: clipToBounds,
            autoScroll: autoScroll ? new UiTextAutoScroll(_elapsedSeconds) : null
        );

    private void AddButton(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        UiAction action,
        bool isEnabled = true,
        UiMaterialIcon? leadingIcon = null,
        string? leadingAsset = null,
        UiColor? fillOverride = null,
        UiColor? textOverride = null,
        UiRect? clipBounds = null
    )
    {
        UiColor fill =
            fillOverride ?? (isEnabled ? s_button : DroidUiColors.WithAlpha(s_button, 120));
        UiColor color = textOverride ?? (isEnabled ? s_text : s_dimText);
        elements.Add(
            new UiElementSnapshot(
                id,
                UiElementKind.Fill,
                bounds,
                fill,
                1f,
                Action: isEnabled ? action : UiAction.None,
                IsEnabled: isEnabled,
                CornerRadius: 12f,
                ClipBounds: clipBounds
            )
        );

        float textX = bounds.X;
        float textWidth = bounds.Width;
        if (leadingAsset is not null)
        {
            elements.Add(
                Sprite(
                    $"{id}-icon",
                    leadingAsset,
                    new UiRect(bounds.X + 16f, bounds.Y + 15f, 28f, 28f),
                    color,
                    1f,
                    isEnabled ? action : UiAction.None
                ) with
                {
                    ClipBounds = clipBounds,
                }
            );
            textX += 28f;
            textWidth -= 34f;
        }
        else if (leadingIcon is not null)
        {
            elements.Add(
                MaterialIcon(
                    $"{id}-icon",
                    leadingIcon.Value,
                    new UiRect(bounds.X + 16f, bounds.Y + 15f, 28f, 28f),
                    color,
                    1f,
                    isEnabled ? action : UiAction.None
                ) with
                {
                    ClipBounds = clipBounds,
                }
            );
            textX += 28f;
            textWidth -= 34f;
        }

        elements.Add(
            Text(
                $"{id}-text",
                text,
                new UiRect(textX, bounds.Y, textWidth, bounds.Height),
                22f,
                color,
                isEnabled ? action : UiAction.None,
                true,
                UiTextAlignment.Center,
                clipToBounds: true
            ) with
            {
                ClipBounds = clipBounds,
            }
        );
    }
}
