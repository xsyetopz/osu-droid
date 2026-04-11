using System.Threading;
using System.Threading.Tasks;

namespace OsuDroid.Game;

public interface IAccountService
{
    AccountProfile Current { get; }

    Task<AccountProfile> RefreshAsync(CancellationToken cancellationToken = default);
}
