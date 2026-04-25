using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game.Scenes.MainMenu;

public sealed partial class MainMenuScene
{
    private void AddAboutDialog(List<UiElementSnapshot> elements, VirtualViewport viewport, string _displayVersion)
    {
        UiRect panel = CreateAboutPanelBounds(viewport);
        elements.Add(new UiElementSnapshot(
            "about-scrim",
            UiElementKind.Fill,
            new UiRect(0f, 0f, viewport.VirtualWidth, viewport.VirtualHeight),
            s_modalScrim,
            1f,
            Action: UiAction.MainMenuAboutClose));

        elements.Add(new UiElementSnapshot(
            "about-panel",
            UiElementKind.Fill,
            panel,
            s_modalPanel,
            1f,
            CornerRadius: AboutPanelRadius));

        float contentWidth = panel.Width;
        AddAboutText(elements, panel.X, panel.Y + 19f, "about-dialog-title", _localizer["MainMenu_AboutTitle"], contentWidth, 25f, false, s_white, UiTextAlignment.Center);
        AddAboutDivider(elements, panel.X, panel.Y + AboutTitleBarHeight, panel.Width, 1f, "about-title-divider");

        AddAboutText(elements, panel.X, panel.Y + AboutContentTop, "about-title", "osu!droid", contentWidth, 36f, true, s_white, UiTextAlignment.Center);
        AddAboutText(elements, panel.X, panel.Y + AboutContentTop + 68f, "about-version", _localizer.Format("MainMenu_AboutVersion", _displayVersion), contentWidth, 30f, true, s_white, UiTextAlignment.Center);
        AddAboutText(elements, panel.X, panel.Y + AboutContentTop + 138f, "about-made-by", _localizer["MainMenu_AboutMadeBy"], contentWidth, 26f, false, s_white, UiTextAlignment.Center);
        AddAboutText(elements, panel.X, panel.Y + AboutContentTop + 174f, "about-copyright", _localizer["MainMenu_AboutCopyright"], contentWidth, 26f, false, s_white, UiTextAlignment.Center);

        float firstLinkY = panel.Y + 335f;
        AddAboutText(elements, panel.X, firstLinkY, "about-osu-link", _localizer["MainMenu_AboutOsuWebsite"], contentWidth, 26f, false, s_modalLink, UiTextAlignment.Center, true, UiAction.MainMenuAboutOsuWebsite);
        AddAboutText(elements, panel.X, firstLinkY + AboutLinkGap, "about-droid-link", _localizer["MainMenu_AboutOsuDroidWebsite"], contentWidth, 26f, false, s_modalLink, UiTextAlignment.Center, true, UiAction.MainMenuAboutOsuDroidWebsite);
        AddAboutText(elements, panel.X, firstLinkY + AboutLinkGap * 2f, "about-discord-link", _localizer["MainMenu_AboutDiscord"], contentWidth, 26f, false, s_modalLink, UiTextAlignment.Center, true, UiAction.MainMenuAboutDiscord);

        float buttonY = panel.Bottom - AboutButtonRowHeight;
        AddAboutDivider(elements, panel.X, buttonY, panel.Width, 1f, "about-button-row-divider");
        AddAboutDivider(elements, panel.X + panel.Width / 2f, buttonY, 1f, AboutButtonRowHeight, "about-button-divider");
        AddAboutButton(elements, "about-changelog", new UiRect(panel.X, buttonY, panel.Width / 2f, AboutButtonRowHeight), _localizer["MainMenu_AboutChangelog"], UiAction.MainMenuAboutChangelog);
        AddAboutButton(elements, "about-close", new UiRect(panel.X + panel.Width / 2f, buttonY, panel.Width / 2f, AboutButtonRowHeight), _localizer["MainMenu_AboutClose"], UiAction.MainMenuAboutClose);
    }

    private static UiRect CreateAboutPanelBounds(VirtualViewport viewport)
    {
        float height = Math.Min(AboutPanelHeight, viewport.VirtualHeight - 64f);
        return new UiRect(
            (viewport.VirtualWidth - AboutPanelWidth) / 2f,
            (viewport.VirtualHeight - height) / 2f,
            AboutPanelWidth,
            height);
    }

    private static void AddAboutDivider(List<UiElementSnapshot> elements, float x, float y, float width, float height, string id)
    {
        elements.Add(new UiElementSnapshot(
            id,
            UiElementKind.Fill,
            new UiRect(x, y, width, height),
            s_modalDivider,
            1f));
    }

    private static void AddAboutText(
        List<UiElementSnapshot> elements,
        float x,
        float y,
        string id,
        string text,
        float width,
        float size,
        bool isBold,
        UiColor color,
        UiTextAlignment alignment,
        bool isUnderlined = false,
        UiAction action = UiAction.None)
    {
        elements.Add(new UiElementSnapshot(
            id,
            UiElementKind.Text,
            new UiRect(x, y, width, size + 12f),
            color,
            1f,
            Action: action,
            Text: text,
            TextStyle: new UiTextStyle(size, isBold, alignment, isUnderlined)));
    }

    private static void AddAboutButton(List<UiElementSnapshot> elements, string id, UiRect bounds, string text, UiAction action)
    {
        elements.Add(new UiElementSnapshot(
            id,
            UiElementKind.Fill,
            bounds,
            s_modalPanel,
            1f,
            Action: action,
            CornerMode: UiCornerMode.None));
        elements.Add(new UiElementSnapshot(
            $"{id}-text",
            UiElementKind.Text,
            new UiRect(bounds.X, bounds.Y + 34f, bounds.Width, bounds.Height - 34f),
            s_white,
            1f,
            Text: text,
            TextStyle: new UiTextStyle(28f, true, UiTextAlignment.Center)));
    }

}
