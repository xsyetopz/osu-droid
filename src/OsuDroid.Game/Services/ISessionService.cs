using System.Threading;
using System.Threading.Tasks;

namespace OsuDroid.Game;

public interface ISessionService
{
    SessionSnapshot Current { get; }

    Task<SessionSnapshot> RestoreAsync(CancellationToken cancellationToken = default);

    Task<SessionSnapshot> SignInAsync(string username, string password, CancellationToken cancellationToken = default);

    void SignOut();
}
