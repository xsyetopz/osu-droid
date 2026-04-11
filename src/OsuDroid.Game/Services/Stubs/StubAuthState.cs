namespace OsuDroid.Game.Services.Stubs;

public sealed class StubAuthState
{
    public SessionSnapshot Session { get; set; } = SessionSnapshot.Guest();

    public AccountProfile Account { get; set; } = AccountProfile.Guest();
}
