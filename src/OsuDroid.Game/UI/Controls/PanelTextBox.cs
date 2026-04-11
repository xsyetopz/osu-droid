using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace OsuDroid.Game.UI.Controls;

public partial class PanelTextBox : CompositeDrawable
{
    public BasicTextBox TextBox { get; }

    public string Text
    {
        get => TextBox.Current.Value;
        set => TextBox.Current.Value = value;
    }

    public string PlaceholderText
    {
        get => TextBox.PlaceholderText.ToString();
        set => TextBox.PlaceholderText = value;
    }

    public PanelTextBox(string placeholderText, bool secret = false)
    {
        RelativeSizeAxes = Axes.X;
        Height = 58;
        Masking = true;
        CornerRadius = 18;

        InternalChildren = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = new Color4(22, 26, 39, 240)
            },
            TextBox = new BasicTextBox
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Horizontal = 18, Vertical = 14 },
                PlaceholderText = placeholderText
            }
        };

        _ = secret;
    }
}
