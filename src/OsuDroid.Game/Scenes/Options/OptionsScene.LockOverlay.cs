using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using static OsuDroid.Game.UI.Style.DroidUiMetrics;

namespace OsuDroid.Game.Scenes.Options;

public sealed partial class OptionsScene
{
    private static void AddLockOverlay(
        List<UiElementSnapshot> elements,
        SettingsRow row,
        int index,
        UiRect bounds
    )
    {
        if (!row.IsLocked)
        {
            return;
        }

        elements.Add(
            Fill(
                $"options-row-{index}-locked-overlay",
                bounds,
                s_rootBackground,
                0.22f,
                UiAction.None,
                row.IsBottom ? AndroidRoundedRectRadius : 0f,
                false,
                row.IsBottom ? UiCornerMode.Bottom : UiCornerMode.None
            )
        );

        float lockSize = 26f * DpScale;
        var lockBounds = new UiRect(
            bounds.X + (bounds.Width - lockSize) / 2f,
            bounds.Y + (bounds.Height - lockSize) / 2f,
            lockSize,
            lockSize
        );
        elements.Add(
            MaterialIcon(
                $"options-row-{index}-lock",
                UiMaterialIcon.Lock,
                lockBounds,
                s_disabledWhite,
                0.82f,
                UiAction.None,
                false
            )
        );
    }
}
