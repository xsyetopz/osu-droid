using System.Threading;
using System.Threading.Tasks;

namespace OsuDroid.Game.Services.Stubs;

public sealed class StubAccountService(StubAuthState state) : IAccountService
{
    public AccountProfile Current => state.Account;

    public Task<AccountProfile> RefreshAsync(CancellationToken cancellationToken = default) => Task.FromResult(state.Account);
}
