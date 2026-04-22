namespace OsuDroid.Game.UI;

public static partial class DroidUiMetrics
{
    public const float RowPadding = 18f * DpScale;
    public const float RowHeight = 64f * DpScale;
    public const float RowTitleSize = 14f * SpScale;
    public const float RowSummarySize = 12f * SpScale;
    public const float InputHeight = 34f * DpScale;
    public const float InputGap = 8f * DpScale;
    public const float AndroidRoundedRectRadius = 14f * DpScale;
    public const float AndroidSidebarRadius = 15f * DpScale;
    public const float ControlColumnWidth = 280f * DpScale;
    public const float ControlGap = 18f * DpScale;
    public const float InputRowHeight = RowPadding * 2f + RowTitleSize + 4f + 6f * DpScale + RowSummarySize + 4f + InputGap + InputHeight;
}
