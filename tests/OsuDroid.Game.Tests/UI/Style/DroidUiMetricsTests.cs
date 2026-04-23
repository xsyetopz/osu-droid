namespace OsuDroid.Game.Tests;

public sealed partial class OptionsSceneTests
{

    [Test]
    public void OptionsSceneUsesAndroidReferenceScale()
    {
        Assert.That(DroidUiMetrics.DpScale, Is.EqualTo(1.6410257f).Within(0.0001f));
        Assert.That(DroidUiMetrics.AppBarHeight, Is.EqualTo(56f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(DroidUiMetrics.ContentPaddingX, Is.EqualTo(32f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(DroidUiMetrics.SectionRailWidth, Is.EqualTo(200f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(DroidUiMetrics.RowTitleSize, Is.EqualTo(14f * DroidUiMetrics.DpScale).Within(0.001f));
        Assert.That(DroidUiMetrics.RowSummarySize, Is.EqualTo(12f * DroidUiMetrics.DpScale).Within(0.001f));
    }
}
