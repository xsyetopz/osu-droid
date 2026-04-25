using NUnit.Framework;
using OsuDroid.Game.UI.Geometry;
using OsuDroid.Game.UI.Scrolling;
namespace OsuDroid.Game.Tests;

public sealed class KineticScrollStateTests
{
    [Test]
    public void DragBelowMinimumTravelDoesNotScroll()
    {
        var scroll = new KineticScrollState(KineticScrollAxis.Vertical);
        float offset = 0f;

        scroll.Begin(new UiPoint(0f, 100f), 0d);
        bool moved = scroll.Drag(new UiPoint(0f, 85f), 0.1d, () => offset, value => offset = value, 0f, 500f);

        Assert.That(moved, Is.False);
        Assert.That(offset, Is.Zero);
    }

    [Test]
    public void ReleasedDragKeepsMovingAndDecelerates()
    {
        var scroll = new KineticScrollState(KineticScrollAxis.Vertical);
        float offset = 0f;

        scroll.Begin(new UiPoint(0f, 100f), 0d);
        Assert.That(scroll.Drag(new UiPoint(0f, 40f), 0.05d, () => offset, value => offset = value, 0f, 500f), Is.True);
        float afterDrag = offset;
        float velocity = scroll.Velocity;
        scroll.End();

        scroll.Update(1f / 60f, () => offset, value => offset = value, 0f, 500f);

        Assert.That(offset, Is.GreaterThan(afterDrag));
        Assert.That(MathF.Abs(scroll.Velocity), Is.LessThan(MathF.Abs(velocity)));
    }

    [Test]
    public void ReleasePointCanStartFlingAfterSparseMoveEvents()
    {
        var scroll = new KineticScrollState(KineticScrollAxis.Vertical);
        float offset = 0f;

        scroll.Begin(new UiPoint(0f, 100f), 0d);
        Assert.That(scroll.Drag(new UiPoint(0f, 85f), 0.05d, () => offset, value => offset = value, 0f, 500f), Is.False);
        scroll.End(new UiPoint(0f, 35f), 0.08d, () => offset, value => offset = value, 0f, 500f);
        float afterRelease = offset;

        scroll.Update(1f / 60f, () => offset, value => offset = value, 0f, 500f);

        Assert.That(afterRelease, Is.GreaterThan(0f));
        Assert.That(offset, Is.GreaterThan(afterRelease));
    }

    [Test]
    public void InertiaClampsToBounds()
    {
        var scroll = new KineticScrollState(KineticScrollAxis.Vertical);
        float offset = 90f;

        scroll.Begin(new UiPoint(0f, 100f), 0d);
        scroll.Drag(new UiPoint(0f, 40f), 0.05d, () => offset, value => offset = value, 0f, 100f);
        scroll.End();

        scroll.Update(1f, () => offset, value => offset = value, 0f, 100f);

        Assert.That(offset, Is.EqualTo(100f));
        Assert.That(scroll.Velocity, Is.Zero);
    }

    [Test]
    public void DroidScrollableRegionKeepsOffsetAtMaximumAfterInertia()
    {
        var region = new DroidScrollableRegion(KineticScrollAxis.Vertical);

        region.ScrollBy(900f, 500f);
        bool updated = region.Update(1f / 60f, 500f);

        Assert.That(updated, Is.False);
        Assert.That(region.Offset, Is.EqualTo(500f));
    }
}
