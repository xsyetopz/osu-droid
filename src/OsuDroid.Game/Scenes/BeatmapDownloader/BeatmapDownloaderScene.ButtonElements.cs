using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private static void AddCompoundButton(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        UiAction action,
        UiMaterialIcon leadingIcon,
        UiMaterialIcon? trailingIcon,
        UiColor background,
        float radius
    )
    {
        elements.Add(Fill(id + "-hit", bounds, background, 1f, action, radius));
        float textWidth = EstimateTextWidth(text, 14f * Dp);
        float trailingWidth = trailingIcon is null ? 0f : 8f * Dp + 24f * Dp;
        float contentWidth = 24f * Dp + 8f * Dp + textWidth + trailingWidth;
        float x = bounds.X + (bounds.Width - contentWidth) / 2f;
        float iconY = bounds.Y + (bounds.Height - 24f * Dp) / 2f;
        elements.Add(
            MaterialIcon(
                id + "-icon",
                leadingIcon,
                new UiRect(x, iconY, 24f * Dp, 24f * Dp),
                s_white,
                1f,
                action
            )
        );
        elements.Add(
            TextMiddle(
                id + "-text",
                text,
                x + 32f * Dp,
                bounds.Y,
                textWidth,
                bounds.Height,
                14f * Dp,
                s_white,
                UiTextAlignment.Left,
                action
            )
        );
        if (trailingIcon is not null)
        {
            elements.Add(
                MaterialIcon(
                    id + "-trailing",
                    trailingIcon.Value,
                    new UiRect(x + 32f * Dp + textWidth + 8f * Dp, iconY, 24f * Dp, 24f * Dp),
                    s_white,
                    1f,
                    action
                )
            );
        }
    }

    private static void AddCompoundSpriteButton(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        string assetName,
        UiAction action,
        UiMaterialIcon trailingIcon
    )
    {
        elements.Add(Fill(id + "-hit", bounds, s_appBar, 0f, action, 0f));
        float textWidth = EstimateTextWidth(text, 14f * Dp);
        float contentWidth = 24f * Dp + 8f * Dp + textWidth + 8f * Dp + 24f * Dp;
        float x = bounds.X + (bounds.Width - contentWidth) / 2f;
        float iconY = bounds.Y + (bounds.Height - 24f * Dp) / 2f;
        elements.Add(
            new UiElementSnapshot(
                id + "-logo",
                UiElementKind.Sprite,
                new UiRect(x, iconY, 24f * Dp, 24f * Dp),
                s_white,
                1f,
                assetName,
                action
            )
        );
        elements.Add(
            TextMiddle(
                id + "-text",
                text,
                x + 32f * Dp,
                bounds.Y,
                textWidth,
                bounds.Height,
                14f * Dp,
                s_white,
                UiTextAlignment.Left,
                action
            )
        );
        elements.Add(
            MaterialIcon(
                id + "-caret",
                trailingIcon,
                new UiRect(x + 32f * Dp + textWidth + 8f * Dp, iconY, 24f * Dp, 24f * Dp),
                s_white,
                1f,
                action
            )
        );
    }

    private static void AddButton(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        UiAction action
    )
    {
        elements.Add(Fill(id + "-bg", bounds, s_field, 1f, action, Radius));
        elements.Add(
            TextMiddle(
                id + "-text",
                text,
                bounds.X + 16f * Dp,
                bounds.Y,
                bounds.Width - 32f * Dp,
                bounds.Height,
                MathF.Min(14f * Dp, bounds.Height * 0.45f),
                s_white,
                UiTextAlignment.Left,
                action
            )
        );
    }

    private static void AddButtonGroup(
        List<UiElementSnapshot> elements,
        List<DownloaderButtonSpec> buttons,
        float centerX,
        float y
    )
    {
        float totalWidth =
            buttons.Sum(button => button.Width) + Math.Max(0, buttons.Count - 1) * 8f * Dp;
        float x = centerX - totalWidth / 2f;
        foreach (DownloaderButtonSpec button in buttons)
        {
            AddIconButton(
                elements,
                button.Id,
                new UiRect(x, y, button.Width, 36f * Dp),
                button.Icon,
                button.Text,
                button.Action
            );
            x += button.Width + 8f * Dp;
        }
    }

    private static void AddIconButton(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        UiMaterialIcon icon,
        string text,
        UiAction action
    )
    {
        elements.Add(Fill(id + "-bg", bounds, s_field, 1f, action, Radius));
        float textWidth = string.IsNullOrEmpty(text)
            ? 0f
            : MathF.Max(48f * Dp, text.Length * 7f * Dp);
        float contentWidth = 24f * Dp + (textWidth > 0f ? 6f * Dp + textWidth : 0f);
        float iconX = bounds.X + (bounds.Width - contentWidth) / 2f;
        elements.Add(
            MaterialIcon(
                id + "-icon",
                icon,
                new UiRect(iconX, bounds.Y + 6f * Dp, 24f * Dp, 24f * Dp),
                s_white,
                1f,
                action
            )
        );
        if (textWidth > 0f)
        {
            elements.Add(
                TextMiddle(
                    id + "-text",
                    text,
                    iconX + 30f * Dp,
                    bounds.Y,
                    textWidth,
                    bounds.Height,
                    14f * Dp,
                    s_white,
                    UiTextAlignment.Left,
                    action
                )
            );
        }
    }
}
