using System;
using OsuDroid.Game.UI.Controls;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Graphics;

namespace OsuDroid.Game.UI.Views;

public partial class AccountOverlayView : CompositeDrawable
{
    private readonly ISessionService _sessionService;
    private readonly IAccountService _accountService;
    private readonly IAudioService _audioService;
    private readonly Action _onClose;
    private readonly Action _onSessionChanged;

    private FillFlowContainer _content = null!;

    public AccountOverlayView(
        ISessionService sessionService,
        IAccountService accountService,
        IAudioService audioService,
        Action onClose,
        Action onSessionChanged)
    {
        _sessionService = sessionService;
        _accountService = accountService;
        _audioService = audioService;
        _onClose = onClose;
        _onSessionChanged = onSessionChanged;

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
                _content = new FillFlowContainer
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
        _content.Clear();

        SessionSnapshot session = _sessionService.Current;
        AccountProfile profile = _accountService.Current;

        _content.Add(new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 6),
            Children = new Drawable[]
            {
                new SpriteText
                {
                    Text = OsuDroid.Game.Localisation.LoginPanelStrings.Account,
                    Font = FontUsage.Default.With(size: 18, weight: "Bold"),
                    Colour = new Color4(164, 178, 255, 255)
                },
                new SpriteText
                {
                    Text = session.DisplayName,
                    Font = FontUsage.Default.With(size: 34, weight: "Bold")
                },
                new SpriteText
                {
                    Text = chooseStatusText(profile),
                    Font = FontUsage.Default.With(size: 18),
                    Colour = new Color4(196, 203, 227, 255)
                }
            }
        });

        _content.Add(new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 10),
            Children = new Drawable[]
            {
                createActionButton(OsuDroid.Game.Localisation.ButtonSystemStrings.Back, new Color4(76, 89, 130, 255), () =>
                {
                    _onSessionChanged();
                    _onClose();
                })
            }
        });
    }

    private PillButton createActionButton(LocalisableString text, Color4 colour, Action action)
        => new(_audioService, text, colour, action)
        {
            RelativeSizeAxes = Axes.X
        };

    private static LocalisableString chooseStatusText(AccountProfile profile)
        => string.IsNullOrWhiteSpace(profile.StatusText)
            ? OsuDroid.Game.Localisation.LoginPanelStrings.AppearOffline
            : profile.StatusText;
}
