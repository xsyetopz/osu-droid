using System;
using System.Threading.Tasks;
using OsuDroid.Game.Localisation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace OsuDroid.Game.UI;

public partial class AccountOverlayView : CompositeDrawable
{
    private readonly ISessionService sessionService;
    private readonly IAccountService accountService;
    private readonly IAudioService audioService;
    private readonly Action onClose;
    private readonly Action onSessionChanged;

    private FillFlowContainer content = null!;
    private PanelTextBox usernameBox = null!;
    private PanelTextBox passwordBox = null!;
    private SpriteText statusText = null!;

    public AccountOverlayView(
        ISessionService sessionService,
        IAccountService accountService,
        IAudioService audioService,
        Action onClose,
        Action onSessionChanged)
    {
        this.sessionService = sessionService;
        this.accountService = accountService;
        this.audioService = audioService;
        this.onClose = onClose;
        this.onSessionChanged = onSessionChanged;

        AutoSizeAxes = Axes.Both;

        InternalChild = new Container
        {
            AutoSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 28,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(14, 18, 30, 248)
                },
                content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Width = 360,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(24),
                    Spacing = new Vector2(0, 18)
                }
            }
        };

        rebuild();
    }

    private void rebuild()
    {
        content.Clear();

        var session = sessionService.Current;
        var profile = accountService.Current;

        content.Add(new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 6),
            Children = new Drawable[]
            {
                new SpriteText
                {
                    Text = LoginPanelStrings.Account,
                    Font = FontUsage.Default.With(size: 18, weight: "Bold"),
                    Colour = new Color4(164, 178, 255, 255)
                },
                new SpriteText
                {
                    Text = session.DisplayName,
                    Font = FontUsage.Default.With(size: 34, weight: "Bold")
                },
                statusText = new SpriteText
                {
                    Text = session.IsSignedIn ? profile.StatusText : LoginPanelStrings.AppearOffline,
                    Font = FontUsage.Default.With(size: 18),
                    Colour = new Color4(196, 203, 227, 255)
                }
            }
        });

        if (session.IsSignedIn)
        {
            content.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 12),
                Children = new Drawable[]
                {
                    createActionButton(LoginPanelStrings.SignOut.ToString(), new Color4(196, 74, 102, 255), () =>
                    {
                        sessionService.SignOut();
                        onSessionChanged();
                        onClose();
                    }),
                    createActionButton(LoginPanelStrings.DoNotDisturb.ToString(), new Color4(76, 89, 130, 255), onClose)
                }
            });

            return;
        }

        content.Add(usernameBox = new PanelTextBox(UsersStrings.LoginUsername.ToString()));
        content.Add(passwordBox = new PanelTextBox(UsersStrings.LoginPassword.ToString(), secret: true));

        content.Add(new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 8),
            Children = new Drawable[]
            {
                new SpriteText
                {
                    Text = LoginPanelStrings.RememberUsername,
                    Font = FontUsage.Default.With(size: 16),
                    Colour = new Color4(174, 182, 207, 255)
                },
                new SpriteText
                {
                    Text = LoginPanelStrings.StaySignedIn,
                    Font = FontUsage.Default.With(size: 16),
                    Colour = new Color4(174, 182, 207, 255)
                }
            }
        });

        content.Add(createActionButton(UsersStrings.LoginButton.ToString(), new Color4(226, 113, 165, 255), performSignIn));
    }

    private PillButton createActionButton(string text, Color4 colour, Action action) =>
        new PillButton(audioService, text, colour, action)
        {
            RelativeSizeAxes = Axes.X
        };

    private void performSignIn()
    {
        _ = Task.Run(async () =>
        {
            await sessionService.SignInAsync(usernameBox.Text, passwordBox.Text).ConfigureAwait(false);
            await accountService.RefreshAsync().ConfigureAwait(false);

            Schedule(() =>
            {
                onSessionChanged();
                rebuild();
            });
        });
    }
}
