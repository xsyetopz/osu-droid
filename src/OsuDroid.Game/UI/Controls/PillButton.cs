using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Graphics;

namespace OsuDroid.Game.UI.Controls;

public partial class PillButton : ClickableContainer
{
    private readonly Box _background;
    private readonly SpriteText _label;
    private readonly IAudioService _audioService;

    public string Text
    {
        get => _label.Text.ToString();
        set => _label.Text = value;
    }

    public PillButton(IAudioService audioService, LocalisableString text, Color4 colour, Action action)
    {
        _audioService = audioService;
        Action = () =>
        {
            _audioService.PlayMenuSample(MenuSample.ButtonConfirm);
            action();
        };

        AutoSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 22;

        Child = new Container
        {
            AutoSizeAxes = Axes.Both,
            Padding = new MarginPadding { Horizontal = 18, Vertical = 12 },
            Children = new Drawable[]
            {
                _background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour,
                    Alpha = 0.9f
                },
                _label = new SpriteText
                {
                    Position = new Vector2(0, 2),
                    Font = FontUsage.Default.With(size: 22, weight: "SemiBold"),
                    Text = text
                }
            }
        };
    }

    public PillButton(IAudioService audioService, string text, Color4 colour, Action action)
        : this(audioService, (LocalisableString)text, colour, action)
    {
    }

    protected override bool OnHover(HoverEvent e)
    {
        _audioService.PlayMenuSample(MenuSample.ButtonHover);
        _background.FadeTo(1f, 120);
        return base.OnHover(e);
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        _background.FadeTo(0.9f, 120);
        base.OnHoverLost(e);
    }
}
