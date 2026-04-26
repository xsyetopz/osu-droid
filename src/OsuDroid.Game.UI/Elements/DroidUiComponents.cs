using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.UI.Elements;

public static class DroidUiComponents
{
    public static UiElementSnapshot ModalScrim(
        string id,
        UiRect bounds,
        UiAction action = UiAction.None,
        UiColor? color = null
    ) => UiElementFactory.Fill(id, bounds, color ?? DroidUiColors.ModalShade, 1f, action);

    public static UiElementSnapshot Panel(
        string id,
        UiRect bounds,
        UiColor? color = null,
        float alpha = 1f,
        float radius = DroidUiMetrics.AndroidRoundedRectRadius,
        UiAction action = UiAction.None
    ) =>
        UiElementFactory.Fill(id, bounds, color ?? DroidUiColors.SurfaceRow, alpha, action, radius);

    public static void AddButton(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        UiAction action,
        UiColor? background = null,
        UiColor? foreground = null,
        float textSize = 18f,
        float radius = DroidUiMetrics.AndroidRoundedRectRadius,
        bool isEnabled = true
    )
    {
        UiColor resolvedForeground = foreground ?? DroidUiColors.TextPrimary;

        elements.Add(
            UiElementFactory.Fill(
                id + "-background",
                bounds,
                background ?? DroidUiColors.SurfaceSelected,
                1f,
                action,
                radius,
                isEnabled
            )
        );
        elements.Add(
            UiElementFactory.Text(
                id + "-text",
                text,
                bounds,
                textSize,
                resolvedForeground,
                action,
                isEnabled,
                bold: true,
                alignment: UiTextAlignment.Center,
                verticalAlignment: UiTextVerticalAlignment.Middle
            )
        );
    }

    public static void AddIconButton(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        UiMaterialIcon icon,
        string text,
        UiAction action,
        UiColor? background = null,
        UiColor? foreground = null,
        float iconSize = 24f,
        float textSize = 18f,
        float radius = DroidUiMetrics.AndroidRoundedRectRadius
    )
    {
        UiColor resolvedForeground = foreground ?? DroidUiColors.TextPrimary;
        elements.Add(
            UiElementFactory.Fill(
                id + "-background",
                bounds,
                background ?? DroidUiColors.SurfaceSelected,
                1f,
                action,
                radius
            )
        );

        float textWidth = string.IsNullOrEmpty(text)
            ? 0f
            : MathF.Max(1f, text.Length * textSize * 0.55f);
        float gap = textWidth > 0f ? 8f : 0f;
        float contentWidth = iconSize + gap + textWidth;
        float x = bounds.X + (bounds.Width - contentWidth) / 2f;
        var iconBounds = new UiRect(
            x,
            bounds.Y + (bounds.Height - iconSize) / 2f,
            iconSize,
            iconSize
        );
        elements.Add(
            UiElementFactory.MaterialIcon(
                id + "-icon",
                icon,
                iconBounds,
                resolvedForeground,
                1f,
                action
            )
        );

        if (textWidth > 0f)
        {
            elements.Add(
                UiElementFactory.Text(
                    id + "-text",
                    text,
                    new UiRect(iconBounds.Right + gap, bounds.Y, textWidth, bounds.Height),
                    textSize,
                    resolvedForeground,
                    action,
                    bold: true,
                    alignment: UiTextAlignment.Left,
                    verticalAlignment: UiTextVerticalAlignment.Middle
                )
            );
        }
    }

    public static void AddSearchField(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        UiAction action,
        UiColor? background = null,
        UiColor? foreground = null,
        UiColor? placeholder = null
    )
    {
        bool hasText = !string.IsNullOrWhiteSpace(text);
        UiColor textColor = hasText
            ? foreground ?? DroidUiColors.TextPrimary
            : placeholder ?? DroidUiColors.TextSecondary;

        elements.Add(
            UiElementFactory.Fill(
                id + "-background",
                bounds,
                background ?? DroidUiColors.SurfaceInput,
                1f,
                action,
                DroidUiMetrics.AndroidRoundedRectRadius,
                clipToBounds: true
            )
        );
        elements.Add(
            UiElementFactory.Text(
                id + "-text",
                hasText ? text : "Search...",
                new UiRect(bounds.X + 18f, bounds.Y, bounds.Width - 70f, bounds.Height),
                24f,
                textColor,
                action,
                verticalAlignment: UiTextVerticalAlignment.Middle,
                clipToBounds: true
            )
        );
        elements.Add(
            UiElementFactory.MaterialIcon(
                id + "-icon",
                UiMaterialIcon.Search,
                new UiRect(bounds.Right - 46f, bounds.Y + (bounds.Height - 34f) / 2f, 34f, 34f),
                placeholder ?? DroidUiColors.TextSecondary,
                1f,
                action
            )
        );
    }

    public static void AddDropdownOption(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        UiAction action,
        bool isSelected,
        UiColor? selectedBackground = null,
        UiColor? foreground = null
    )
    {
        if (isSelected)
        {
            elements.Add(
                UiElementFactory.Fill(
                    id + "-selected",
                    bounds,
                    selectedBackground ?? DroidUiColors.DropdownSelected,
                    1f,
                    action,
                    DroidUiMetrics.AndroidRoundedRectRadius
                )
            );
        }

        elements.Add(
            UiElementFactory.Text(
                id + "-text",
                text,
                new UiRect(
                    bounds.X + 12f,
                    bounds.Y,
                    Math.Max(1f, bounds.Width - 48f),
                    bounds.Height
                ),
                18f,
                foreground ?? DroidUiColors.TextPrimary,
                action,
                alignment: UiTextAlignment.Left,
                verticalAlignment: UiTextVerticalAlignment.Middle
            )
        );

        if (isSelected)
        {
            elements.Add(
                UiElementFactory.MaterialIcon(
                    id + "-check",
                    UiMaterialIcon.Check,
                    new UiRect(bounds.Right - 34f, bounds.Y + (bounds.Height - 24f) / 2f, 24f, 24f),
                    DroidUiColors.Accent,
                    1f,
                    action
                )
            );
        }
    }

    public static void AddBadge(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        UiColor background,
        UiColor foreground,
        UiAction action = UiAction.None
    )
    {
        elements.Add(
            UiElementFactory.Fill(
                id + "-background",
                bounds,
                background,
                1f,
                action,
                DroidUiMetrics.AndroidRoundedRectRadius
            )
        );
        elements.Add(
            UiElementFactory.Text(
                id + "-text",
                text,
                bounds,
                18f,
                foreground,
                action,
                bold: true,
                alignment: UiTextAlignment.Center,
                verticalAlignment: UiTextVerticalAlignment.Middle
            )
        );
    }

    public static void AddStatusPill(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        UiColor foreground,
        UiAction action = UiAction.None,
        UiColor? background = null
    )
    {
        elements.Add(
            UiElementFactory.Fill(
                id + "-background",
                bounds,
                background ?? DroidUiColors.SurfaceRow,
                1f,
                action,
                DroidUiMetrics.AndroidRoundedRectRadius
            )
        );
        elements.Add(
            UiElementFactory.Text(
                id + "-text",
                text,
                bounds,
                16f,
                foreground,
                action,
                alignment: UiTextAlignment.Center,
                verticalAlignment: UiTextVerticalAlignment.Middle
            )
        );
    }
}
