using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game.UI.Style;

public static class DroidUiColors
{
    public static readonly UiColor Black = UiColor.Opaque(0, 0, 0);
    public static readonly UiColor RootBackground = UiColor.Opaque(19, 19, 26);
    public static readonly UiColor AppBarBackground = UiColor.Opaque(30, 30, 46);
    public static readonly UiColor SelectedSection = UiColor.Opaque(54, 54, 83);
    public static readonly UiColor RowBackground = UiColor.Opaque(22, 22, 34);
    public static readonly UiColor InputBackground = UiColor.Opaque(54, 54, 83);
    public static readonly UiColor White = UiColor.Opaque(255, 255, 255);
    public static readonly UiColor SecondaryText = UiColor.Opaque(178, 178, 204);
    public static readonly UiColor DisabledWhite = UiColor.Opaque(235, 235, 245);
    public static readonly UiColor CheckboxAccent = UiColor.Opaque(243, 115, 115);
    public static readonly UiColor SliderTrack = UiColor.Opaque(54, 54, 83);
    public static readonly UiColor MutedText = UiColor.Opaque(130, 130, 168);
    public static readonly UiColor DangerText = UiColor.Opaque(255, 191, 191);
    public static readonly UiColor DarkText = UiColor.Opaque(32, 32, 46);
    public static readonly UiColor StarNeutral = UiColor.Opaque(170, 170, 170);
    public static readonly UiColor CoverFallback = UiColor.Opaque(75, 75, 128);
    public static readonly UiColor FilterPanel = UiColor.Opaque(33, 33, 51);
    public static readonly UiColor DropdownPanel = UiColor.Opaque(40, 40, 61);
    public static readonly UiColor OnlinePanel = UiColor.Opaque(51, 51, 51);
    public static readonly UiColor OnlineProfilePanel = new(51, 51, 51, 128);
    public static readonly UiColor OnlineProfileFooter = new(51, 51, 51, 204);
    public static readonly UiColor OnlineProfileSecondaryText = UiColor.Opaque(217, 217, 230);
    public static readonly UiColor SetRowTint = UiColor.Opaque(240, 150, 0);
    public static readonly UiColor DifficultyRowTint = UiColor.Opaque(25, 25, 240);

    public static readonly UiColor Surface = RootBackground;
    public static readonly UiColor SurfaceAppBar = AppBarBackground;
    public static readonly UiColor SurfaceSelected = SelectedSection;
    public static readonly UiColor SurfaceRow = RowBackground;
    public static readonly UiColor SurfaceInput = InputBackground;
    public static readonly UiColor TextPrimary = White;
    public static readonly UiColor TextSecondary = SecondaryText;
    public static readonly UiColor TextDisabled = DisabledWhite;
    public static readonly UiColor Accent = CheckboxAccent;
    public static readonly UiColor Track = SliderTrack;
    public static readonly UiColor DialogScrim = Black;
    public static readonly UiColor ModalShade = new(0, 0, 0, 128);
    public static readonly UiColor ModalShadeLight = new(0, 0, 0, 96);
    public static readonly UiColor SongSelectShade = new(0, 0, 0, 132);
    public static readonly UiColor DividerSubtle = new(255, 255, 255, 10);
    public static readonly UiColor SurfaceDivider = new(19, 19, 26, 115);
    public static readonly UiColor FilterLabel = new(255, 255, 255, 191);
    public static readonly UiColor DropdownSelected = new(242, 114, 114, 41);
    public static readonly UiColor ModalShadeStrong = new(19, 19, 26, 190);

    public static UiColor WithAlpha(UiColor color, byte alpha) =>
        new(color.Red, color.Green, color.Blue, alpha);
}
