using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.ModSelect;

public sealed partial class ModSelectScene
{
    private UiFrameSnapshot CreateUiFrame(VirtualViewport viewport)
    {
        _lastViewport = viewport;
        ClampAllScrolls(viewport);
        var elements = new List<UiElementSnapshot>
        {
            Fill(
                "modselect-background",
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                s_panel,
                0.9f
            ),
        };

        AddTopBar(elements, viewport);
        AddSections(elements, viewport);
        AddBottomBar(elements, viewport);
        AddCustomizeDialog(elements, viewport);
        AddPresetDialog(elements, viewport);
        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }

    private UiFrameSnapshot CreateUiFrame(VirtualViewport viewport, UiFrameSnapshot parentFrame)
    {
        _lastViewport = viewport;
        ClampAllScrolls(viewport);
        var elements = new List<UiElementSnapshot>(parentFrame.Elements.Count + 96);
        elements.AddRange(parentFrame.Elements.Select(WithoutAction));
        elements.Add(
            Fill(
                "modselect-background",
                new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
                s_panel,
                0.9f
            )
        );
        AddTopBar(elements, viewport);
        AddSections(elements, viewport);
        AddBottomBar(elements, viewport);
        AddCustomizeDialog(elements, viewport);
        AddPresetDialog(elements, viewport);
        return new UiFrameSnapshot(viewport, elements, DroidAssets.MainMenuManifest);
    }
}
