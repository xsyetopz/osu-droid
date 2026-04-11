using OsuDroid.Game.Resources;
using OsuDroid.Game.UI.Controls;
using OsuDroid.Game.UI.Navigation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace OsuDroid.Game.UI.Views;

public partial class RootView : CompositeDrawable
{
    private readonly IAudioService _audioService;
    private readonly ISessionService _sessionService;
    private readonly IAccountService _accountService;
    private readonly ILocalBeatmapLibraryService _localBeatmapLibraryService;
    private readonly IOnlineBeatmapBrowseService _onlineBeatmapBrowseService;
    private readonly IExternalUriLauncher _externalUriLauncher;
    private readonly IGameResources _resources;

    private readonly Container _content;
    private readonly Container _overlayContainer;
    private readonly PillButton _accountButton;

    private bool _accountOverlayVisible;
    private AppRoute _route = AppRoute.MainMenu;

    public RootView(
        IAudioService audioService,
        ISessionService sessionService,
        IAccountService accountService,
        ILocalBeatmapLibraryService localBeatmapLibraryService,
        IOnlineBeatmapBrowseService onlineBeatmapBrowseService,
        IExternalUriLauncher externalUriLauncher,
        IGameResources resources)
    {
        _audioService = audioService;
        _sessionService = sessionService;
        _accountService = accountService;
        _localBeatmapLibraryService = localBeatmapLibraryService;
        _onlineBeatmapBrowseService = onlineBeatmapBrowseService;
        _externalUriLauncher = externalUriLauncher;
        _resources = resources;

        RelativeSizeAxes = Axes.Both;

        InternalChildren = new Drawable[]
        {
            _content = new Container
            {
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Padding = new MarginPadding(24),
                Child = _accountButton = new PillButton(
                    _audioService,
                    _sessionService.Current.DisplayName,
                    new Color4(36, 44, 72, 240),
                    toggleAccountOverlay)
            },
            _overlayContainer = new Container
            {
                RelativeSizeAxes = Axes.Both
            }
        };

        updateRoute();
    }

    private void updateRoute()
    {
        _content.Clear();

        Drawable view = _route switch
        {
            AppRoute.SongSelect => new SongSelectView(_localBeatmapLibraryService, _externalUriLauncher, _audioService, _resources, () =>
            {
                _route = AppRoute.MainMenu;
                updateRoute();
            }),
            AppRoute.Browse => new BrowseView(_onlineBeatmapBrowseService, _externalUriLauncher, _audioService, _resources, () =>
            {
                _route = AppRoute.MainMenu;
                updateRoute();
            }),
            _ => new MainMenuView(_audioService, _resources, () =>
            {
                _route = AppRoute.SongSelect;
                updateRoute();
            }, () =>
            {
                _route = AppRoute.Browse;
                updateRoute();
            })
        };

        view.Alpha = 0;
        _content.Add(view);
        view.FadeIn(220, Easing.OutQuint);
        refreshSessionDisplay();
    }

    private void refreshSessionDisplay()
        => _accountButton.Text = _sessionService.Current.DisplayName;

    private void toggleAccountOverlay()
    {
        if (_accountOverlayVisible)
        {
            _accountOverlayVisible = false;
            _overlayContainer.Clear();
            return;
        }

        _accountOverlayVisible = true;

        _overlayContainer.Add(new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0, 0, 0, 110)
                },
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Padding = new MarginPadding { Top = 92, Right = 24 },
                    Child = new AccountOverlayView(_sessionService, _accountService, _audioService, () =>
                    {
                        _accountOverlayVisible = false;
                        _overlayContainer.Clear();
                    }, refreshSessionDisplay)
                }
            }
        });
    }
}
