using OsuDroid.Game.Compatibility.Online;
using OsuDroid.Game.UI.Elements;

namespace OsuDroid.Game;

public sealed partial class OsuDroidGameCore
{
    private void ApplyOnlinePanelSetting()
    {
        _onlineLoginCancellation?.Cancel();

        if (!_settingsStore.GetBool("stayOnline", false))
        {
            _onlineProfile = null;
            ApplyOnlinePanelState(null);
            return;
        }

        if (_onlineProfile is not null)
        {
            ApplyOnlinePanelState(CreateOnlinePanelState(_onlineProfile));
            return;
        }

        string username = _settingsStore.GetString("onlineUsername", string.Empty).Trim();
        string password = _settingsStore.GetString("onlinePassword", string.Empty);
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ApplyOnlinePanelState(
                OnlineProfilePanelState.Failed(
                    "Wrong name or password",
                    LoadOnlineAvatarSetting(),
                    ReceiveAnnouncementsSetting()
                )
            );
            return;
        }

        StartOnlineLogin(username, password);
    }

    private void StartOnlineLogin(string username, string password)
    {
        int generation = unchecked(++_onlineLoginGeneration);
        var cancellation = new CancellationTokenSource();
        _onlineLoginCancellation = cancellation;
        ApplyOnlinePanelState(
            OnlineProfilePanelState.LoggingIn(
                LoadOnlineAvatarSetting(),
                ReceiveAnnouncementsSetting()
            )
        );
        _ = RunOnlineLoginAsync(username, password, generation, cancellation.Token);
    }

    private async Task RunOnlineLoginAsync(
        string username,
        string password,
        int generation,
        CancellationToken cancellationToken
    )
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            if (cancellationToken.IsCancellationRequested || generation != _onlineLoginGeneration)
            {
                return;
            }

            ApplyOnlinePanelState(
                OnlineProfilePanelState.LoggingIn(
                    LoadOnlineAvatarSetting(),
                    ReceiveAnnouncementsSetting()
                )
            );

            try
            {
                OnlineLoginResult result = await _onlineLoginClient
                    .LoginAsync(username, password, cancellationToken)
                    .ConfigureAwait(false);
                if (
                    cancellationToken.IsCancellationRequested
                    || generation != _onlineLoginGeneration
                )
                {
                    return;
                }

                if (result.IsSuccess && result.Profile is not null)
                {
                    _onlineProfile = CreateOnlineProfileSnapshot(result.Profile);
                    ApplyOnlinePanelState(CreateOnlinePanelState(_onlineProfile));
                    return;
                }

                ApplyOnlinePanelState(
                    OnlineProfilePanelState.Failed(
                        string.IsNullOrWhiteSpace(result.FailureMessage)
                            ? "Unknown server error"
                            : result.FailureMessage,
                        LoadOnlineAvatarSetting(),
                        ReceiveAnnouncementsSetting()
                    )
                );
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch
            {
                if (attempt == 2)
                {
                    ApplyOnlinePanelState(
                        OnlineProfilePanelState.Failed(
                            "Cannot connect to server",
                            LoadOnlineAvatarSetting(),
                            ReceiveAnnouncementsSetting()
                        )
                    );
                    return;
                }

                ApplyOnlinePanelState(
                    OnlineProfilePanelState.Retrying(
                        LoadOnlineAvatarSetting(),
                        ReceiveAnnouncementsSetting()
                    )
                );
                await Task.Yield();
            }
        }
    }

    private void ApplyOnlinePanelState(OnlineProfilePanelState? state)
    {
        _mainMenu.SetOnlinePanelState(state);
        _songSelect.SetOnlinePanelState(state);
    }

    private bool LoadOnlineAvatarSetting() => _settingsStore.GetBool("loadAvatar", false);

    private bool ReceiveAnnouncementsSetting() =>
        _settingsStore.GetBool("receiveAnnouncements", true);

    private static OnlineProfileSnapshot CreateOnlineProfileSnapshot(LoginProfile profile) =>
        new(
            profile.Username,
            AvatarPath: string.IsNullOrWhiteSpace(profile.AvatarUrl) ? null : profile.AvatarUrl,
            Rank: ClampInt(profile.Rank),
            PerformancePoints: ClampInt((long)Math.Round(profile.PerformancePoints)),
            Accuracy: profile.Accuracy * 100f
        );

    private static int ClampInt(long value) =>
        value > int.MaxValue ? int.MaxValue
        : value < int.MinValue ? int.MinValue
        : (int)value;
}
