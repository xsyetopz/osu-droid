using OsuDroid.Game.Scenes;

namespace OsuDroid.Game.UI;

public static class DroidUiMetrics
{
    public const float ReferencePixelWidth = 2340f;
    public const float ReferenceDensity = 3f;
    public const float DpScale = VirtualViewport.LegacyWidth / (ReferencePixelWidth / ReferenceDensity);
    public const float SpScale = DpScale;

    public const float AppBarHeight = 56f * DpScale;
    public const float ContentPaddingX = 32f * DpScale;
    public const float ContentTop = AppBarHeight + 32f * DpScale;
    public const float SectionRailWidth = 200f * DpScale;
    public const float SectionHeight = 48f * DpScale;
    public const float SectionIconSize = 24f * DpScale;
    public const float SectionPadding = 12f * DpScale;
    public const float SectionDrawablePadding = 12f * DpScale;
    public const float SectionSelectedRadius = 15f * DpScale;
    public const float ListGap = 32f * DpScale;
    public const float CategoryTopMargin = 12f * DpScale;
    public const float CategoryHeaderHeight = 48f * DpScale;
    public const float RowPadding = 18f * DpScale;
    public const float RowHeight = 64f * DpScale;
    public const float RowTitleSize = 14f * SpScale;
    public const float RowSummarySize = 12f * SpScale;
    public const float InputHeight = 34f * DpScale;
    public const float InputGap = 8f * DpScale;
    public const float AndroidRoundedRectRadius = 14f * DpScale;
    public const float AndroidSidebarRadius = 15f * DpScale;
    public const float SeekbarContainerMarginX = 18f * DpScale;
    public const float SeekbarTrackMarginX = 2f * DpScale;
    public const float SeekbarTopMargin = 16f * DpScale;
    public const float SeekbarTrackHeight = 6f * DpScale;
    public const float SeekbarThumbSize = 16f * DpScale;
    public const float ControlColumnWidth = 280f * DpScale;
    public const float ControlGap = 18f * DpScale;
    public const float InputRowHeight = RowPadding * 2f + RowTitleSize + 4f + 6f * DpScale + RowSummarySize + 4f + InputGap + InputHeight;
    public const float SliderRowHeight = RowPadding * 2f + RowTitleSize + 4f + 6f * DpScale + RowSummarySize + 4f + SeekbarTopMargin + SeekbarThumbSize;
}

public static class DroidUiColors
{
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
}

public sealed record DroidUiStyle(UiColor Background, UiColor Foreground, float CornerRadius, float Padding);
