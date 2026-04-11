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

public partial class ShearedButton : ClickableContainer
{
    private readonly Color4 _baseColour;
    private readonly Box _fill;
    private readonly Box _accent;
    private readonly SpriteText _label;
    private readonly IAudioService _audioService;

    public string Text
    {
        get => _label.Text.ToString();
        set => _label.Text = value;
    }

    public ShearedButton(IAudioService audioService, LocalisableString text, Color4 colour, Action action)
    {
        _audioService = audioService;
        _baseColour = colour;
        RelativeSizeAxes = Axes.X;
        Height = 88;
        Masking = true;
        CornerRadius = 22;
        Shear = new Vector2(-0.18f, 0);
        Action = () =>
        {
            _audioService.PlayMenuSample(MenuSample.ButtonConfirm);
            action();
        };

        Child = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Shear = new Vector2(0.18f, 0),
            Children = new Drawable[]
            {
                _fill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = _baseColour
                },
                _accent = new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 10,
                    Colour = Color4.White,
                    Alpha = 0.24f
                },
                _label = new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Position = new Vector2(28, 44),
                    Font = FontUsage.Default.With(size: 32, weight: "Bold"),
                    Text = text
                }
            }
        };
    }

    public ShearedButton(IAudioService audioService, string text, Color4 colour, Action action)
        : this(audioService, (LocalisableString)text, colour, action)
    {
    }

    protected override bool OnHover(HoverEvent e)
    {
        _audioService.PlayMenuSample(MenuSample.ButtonHover);
        _fill.FadeColour(lighten(_baseColour, 18), 140);
        _accent.FadeTo(0.45f, 140);
        this.MoveToX(8, 140, Easing.OutQuint);
        return base.OnHover(e);
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        _fill.FadeColour(_baseColour, 140);
        _accent.FadeTo(0.24f, 140);
        this.MoveToX(0, 180, Easing.OutQuint);
        base.OnHoverLost(e);
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        this.ScaleTo(new Vector2(0.985f, 0.985f), 60, Easing.OutQuint);
        return base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseUpEvent e)
    {
        this.ScaleTo(Vector2.One, 120, Easing.OutQuint);
        base.OnMouseUp(e);
    }

    private static Color4 lighten(Color4 colour, byte amount)
        => new(
            (byte)Math.Min(255, colour.R + amount),
            (byte)Math.Min(255, colour.G + amount),
            (byte)Math.Min(255, colour.B + amount),
            colour.A);
}
