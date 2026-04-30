using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Style;

namespace OsuDroid.Game.Scenes.BeatmapDownloader;

public sealed partial class BeatmapDownloaderScene
{
    private void AddStatusPill(
        List<UiElementSnapshot> elements,
        string id,
        BeatmapRankedStatus _status,
        float x,
        float y,
        UiAction action
    )
    {
        float width = StatusPillWidth(_status);
        var bounds = new UiRect(x, y, width, 20f * Dp);
        elements.Add(Fill(id + "-bg", bounds, StatusPillColor(), 1f, action, Radius));
        elements.Add(
            TextMiddle(
                id,
                RankedStatusText(_status),
                bounds.X + 12f * Dp,
                bounds.Y,
                bounds.Width - 24f * Dp,
                bounds.Height,
                10f * Dp,
                RankedStatusColor(_status),
                UiTextAlignment.Center,
                action
            )
        );
    }

    private float StatusPillWidth(BeatmapRankedStatus _status) =>
        Math.Max(58f * Dp, EstimateTextWidth(RankedStatusText(_status), 10f * Dp) + 24f * Dp);

    private static UiColor StatusPillColor() => s_panel;

    private static UiColor RankedStatusColor(BeatmapRankedStatus status) =>
        status switch
        {
            BeatmapRankedStatus.Ranked => DroidUiTheme.BeatmapStatus.Ranked,
            BeatmapRankedStatus.Approved => DroidUiTheme.BeatmapStatus.Ranked,
            BeatmapRankedStatus.Qualified => DroidUiTheme.BeatmapStatus.Qualified,
            BeatmapRankedStatus.Loved => DroidUiTheme.BeatmapStatus.Loved,
            BeatmapRankedStatus.Pending => DroidUiTheme.BeatmapStatus.Pending,
            BeatmapRankedStatus.WorkInProgress => DroidUiTheme.BeatmapStatus.Pending,
            BeatmapRankedStatus.Graveyard => DroidUiTheme.BeatmapStatus.Graveyard,
            _ => s_white,
        };
}
