using OsuDroid.Game.Compatibility.Online;

namespace OsuDroid.Game;

public sealed class OsuDroidOnlineLoginClient(HttpClient? httpClient = null) : IOnlineLoginClient
{
    private readonly HttpClient _httpClient = httpClient ?? new HttpClient();

    public async Task<OnlineLoginResult> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken
    )
    {
        OnlineRequest request = OnlineProtocol.CreateLoginRequest(username, password);
        using HttpResponseMessage response = await _httpClient
            .PostAsync(request.Url, request.ToFormContent(), cancellationToken)
            .ConfigureAwait(false);
        string body = await response
            .Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);
        string[] lines = body.Split(["\r\n", "\n"], StringSplitOptions.None)
            .Where(line => line.Length > 0)
            .ToArray();
        OnlineResponse onlineResponse = OnlineResponseParser.ParseLines(lines);
        if (!onlineResponse.IsSuccess)
        {
            return OnlineLoginResult.Failure(
                onlineResponse.FailureMessage ?? "Unknown server error"
            );
        }

        try
        {
            return OnlineLoginResult.Success(
                OnlineResponseParser.ParseLoginProfile(onlineResponse.Lines)
            );
        }
        catch (FormatException)
        {
            return OnlineLoginResult.Failure("Invalid server response");
        }
    }
}
