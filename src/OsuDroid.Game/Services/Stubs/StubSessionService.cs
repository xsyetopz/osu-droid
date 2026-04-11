using System.Threading;
using System.Threading.Tasks;
using OsuDroid.Game.Localisation;

namespace OsuDroid.Game.Services.Stubs;

public sealed class StubSessionService(StubAuthState state) : ISessionService
{
    public SessionSnapshot Current => state.Session;

    public Task<SessionSnapshot> RestoreAsync(CancellationToken cancellationToken = default) => Task.FromResult(state.Session);

    public Task<SessionSnapshot> SignInAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var trimmed = string.IsNullOrWhiteSpace(username) ? "Guest" : username.Trim();

        state.Session = new SessionSnapshot(false, true, trimmed, trimmed);
        state.Account = new AccountProfile(trimmed, null, LoginPanelStrings.SignedIn.ToString());
        return Task.FromResult(state.Session);
    }

    public void SignOut()
    {
        state.Session = SessionSnapshot.Guest();
        state.Account = AccountProfile.Guest();
    }
}
