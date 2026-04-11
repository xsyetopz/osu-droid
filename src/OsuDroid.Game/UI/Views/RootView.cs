using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using OsuDroid.Game.UI.Controls;
using OsuDroid.Game.UI.Navigation;

namespace OsuDroid.Game.UI.Views;

public partial class RootView : CompositeDrawable
{
    private readonly IAudioService audioService;
    private readonly ISessionService sessionService;
    private readonly IAccountService accountService;
    private readonly IBeatmapLibraryService beatmapLibraryService;
    private readonly IExternalUriLauncher externalUriLauncher;

    private readonly Container content;
    private readonly Container overlayContainer;
    private readonly PillButton accountButton;

    private bool accountOverlayVisible;
    private AppRoute route = AppRoute.MainMenu;

    public RootView(
        IAudioService audioService,
        ISessionService sessionService,
        IAccountService accountService,
        IBeatmapLibraryService beatmapLibraryService,
        IExternalUriLauncher externalUriLauncher)
    {
        this.audioService = audioService;
        this.sessionService = sessionService;
        this.accountService = accountService;
        this.beatmapLibraryService = beatmapLibraryService;
        this.externalUriLauncher = externalUriLauncher;

        RelativeSizeAxes = Axes.Both;

        InternalChildren = new Drawable[]
        {
            content = new Container
            {
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Padding = new MarginPadding(24),
                Child = accountButton = new PillButton(audioService, sessionService.Current.DisplayName, new Color4(36, 44, 72, 240), toggleAccountOverlay)
            },
            overlayContainer = new Container
            {
                RelativeSizeAxes = Axes.Both
            }
        };

        updateRoute();
    }

    private void updateRoute()
    {
        content.Clear();

        Drawable view = route switch
        {
            AppRoute.SongSelect => new SongSelectView(beatmapLibraryService, externalUriLauncher, audioService, () =>
            {
                route = AppRoute.MainMenu;
                updateRoute();
            }),
            AppRoute.Browse => new BrowseView(beatmapLibraryService, externalUriLauncher, audioService, () =>
            {
                route = AppRoute.MainMenu;
                updateRoute();
            }),
            _ => new MainMenuView(audioService, () =>
            {
                route = AppRoute.SongSelect;
                updateRoute();
            }, () =>
            {
                route = AppRoute.Browse;
                updateRoute();
            })
        };

        view.Alpha = 0;
        content.Add(view);
        view.FadeIn(220, Easing.OutQuint);
        refreshSessionDisplay();
    }

    private void refreshSessionDisplay()
    {
        accountButton.Text = sessionService.Current.DisplayName;
    }

    private void toggleAccountOverlay()
    {
        if (accountOverlayVisible)
        {
            accountOverlayVisible = false;
            overlayContainer.Clear();
            return;
        }

        accountOverlayVisible = true;

        overlayContainer.Add(new Container
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
                    Child = new AccountOverlayView(sessionService, accountService, audioService, () =>
                    {
                        accountOverlayVisible = false;
                        overlayContainer.Clear();
                    }, refreshSessionDisplay)
                }
            }
        });
    }
}
