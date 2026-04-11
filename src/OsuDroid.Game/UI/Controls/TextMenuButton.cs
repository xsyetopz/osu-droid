using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace OsuDroid.Game.UI.Controls;

public partial class TextMenuButton : ClickableContainer
{
    private readonly Box background;
    private readonly Color4 baseColour;

    public TextMenuButton(string text, Color4 colour, Action action)
    {
        baseColour = colour;
        RelativeSizeAxes = Axes.X;
        Height = 74;
        Action = action;
        Masking = true;
        CornerRadius = 18;

        Child = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = baseColour
                },
                new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Position = new Vector2(24, 37),
                    Font = FontUsage.Default.With(size: 28, weight: "SemiBold"),
                    Text = text
                }
            }
        };
    }

    protected override bool OnHover(HoverEvent e)
    {
        background.FadeTo(0.85f, 100);
        return base.OnHover(e);
    }

    protected override void OnHoverLost(HoverLostEvent e)
    {
        background.FadeTo(1f, 100);
        base.OnHoverLost(e);
    }
}
