using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace OsuDroid.Game.UI;

public partial class PillButton : ClickableContainer
{
    private readonly Box background;
    private readonly SpriteText label;
    private readonly IAudioService audioService;

    public string Text
    {
        get => label.Text.ToString();
        set => label.Text = value;
    }

    public PillButton(IAudioService audioService, string text, Color4 colour, Action action)
    {
        this.audioService = audioService;
        Action = () =>
        {
            audioService.PlayMenuSample(MenuSample.ButtonConfirm);
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
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour,
                    Alpha = 0.9f
                },
                label = new SpriteText
                {
                    Position = new Vector2(0, 2),
                    Font = FontUsage.Default.With(size: 22, weight: "SemiBold"),
                    Text = text
                }
            }
        };
    }

    protected override bool OnHover(HoverEvent e)
    {
        audioService.PlayMenuSample(MenuSample.ButtonHover);
        background.FadeTo(1f, 120);
        return base.OnHover(e);
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        background.FadeTo(0.9f, 120);
        base.OnHoverLost(e);
    }
}
