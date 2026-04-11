using System.Threading;
using System.Threading.Tasks;

namespace OsuDroid.Game.Services.Guest;

public sealed class GuestSessionService : ISessionService
{
    private readonly SessionSnapshot _current = SessionSnapshot.Guest();

    public SessionSnapshot Current => _current;

    public Task<SessionSnapshot> RestoreAsync(CancellationToken cancellationToken = default) => Task.FromResult(_current);

    public Task<SessionSnapshot> SignInAsync(string username, string password, CancellationToken cancellationToken = default) =>
        Task.FromResult(_current);

    public void SignOut()
    {
    }
}
