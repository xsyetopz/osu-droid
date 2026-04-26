using OsuDroid.Game.Runtime.Audio;
using OsuDroid.Game.UI.Actions;
using OsuDroid.Game.UI.Elements;

namespace OsuDroid.Game.Scenes.MainMenu;

public sealed partial class MainMenuScene
{
    public void SetNowPlaying(MenuNowPlayingState state)
    {
        _nowPlaying = state;
        _currentBeatMilliseconds =
            state.IsPlaying && state.Bpm > 0.01f
                ? Math.Clamp(60000d / state.Bpm, 260d, 2000d)
                : LogoBeatMilliseconds;
    }

    public void SetOnlinePanelState(OnlineProfilePanelState? state) => _onlinePanelState = state;

    public void SetSpectrum(float[] spectrum1024, bool available)
    {
        if (!available || spectrum1024.Length < _rawSpectrum.Length)
        {
            _hasRawSpectrum = false;
            return;
        }

        Array.Copy(spectrum1024, _rawSpectrum, _rawSpectrum.Length);
        _hasRawSpectrum = true;
    }

    public MainMenuRoute Handle(MainMenuAction action)
    {
        if (_isAboutDialogOpen || _isExitDialogOpen || IsExitAnimating)
        {
            return MainMenuRoute.None;
        }

        if (!IsMenuShown && action == MainMenuAction.Activate)
        {
            BeginExpand();
            return MainMenuRoute.None;
        }

        switch (action)
        {
            case MainMenuAction.MoveUp:
                _selectedIndex =
                    (_selectedIndex + CurrentEntries.Length - 1) % CurrentEntries.Length;
                return MainMenuRoute.None;

            case MainMenuAction.MoveDown:
                _selectedIndex = (_selectedIndex + 1) % CurrentEntries.Length;
                return MainMenuRoute.None;

            case MainMenuAction.Back:
                ShowFirstMenu();
                return MainMenuRoute.None;

            case MainMenuAction.Activate:
                return ActivateSelected();

            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public MainMenuRoute Tap(MainMenuButtonSlot slot)
    {
        if (
            !IsMenuShown
            || _isAboutDialogOpen
            || _isExitDialogOpen
            || IsExitAnimating
            || _menuVisibility == MenuVisibility.Collapsing
        )
        {
            return MainMenuRoute.None;
        }

        _shownMilliseconds = 0d;
        _selectedIndex = slot switch
        {
            MainMenuButtonSlot.First => 0,
            MainMenuButtonSlot.Second => 1,
            MainMenuButtonSlot.Third => 2,
            _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null),
        };

        return ActivateSelected();
    }

    public void ToggleCookie()
    {
        if (_isAboutDialogOpen || _isExitDialogOpen || IsExitAnimating)
        {
            return;
        }

        if (_menuVisibility is MenuVisibility.Collapsed or MenuVisibility.Collapsing)
        {
            BeginExpand();
        }
        else
        {
            BeginCollapse();
        }
    }

    public void OpenAboutDialog()
    {
        if (!_isExitDialogOpen && !IsExitAnimating)
        {
            _isAboutDialogOpen = true;
        }
    }

    public void CloseAboutDialog() => _isAboutDialogOpen = false;

    public void ConfirmExitDialog()
    {
        if (!_isExitDialogOpen)
        {
            return;
        }

        _isExitDialogOpen = false;
        _ = BeginExitAnimation();
    }

    public void CancelExitDialog()
    {
        _isExitDialogOpen = false;
        _pressedAction = UiAction.None;
    }

    public void StartReturnTransition(string? backgroundPath = null)
    {
        _isReturnTransitionActive = true;
        _returnTransitionMilliseconds = 0d;
        _returnTransitionBackgroundPath = backgroundPath;
    }

    public void Press(UiAction action)
    {
        if (_isAboutDialogOpen || _isExitDialogOpen || IsExitAnimating)
        {
            return;
        }

        _pressedAction = action;
    }

    public void ReleasePress() => _pressedAction = UiAction.None;

    public MainMenuRoute ConsumePendingRoute()
    {
        if (!_hasPendingExitRoute)
        {
            return MainMenuRoute.None;
        }

        _hasPendingExitRoute = false;
        return MainMenuRoute.Exit;
    }

    public void Update(TimeSpan elapsed)
    {
        double elapsedMilliseconds = Math.Max(0d, elapsed.TotalMilliseconds);
        if (_nowPlaying.IsPlaying)
        {
            _beatMilliseconds += elapsedMilliseconds;
            if (_beatMilliseconds >= _currentBeatMilliseconds)
            {
                _beatMilliseconds %= _currentBeatMilliseconds;
                _heartbeatMilliseconds = 0d;
            }

            if (_heartbeatMilliseconds >= 0d)
            {
                _heartbeatMilliseconds += elapsedMilliseconds;
                if (_heartbeatMilliseconds > _currentBeatMilliseconds * 0.97d)
                {
                    _heartbeatMilliseconds = -1d;
                }
            }
        }
        else
        {
            _beatMilliseconds += elapsedMilliseconds;
            if (_beatMilliseconds >= LogoBeatMilliseconds)
            {
                _beatMilliseconds %= LogoBeatMilliseconds;
                _heartbeatMilliseconds = 0d;
            }

            if (_heartbeatMilliseconds >= 0d)
            {
                _heartbeatMilliseconds += elapsedMilliseconds;
                if (_heartbeatMilliseconds > LogoBeatMilliseconds * 0.97d)
                {
                    _heartbeatMilliseconds = -1d;
                }
            }
        }

        UpdateSpectrumState();

        if (_isReturnTransitionActive)
        {
            _returnTransitionMilliseconds += elapsedMilliseconds;
            if (_returnTransitionMilliseconds >= ReturnBackgroundFadeDurationMilliseconds)
            {
                _returnTransitionMilliseconds = ReturnBackgroundFadeDurationMilliseconds;
                _isReturnTransitionActive = false;
                _returnTransitionBackgroundPath = null;
            }
        }

        if (_isAboutDialogOpen || _isExitDialogOpen)
        {
            return;
        }

        switch (_menuVisibility)
        {
            case MenuVisibility.Expanding:
                _transitionMilliseconds += elapsedMilliseconds;
                _shownMilliseconds += elapsedMilliseconds;
                if (_transitionMilliseconds >= MenuExpandDurationMilliseconds)
                {
                    _transitionMilliseconds = MenuExpandDurationMilliseconds;
                    _menuVisibility = MenuVisibility.Expanded;
                }

                break;

            case MenuVisibility.Expanded:
                _shownMilliseconds += elapsedMilliseconds;
                break;

            case MenuVisibility.Collapsing:
                _transitionMilliseconds += elapsedMilliseconds;
                if (_transitionMilliseconds >= MenuCollapseDurationMilliseconds)
                {
                    _transitionMilliseconds = 0d;
                    _shownMilliseconds = 0d;
                    _menuVisibility = MenuVisibility.Collapsed;
                }

                break;

            case MenuVisibility.Exiting:
                _exitMilliseconds += elapsedMilliseconds;
                if (_exitMilliseconds >= ExitAnimationMilliseconds)
                {
                    _exitMilliseconds = ExitAnimationMilliseconds;
                    if (!_exitRoutePublished)
                    {
                        _hasPendingExitRoute = true;
                        _exitRoutePublished = true;
                    }
                }

                break;
            case MenuVisibility.Collapsed:
                break;
            default:
                break;
        }

        if (
            (_menuVisibility is MenuVisibility.Expanding or MenuVisibility.Expanded)
            && _shownMilliseconds > MenuIdleCollapseMilliseconds
        )
        {
            BeginCollapse();
        }
    }
}
