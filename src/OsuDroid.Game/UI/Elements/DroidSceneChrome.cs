namespace OsuDroid.Game.UI.Elements;

public static class DroidSceneChrome
{
    public static void AddAppBar(List<UiElementSnapshot> elements, string idPrefix, float width, UiColor background)
    {
        elements.Add(UiElementFactory.Fill(
            idPrefix + "-appbar",
            new UiRect(0f, 0f, width, DroidUiMetrics.AppBarHeight),
            background));
    }

    public static void AddBackButton(List<UiElementSnapshot> elements, string idPrefix, UiAction action, UiColor background, UiColor iconColor)
    {
        elements.Add(UiElementFactory.Fill(
            idPrefix + "-back-hit",
            new UiRect(0f, 0f, DroidUiMetrics.AppBarHeight, DroidUiMetrics.AppBarHeight),
            background,
            0f,
            action));
        elements.Add(UiElementFactory.MaterialIcon(
            idPrefix + "-back",
            UiMaterialIcon.ArrowBack,
            new UiRect(
                16f * DroidUiMetrics.DpScale,
                16f * DroidUiMetrics.DpScale,
                DroidUiMetrics.SectionIconSize,
                DroidUiMetrics.SectionIconSize),
            iconColor,
            1f,
            action));
    }
}
