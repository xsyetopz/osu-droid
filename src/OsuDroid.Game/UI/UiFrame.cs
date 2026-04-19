using OsuDroid.Game.Scenes;

namespace OsuDroid.Game.UI;

public enum UiElementKind
{
    Fill,
    Sprite,
    Text,
    Icon,
    MaterialIcon,
}

public enum UiAction
{
    None,
    MainMenuCookie,
    MainMenuFirst,
    MainMenuSecond,
    MainMenuThird,
    MainMenuVersionPill,
    MainMenuAboutClose,
    MainMenuAboutChangelog,
    MainMenuAboutOsuWebsite,
    MainMenuAboutOsuDroidWebsite,
    MainMenuAboutDiscord,
    MainMenuMusicPrevious,
    MainMenuMusicPlay,
    MainMenuMusicPause,
    MainMenuMusicStop,
    MainMenuMusicNext,
    OptionsBack,
    OptionsSectionGeneral,
    OptionsSectionGameplay,
    OptionsSectionGraphics,
    OptionsSectionAudio,
    OptionsSectionLibrary,
    OptionsSectionInput,
    OptionsSectionAdvanced,
    OptionsToggleServerConnection,
    OptionsToggleLoadAvatar,
    OptionsToggleAnnouncements,
    OptionsToggleMusicPreview,
    OptionsToggleShiftPitch,
    OptionsToggleBeatmapSounds,
}

public enum UiIcon
{
    BackArrow,
    Grid,
    Square,
    Display,
    Headphones,
    MusicLibrary,
    Input,
    Gear,
    Check,
    CheckboxChecked,
    CheckboxUnchecked,
    ChevronRight,
    ChevronDown,
}

public enum UiCornerMode
{
    None,
    All,
    Top,
    Bottom,
}

public enum UiMaterialIcon
{
    ArrowBack,
    ArrowDropDown,
    ChevronRight,
    Check,
    CheckboxBlankOutline,
    ViewGridOutline,
    GamepadVariantOutline,
    MonitorDashboard,
    Headphones,
    LibraryMusic,
    GestureTapButton,
    Cogs,
}

public enum UiTextAlignment
{
    Left,
    Center,
    Right,
}

public sealed record UiTextStyle(float Size, bool Bold = false, UiTextAlignment Alignment = UiTextAlignment.Left, bool Underline = false);

public sealed record UiElementSnapshot(
    string Id,
    UiElementKind Kind,
    UiRect Bounds,
    UiColor Color,
    float Alpha,
    string? AssetName = null,
    UiAction Action = UiAction.None,
    string? Text = null,
    UiTextStyle? TextStyle = null,
    bool IsEnabled = true,
    UiIcon? Icon = null,
    float CornerRadius = 0f,
    UiMaterialIcon? MaterialIcon = null,
    UiCornerMode CornerMode = UiCornerMode.All);

public sealed record UiFrameSnapshot(
    VirtualViewport Viewport,
    IReadOnlyList<UiElementSnapshot> Elements,
    UiAssetManifest AssetManifest)
{
    public UiElementSnapshot? HitTest(UiPoint point)
    {
        for (var index = Elements.Count - 1; index >= 0; index--)
        {
            var element = Elements[index];
            if (element.Action != UiAction.None && element.Bounds.Contains(point))
                return element;
        }

        return null;
    }
}

public sealed class UiSceneStack
{
    private readonly Stack<string> sceneNames = new();

    public UiSceneStack(string rootSceneName) => sceneNames.Push(rootSceneName);

    public string Current => sceneNames.Peek();

    public void Push(string sceneName) => sceneNames.Push(sceneName);

    public bool TryPop()
    {
        if (sceneNames.Count == 1)
            return false;

        _ = sceneNames.Pop();
        return true;
    }
}

public static class UiActionRouter
{
    public static MainMenuButtonSlot ToMainMenuSlot(UiAction action) => action switch
    {
        UiAction.MainMenuFirst => MainMenuButtonSlot.First,
        UiAction.MainMenuSecond => MainMenuButtonSlot.Second,
        UiAction.MainMenuThird => MainMenuButtonSlot.Third,
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
    };
}
