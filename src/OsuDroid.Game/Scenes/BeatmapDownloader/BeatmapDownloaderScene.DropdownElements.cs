using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private static void AddDropdownOption(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        UiAction action,
        bool isSelected
    )
    {
        if (isSelected)
        {
            elements.Add(Fill(id + "-selected", bounds, s_dropdownSelected, 1f, action, Radius));
        }

        elements.Add(
            TextMiddle(
                id + "-text",
                text,
                bounds.X + 12f * Dp,
                bounds.Y,
                Math.Max(1f, bounds.Width - 48f * Dp),
                bounds.Height,
                14f * Dp,
                s_white,
                UiTextAlignment.Left,
                action
            )
        );
        if (isSelected)
        {
            elements.Add(
                MaterialIcon(
                    id + "-check",
                    UiMaterialIcon.Check,
                    new UiRect(bounds.Right - 34f * Dp, bounds.Y + 9f * Dp, 24f * Dp, 24f * Dp),
                    s_accent,
                    1f,
                    action
                )
            );
        }
    }

    private static void AddDropdownButton(
        List<UiElementSnapshot> elements,
        string id,
        UiRect bounds,
        string text,
        UiAction action
    )
    {
        elements.Add(Fill(id + "-bg", bounds, s_field, 1f, action, Radius));
        float textWidth = MathF.Min(
            bounds.Width - 56f * Dp,
            MathF.Max(24f * Dp, EstimateTextWidth(text, 14f * Dp))
        );
        elements.Add(
            TextMiddle(
                id + "-text",
                text,
                bounds.X + 16f * Dp,
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
                UiMaterialIcon.ArrowDropDown,
                new UiRect(bounds.Right - 34f * Dp, bounds.Y + 9f * Dp, 24f * Dp, 24f * Dp),
                s_white,
                1f,
                action
            )
        );
    }
}
