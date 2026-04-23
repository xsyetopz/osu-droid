using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Runtime.Paths;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game.Tests;

public sealed partial class UiCompatibilityTests
{

    [Test]
    public void ViewportPreservesLegacyWidthAndDeviceAspect()
    {
        var viewport = VirtualViewport.FromSurface(2532, 1170);

        Assert.That(viewport.VirtualWidth, Is.EqualTo(1280f));
        Assert.That(viewport.VirtualHeight, Is.EqualTo(1170f / (2532f / 1280f)).Within(0.01f));
        Assert.That(viewport.ToVirtual(1266f, 585f).X, Is.EqualTo(640f).Within(0.01f));
    }
}
