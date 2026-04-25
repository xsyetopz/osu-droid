using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Assets;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;

namespace OsuDroid.Game.Scenes.MainMenu;

public sealed partial class MainMenuScene
{
    private void AddExitDialog(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        UiRect panel = CreateExitDialogPanelBounds(viewport);
        elements.Add(new UiElementSnapshot(
            "exit-dialog-scrim",
            UiElementKind.Fill,
            new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
            s_modalScrim,
            1f,
            Action: UiAction.MainMenuExitDialogPanel));

        elements.Add(new UiElementSnapshot(
            "exit-dialog-panel",
            UiElementKind.Fill,
            panel,
            s_modalPanel,
            1f,
            Action: UiAction.MainMenuExitDialogPanel,
            CornerRadius: ExitDialogPanelRadius));

        AddAboutText(elements, panel.X, panel.Y + 18f, "exit-dialog-title", _localizer["MainMenu_ExitDialogTitle"], panel.Width, 26f, true, s_white, UiTextAlignment.Center);
        AddAboutDivider(elements, panel.X, panel.Y + ExitDialogTitleBarHeight, panel.Width, 1f, "exit-dialog-title-divider");

        AddAboutText(
            elements,
            panel.X + ExitDialogContentInset,
            panel.Y + ExitDialogTitleBarHeight + 34f,
            "exit-dialog-message",
            _localizer["MainMenu_ExitDialogMessage"],
            panel.Width - ExitDialogContentInset * 2f,
            ExitDialogTextSize,
            false,
            s_white,
            UiTextAlignment.Center);

        float buttonWidth = (panel.Width - ExitDialogContentInset * 2f - ExitDialogButtonGap) / 2f;
        float buttonY = panel.Bottom - ExitDialogContentInset - ExitDialogButtonHeight;
        AddExitDialogButton(
            elements,
            "exit-dialog-confirm",
            new UiRect(panel.X + ExitDialogContentInset, buttonY, buttonWidth, ExitDialogButtonHeight),
            _localizer["MainMenu_ExitDialogConfirm"],
            UiAction.MainMenuExitConfirm,
            UiColor.Opaque(196, 205, 255),
            UiColor.Opaque(24, 24, 38));
        AddExitDialogButton(
            elements,
            "exit-dialog-cancel",
            new UiRect(panel.X + ExitDialogContentInset + buttonWidth + ExitDialogButtonGap, buttonY, buttonWidth, ExitDialogButtonHeight),
            _localizer["MainMenu_ExitDialogCancel"],
            UiAction.MainMenuExitCancel,
            UiColor.Opaque(58, 58, 88),
            s_white);
    }

    private static UiRect CreateExitDialogPanelBounds(VirtualViewport viewport)
    {
        float width = Math.Min(ExitDialogPanelWidth, viewport.VirtualWidth - 80f);
        float height = Math.Min(ExitDialogPanelHeight, viewport.VirtualHeight - 80f);
        return new UiRect(
            (viewport.VirtualWidth - width) / 2f,
            (viewport.VirtualHeight - height) / 2f,
            width,
            height);
    }

    private static void AddExitDialogButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action, UiColor fillColor, UiColor textColor)
    {
        elements.Add(new UiElementSnapshot(
            id,
            UiElementKind.Fill,
            bounds,
            fillColor,
            1f,
            Action: action,
            CornerRadius: 9f));
        elements.Add(new UiElementSnapshot(
            $"{id}-text",
            UiElementKind.Text,
            new UiRect(bounds.X, bounds.Y + 14f, bounds.Width, bounds.Height - 14f),
            textColor,
            1f,
            Text: text,
            TextStyle: new UiTextStyle(25f, true, UiTextAlignment.Center)));
    }

    private void AddExitOverlay(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        float progress = GetExitProgress();
        if (progress <= 0f)
        {
            return;
        }

        elements.Add(new UiElementSnapshot(
            "exit-blackout",
            UiElementKind.Fill,
            new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
            UiColor.Opaque(0, 0, 0),
            progress));

        if (progress < 1f)
        {
            return;
        }

        elements.Add(new UiElementSnapshot(
            "exit-instruction",
            UiElementKind.Text,
            new UiRect(0f, viewport.VirtualHeight * 0.5f - 18f, viewport.VirtualWidth, 40f),
            s_white,
            1f,
            Text: _localizer["MainMenu_ExitInstruction"],
            TextStyle: new UiTextStyle(ExitInstructionTextSize, Alignment: UiTextAlignment.Center)));
    }

    private void AddDevelopmentBuildOverlay(List<UiElementSnapshot> elements, VirtualViewport viewport)
    {
        if (!_isDevelopmentBuild)
        {
            return;
        }

        UiAssetEntry asset = DroidAssets.MainMenuManifest.Get(DroidAssets.DevBuildOverlay);
        float scale = viewport.VirtualWidth / asset.NativeSize.Width;
        float height = Math.Max(1f, asset.NativeSize.Height * scale);
        elements.Add(new UiElementSnapshot(
            "dev-build-overlay",
            UiElementKind.Sprite,
            new UiRect(0f, viewport.VirtualHeight - height, viewport.VirtualWidth, height),
            s_white,
            1f,
            DroidAssets.DevBuildOverlay));
        var textBounds = new UiRect(0f, viewport.VirtualHeight - height - 24f, viewport.VirtualWidth, 22f);
        elements.Add(new UiElementSnapshot(
            "dev-build-text-shadow",
            UiElementKind.Text,
            new UiRect(textBounds.X + 2f, textBounds.Y + 2f, textBounds.Width, textBounds.Height),
            UiColor.Opaque(0, 0, 0),
            0.5f,
            Text: _localizer["MainMenu_DevelopmentBuild"],
            TextStyle: new UiTextStyle(16f, Alignment: UiTextAlignment.Center)));
        elements.Add(new UiElementSnapshot(
            "dev-build-text",
            UiElementKind.Text,
            textBounds,
            UiColor.Opaque(255, 237, 0),
            1f,
            Text: _localizer["MainMenu_DevelopmentBuild"],
            TextStyle: new UiTextStyle(16f, Alignment: UiTextAlignment.Center)));
    }
}
