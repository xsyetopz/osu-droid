using OsuDroid.Game.UI.Geometry;
namespace OsuDroid.Game.UI.Style;

public static class DroidUiTheme
{
    public static class Controls
    {
        public static readonly UiColor Button = DroidUiColors.SurfaceSelected;
        public static readonly UiColor Search = DroidUiColors.SurfaceInput;
        public static readonly UiColor Placeholder = DroidUiColors.TextSecondary;
        public static readonly UiColor Danger = DroidUiColors.DangerText;
        public static readonly UiColor DropdownSelected = DroidUiColors.DropdownSelected;
    }

    public static class BeatmapStatus
    {
        public static readonly UiColor Ranked = UiColor.Opaque(65, 255, 100);
        public static readonly UiColor Qualified = UiColor.Opaque(100, 242, 255);
        public static readonly UiColor Loved = UiColor.Opaque(250, 100, 255);
        public static readonly UiColor Pending = UiColor.Opaque(255, 172, 100);
        public static readonly UiColor Graveyard = DroidUiColors.TextPrimary;
    }

    public static class ModMenu
    {
        public static readonly UiColor Accent = new(194, 202, 255, 255);
        public static readonly UiColor Panel = Accent.MultiplyRgb(0.1f);
        public static readonly UiColor Badge = Accent.MultiplyRgb(0.15f);
        public static readonly UiColor Button = Accent.MultiplyRgb(0.175f);
        public static readonly UiColor Search = Accent.MultiplyRgb(0.25f);
        public static readonly UiColor SearchPlaceholder = Accent.MultiplyRgb(0.6f);
        public static readonly UiColor Selected = new(243, 115, 115, 255);
        public static readonly UiColor SelectedText = Accent.MultiplyRgb(0.1f);
        public static readonly UiColor ClearButton = new(52, 33, 33, 255);
        public static readonly UiColor Ranked = new(131, 223, 107, 255);
    }

    public static class Scroll
    {
        public const float IndicatorVisibleSeconds = 0.65f;
        public const float VelocityStop = 1f;
        public const float MaxVelocity = 3000f;
        public const float DecelerationPerFrame = 0.98f;
    }

    public static readonly UiColor TextPrimary = DroidUiColors.TextPrimary;
    public static readonly UiColor TextSecondary = DroidUiColors.TextSecondary;
    public static readonly UiColor Black = DroidUiColors.Black;

    private static UiColor MultiplyRgb(this UiColor color, float scalar) =>
        new(
            (byte)Math.Clamp((int)MathF.Round(color.Red * scalar), 0, 255),
            (byte)Math.Clamp((int)MathF.Round(color.Green * scalar), 0, 255),
            (byte)Math.Clamp((int)MathF.Round(color.Blue * scalar), 0, 255),
            color.Alpha);
}
