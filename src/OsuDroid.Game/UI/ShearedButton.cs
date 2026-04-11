using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace OsuDroid.Game.UI;

public partial class ShearedButton : ClickableContainer
{
    private readonly Color4 baseColour;
    private readonly Box fill;
    private readonly Box accent;
    private readonly SpriteText label;
    private readonly IAudioService audioService;

    public string Text
    {
        get => label.Text.ToString();
        set => label.Text = value;
    }

    public ShearedButton(IAudioService audioService, string text, Color4 colour, Action action)
    {
        this.audioService = audioService;
        baseColour = colour;
        RelativeSizeAxes = Axes.X;
        Height = 88;
        Masking = true;
        CornerRadius = 22;
        Shear = new Vector2(-0.18f, 0);
        Action = () =>
        {
            audioService.PlayMenuSample(MenuSample.ButtonConfirm);
            action();
        };

        Child = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Shear = new Vector2(0.18f, 0),
            Children = new Drawable[]
            {
                fill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = baseColour
                },
                accent = new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 10,
                    Colour = Color4.White,
                    Alpha = 0.24f
                },
                label = new SpriteText
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

    protected override bool OnHover(HoverEvent e)
    {
        audioService.PlayMenuSample(MenuSample.ButtonHover);
        fill.FadeColour(lighten(baseColour, 18), 140);
        accent.FadeTo(0.45f, 140);
        this.MoveToX(8, 140, Easing.OutQuint);
        return base.OnHover(e);
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        fill.FadeColour(baseColour, 140);
        accent.FadeTo(0.24f, 140);
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

    private static Color4 lighten(Color4 colour, byte amount) =>
        new(
            (byte)Math.Min(255, colour.R + amount),
            (byte)Math.Min(255, colour.G + amount),
            (byte)Math.Min(255, colour.B + amount),
            colour.A);
}
