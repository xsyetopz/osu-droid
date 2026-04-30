using OsuDroid.Game.Compatibility.Online;

namespace OsuDroid.Game;

public sealed record OnlineLoginResult(
    bool IsSuccess,
    LoginProfile? Profile = null,
    string FailureMessage = ""
)
{
    public static OnlineLoginResult Success(LoginProfile profile) => new(true, profile);

    public static OnlineLoginResult Failure(string message) => new(false, FailureMessage: message);
}

public interface IOnlineLoginClient
{
    Task<OnlineLoginResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken
    );
}
