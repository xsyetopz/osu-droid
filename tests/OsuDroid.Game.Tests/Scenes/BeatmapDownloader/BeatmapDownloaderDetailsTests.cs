namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{

    [Test]
    public void DetailsPanelBlocksShadeCloseAndDifficultyDotsStayInteractive()
    {
        BeatmapDownloaderScene scene = CreateScene();
        SetSets(scene, [CreateSet()]);
        scene.SelectCard(0);

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        UiElementSnapshot panel = frame.Elements.Single(element => element.Id == "downloader-details-panel");
        UiElementSnapshot selectedDifficulty = frame.Elements.Single(element => element.Id == "downloader-details-diff-0-selected");
        UiElementSnapshot selectedGlyph = frame.Elements.Single(element => element.Id == "downloader-details-diff-0");

        Assert.That(frame.HitTest(new UiPoint(panel.Bounds.X + 24, panel.Bounds.Y + 24))!.Action, Is.EqualTo(UiAction.DownloaderDetailsPanel));
        Assert.That(selectedDifficulty.Bounds.Width, Is.EqualTo(56f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(frame.HitTest(new UiPoint(selectedDifficulty.Bounds.X + selectedDifficulty.Bounds.Width / 2, selectedDifficulty.Bounds.Y + selectedDifficulty.Bounds.Height / 2))!.Action, Is.EqualTo(UiAction.DownloaderDetailsDifficulty0));
        Assert.That(selectedGlyph.Text, Is.EqualTo("⦿"));
        Assert.That(selectedGlyph.Kind, Is.EqualTo(UiElementKind.Text));
        Assert.That(frame.HitTest(new UiPoint(selectedGlyph.Bounds.X + selectedGlyph.Bounds.Width / 2, selectedGlyph.Bounds.Y + selectedGlyph.Bounds.Height / 2))!.Action, Is.EqualTo(UiAction.DownloaderDetailsDifficulty0));
    }

    [Test]
    public void DetailsPanelUsesLegacyWrapContentConstraints()
    {
        BeatmapDownloaderScene scene = CreateScene();
        SetSets(scene, [CreateSet()]);
        scene.SelectCard(0);

        VirtualViewport viewport = VirtualViewport.LegacyLandscape;
        UiFrameSnapshot frame = scene.CreateSnapshot(viewport).UiFrame;
        UiElementSnapshot panel = frame.Elements.Single(element => element.Id == "downloader-details-panel");
        UiElementSnapshot body = frame.Elements.Single(element => element.Id == "downloader-details-body");
        UiElementSnapshot download = frame.Elements.Single(element => element.Id == "downloader-details-download-bg");

        Assert.That(panel.Bounds.Width, Is.EqualTo(500f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(panel.Bounds.Height, Is.EqualTo(306f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(panel.Bounds.X + panel.Bounds.Width / 2f, Is.EqualTo(viewport.VirtualWidth / 2f).Within(0.001f));
        Assert.That(panel.Bounds.Y + panel.Bounds.Height / 2f, Is.EqualTo(viewport.VirtualHeight / 2f).Within(0.001f));
        Assert.That(panel.Bounds.Bottom, Is.LessThanOrEqualTo(viewport.VirtualHeight - 8f * DroidUiMetrics.DpScale + 0.001f));
        Assert.That(body.Bounds.Bottom, Is.EqualTo(panel.Bounds.Bottom).Within(0.001f));
        Assert.That(download.Bounds.Bottom, Is.LessThanOrEqualTo(panel.Bounds.Bottom - 8f * DroidUiMetrics.DpScale));
    }

    [Test]
    public void DetailsDifficultyRowIsCenteredAndUsesAndroidPaddedSelectionCell()
    {
        BeatmapDownloaderScene scene = CreateScene();
        SetSets(scene, [CreateSet()]);
        scene.SelectCard(0);

        UiFrameSnapshot frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        UiElementSnapshot panel = frame.Elements.Single(element => element.Id == "downloader-details-panel");
        UiElementSnapshot[] dots = frame.Elements.Where(element => element.Id.StartsWith("downloader-details-diff-", StringComparison.Ordinal) && !element.Id.EndsWith("-selected", StringComparison.Ordinal)).ToArray();
        UiElementSnapshot selected = frame.Elements.Single(element => element.Id == "downloader-details-diff-0-selected");
        float left = dots.Min(element => element.Bounds.X);
        float right = dots.Max(element => element.Bounds.Right);

        Assert.That(selected.Bounds.Width, Is.EqualTo(56f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(left + (right - left) / 2f, Is.EqualTo(panel.Bounds.X + panel.Bounds.Width / 2f).Within(1f));
    }
}
