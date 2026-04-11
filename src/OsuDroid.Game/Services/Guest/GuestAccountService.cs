using System.Threading;
using System.Threading.Tasks;

namespace OsuDroid.Game.Services.Guest;

public sealed class GuestAccountService : IAccountService
{
    private readonly AccountProfile _current = AccountProfile.Guest();

    public AccountProfile Current => _current;

    public Task<AccountProfile> RefreshAsync(CancellationToken cancellationToken = default) => Task.FromResult(_current);
}
