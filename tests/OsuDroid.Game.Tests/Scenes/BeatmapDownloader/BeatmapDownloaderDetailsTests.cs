using System.Net;
using System.Reflection;
using OsuDroid.Game.Beatmaps.Import;
using OsuDroid.Game.Beatmaps.Online;
using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class BeatmapDownloaderTests
{

    [Test]
    public void DetailsPanelBlocksShadeCloseAndDifficultyDotsStayInteractive()
    {
        var scene = CreateScene();
        SetSets(scene, [CreateSet()]);
        scene.SelectCard(0);

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        var panel = frame.Elements.Single(element => element.Id == "downloader-details-panel");
        var selectedDifficulty = frame.Elements.Single(element => element.Id == "downloader-details-diff-0-selected");
        var selectedGlyph = frame.Elements.Single(element => element.Id == "downloader-details-diff-0");

        Assert.That(frame.HitTest(new UiPoint(panel.Bounds.X + 24, panel.Bounds.Y + 24))!.Action, Is.EqualTo(UiAction.DownloaderDetailsPanel));
        Assert.That(selectedDifficulty.Bounds.Width, Is.EqualTo(56f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(frame.HitTest(new UiPoint(selectedDifficulty.Bounds.X + selectedDifficulty.Bounds.Width / 2, selectedDifficulty.Bounds.Y + selectedDifficulty.Bounds.Height / 2))!.Action, Is.EqualTo(UiAction.DownloaderDetailsDifficulty0));
        Assert.That(selectedGlyph.Text, Is.EqualTo("⦿"));
        Assert.That(selectedGlyph.Kind, Is.EqualTo(UiElementKind.Text));
        Assert.That(frame.HitTest(new UiPoint(selectedGlyph.Bounds.X + selectedGlyph.Bounds.Width / 2, selectedGlyph.Bounds.Y + selectedGlyph.Bounds.Height / 2))!.Action, Is.EqualTo(UiAction.DownloaderDetailsDifficulty0));
    }
    [Test]
    public void DetailsDifficultyRowIsCenteredAndUsesAndroidPaddedSelectionCell()
    {
        var scene = CreateScene();
        SetSets(scene, [CreateSet()]);
        scene.SelectCard(0);

        var frame = scene.CreateSnapshot(VirtualViewport.LegacyLandscape).UiFrame;
        var panel = frame.Elements.Single(element => element.Id == "downloader-details-panel");
        var dots = frame.Elements.Where(element => element.Id.StartsWith("downloader-details-diff-", StringComparison.Ordinal) && !element.Id.EndsWith("-selected", StringComparison.Ordinal)).ToArray();
        var selected = frame.Elements.Single(element => element.Id == "downloader-details-diff-0-selected");
        var left = dots.Min(element => element.Bounds.X);
        var right = dots.Max(element => element.Bounds.Right);

        Assert.That(selected.Bounds.Width, Is.EqualTo(56f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(left + (right - left) / 2f, Is.EqualTo(panel.Bounds.X + panel.Bounds.Width / 2f).Within(1f));
    }
}
