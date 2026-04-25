namespace OsuDroid.Game.UI.Style;

public static partial class DroidUiMetrics
{
    public const float SeekbarContainerMarginX = 18f * DpScale;
    public const float SeekbarTrackMarginX = 2f * DpScale;
    public const float SeekbarTopMargin = 16f * DpScale;
    public const float SeekbarTrackHeight = 6f * DpScale;
    public const float SeekbarThumbSize = 16f * DpScale;
    public const float SliderSummaryLineHeight = RowSummarySize + 4f;
    public const float SliderSummaryParagraphGap = 10f * DpScale;
    public const int LongSliderSummaryLineCount = 4;
    public const float SliderRowHeight = RowPadding * 2f + RowTitleSize + 4f + 6f * DpScale + SliderSummaryLineHeight + SeekbarTopMargin + SeekbarThumbSize;
    public const float LongSliderRowHeight = RowPadding * 2f + RowTitleSize + 4f + 6f * DpScale + LongSliderSummaryLineCount * SliderSummaryLineHeight + SliderSummaryParagraphGap + SeekbarTopMargin + SeekbarThumbSize;
}
