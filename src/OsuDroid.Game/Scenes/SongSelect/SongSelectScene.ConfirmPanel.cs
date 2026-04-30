using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Scenes.SongSelect;

public sealed partial class SongSelectScene
{
    private void AddConfirmPanel(
        List<UiElementSnapshot> elements,
        VirtualViewport viewport,
        string id,
        string title,
        string message,
        UiAction confirmAction,
        UiAction cancelAction
    )
    {
        elements.Add(
            Fill(
                id + "-shade",
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                DroidUiColors.ModalShadeLight,
                1f,
                cancelAction
            )
        );
        float width = 300f * Dp;
        float height = 150f * Dp;
        var panel = new UiRect(
            (viewport.VirtualWidth - width) / 2f,
            (viewport.VirtualHeight - height) / 2f,
            width,
            height
        );
        elements.Add(
            Fill(
                id + "-panel",
                panel,
                s_propertiesPanel,
                1f,
                UiAction.SongSelectPropertiesPanel,
                14f * Dp
            )
        );
        AddPropertiesRowText(
            elements,
            id + "-title",
            title,
            panel.X,
            panel.Y,
            panel.Width,
            44f * Dp,
            15f * Dp,
            s_white,
            UiAction.SongSelectPropertiesPanel
        );
        AddPropertiesRowText(
            elements,
            id + "-message",
            message,
            panel.X,
            panel.Y + 44f * Dp,
            panel.Width,
            44f * Dp,
            14f * Dp,
            s_propertiesSecondary,
            UiAction.SongSelectPropertiesPanel
        );
        AddFullWidthRow(
            elements,
            id + "-yes",
            _localizer["Common_Yes"],
            panel.X,
            panel.Y + 88f * Dp,
            panel.Width / 2f,
            confirmAction,
            s_propertiesDanger
        );
        AddFullWidthRow(
            elements,
            id + "-no",
            _localizer["Common_No"],
            panel.X + panel.Width / 2f,
            panel.Y + 88f * Dp,
            panel.Width / 2f,
            cancelAction,
            s_white
        );
    }
}
