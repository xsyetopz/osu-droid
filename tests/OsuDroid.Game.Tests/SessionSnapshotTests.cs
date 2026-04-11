using OsuDroid.Game;

namespace OsuDroid.Game.Tests;

public class SessionSnapshotTests
{
    [Test]
    public void GuestFactoryProducesGuestState()
    {
        var snapshot = SessionSnapshot.Guest();

        Assert.Multiple(() =>
        {
            Assert.That(snapshot.IsGuest, Is.True);
            Assert.That(snapshot.IsSignedIn, Is.False);
            Assert.That(snapshot.DisplayName, Is.EqualTo("Guest"));
        });
    }
}
