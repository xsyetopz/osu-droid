using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace OsuDroid.Game.UI.Controls;

public partial class BeatmapListCard : ClickableContainer
{
    public BeatmapListCard(BeatmapCard beatmap, bool selected, Action action)
    {
        RelativeSizeAxes = Axes.X;
        Height = selected ? 118 : 94;
        Action = action;
        Masking = true;
        CornerRadius = 24;

        Child = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = selected ? new Color4(225, 112, 170, 255) : new Color4(30, 38, 58, 255)
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Position = new Vector2(22, selected ? 59 : 47),
                    Spacing = new Vector2(0, 4),
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = $"{beatmap.Artist} - {beatmap.Title}",
                            Font = FontUsage.Default.With(size: selected ? 27 : 24, weight: "Bold")
                        },
                        new SpriteText
                        {
                            Text = $"{beatmap.DifficultyName}  •  {beatmap.Mapper}",
                            Font = FontUsage.Default.With(size: 18),
                            Colour = selected ? new Color4(255, 242, 249, 255) : new Color4(185, 194, 221, 255)
                        }
                    }
                }
            }
        };
    }
}
