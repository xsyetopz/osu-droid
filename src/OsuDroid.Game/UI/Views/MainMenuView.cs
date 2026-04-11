using System;
using OsuDroid.Game.Localisation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using OsuDroid.Game.UI.Controls;

namespace OsuDroid.Game.UI.Views;

public partial class MainMenuView : CompositeDrawable
{
    private readonly IAudioService audioService;
    private readonly Action onSolo;
    private readonly Action onBrowse;

    private FillFlowContainer rail = null!;
    private bool playBranchOpen;

    public MainMenuView(IAudioService audioService, Action onSolo, Action onBrowse)
    {
        this.audioService = audioService;
        this.onSolo = onSolo;
        this.onBrowse = onBrowse;

        RelativeSizeAxes = Axes.Both;

        InternalChild = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(8, 10, 20, 255)
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.35f,
                    Colour = new Color4(20, 50, 104, 255)
                },
                new CircularContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    Position = new Vector2(310, 0),
                    Size = new Vector2(320),
                    Masking = true,
                    BorderThickness = 10,
                    BorderColour = new Color4(255, 255, 255, 45),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = new Color4(228, 104, 164, 255)
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.5f,
                            Colour = new Color4(72, 112, 220, 255)
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Spacing = new Vector2(0, 6),
                            Children = new Drawable[]
                            {
                                new SpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "osu!",
                                    Font = FontUsage.Default.With(size: 84, weight: "Bold")
                                },
                                new SpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "droid",
                                    Font = FontUsage.Default.With(size: 42, weight: "Bold")
                                }
                            }
                        }
                    }
                },
                rail = new FillFlowContainer
                {
                    Width = 340,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 18),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Position = new Vector2(270, 0)
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 132,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Padding = new MarginPadding { Right = 26 },
                    Child = new ShearedButton(audioService, ButtonSystemStrings.Browse.ToString(), new Color4(86, 126, 219, 255), onBrowse)
                }
            }
        };

        updateRail();
    }

    private void updateRail()
    {
        rail.Clear();

        if (playBranchOpen)
        {
            rail.Add(new ShearedButton(audioService, ButtonSystemStrings.Solo.ToString(), new Color4(230, 106, 169, 255), onSolo));
            rail.Add(new ShearedButton(audioService, ButtonSystemStrings.Back.ToString(), new Color4(51, 63, 99, 255), () =>
            {
                playBranchOpen = false;
                updateRail();
            }));
            return;
        }

        rail.Add(new ShearedButton(audioService, ButtonSystemStrings.Play.ToString(), new Color4(230, 106, 169, 255), () =>
        {
            playBranchOpen = true;
            audioService.PlayMenuSample(MenuSample.LogoSwoosh);
            updateRail();
        }));
    }
}
