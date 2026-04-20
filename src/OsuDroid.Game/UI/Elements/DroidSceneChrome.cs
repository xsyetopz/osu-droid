namespace OsuDroid.Game.UI;

public static class DroidSceneChrome
{
    public static void AddAppBar(List<UiElementSnapshot> elements, string idPrefix, float width, UiColor background)
    {
        elements.Add(new UiElementSnapshot(
            idPrefix + "-appbar",
            UiElementKind.Fill,
            new UiRect(0f, 0f, width, DroidUiMetrics.AppBarHeight),
            background,
            1f));
    }

    public static void AddBackButton(List<UiElementSnapshot> elements, string idPrefix, UiAction action, UiColor background, UiColor iconColor)
    {
        elements.Add(new UiElementSnapshot(
            idPrefix + "-back-hit",
            UiElementKind.Fill,
            new UiRect(0f, 0f, DroidUiMetrics.AppBarHeight, DroidUiMetrics.AppBarHeight),
            background,
            0f,
            Action: action));
        elements.Add(new UiElementSnapshot(
            idPrefix + "-back",
            UiElementKind.MaterialIcon,
            new UiRect(
                16f * DroidUiMetrics.DpScale,
                16f * DroidUiMetrics.DpScale,
                DroidUiMetrics.SectionIconSize,
                DroidUiMetrics.SectionIconSize),
            iconColor,
            1f,
            Action: action,
            MaterialIcon: UiMaterialIcon.ArrowBack));
    }
}
