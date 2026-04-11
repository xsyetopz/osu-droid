using System;
using OsuDroid.Game.Resources;
using OsuDroid.Game.UI.Controls;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;
using osuTK.Graphics;

namespace OsuDroid.Game.UI.Views;

public partial class MainMenuView : CompositeDrawable
{
    private readonly IAudioService _audioService;
    private readonly IGameResources _resources;
    private readonly Action _onSolo;
    private readonly Action _onBrowse;

    private FillFlowContainer _rail = null!;
    private bool _playBranchOpen;

    public MainMenuView(IAudioService audioService, IGameResources resources, Action onSolo, Action onBrowse)
    {
        _audioService = audioService;
        _resources = resources;
        _onSolo = onSolo;
        _onBrowse = onBrowse;

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
                new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = getTexture("Menu/menu-background-1"),
                    FillMode = FillMode.Fill,
                    Alpha = 0.82f
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(12, 16, 30, 160)
                },
                new Sprite
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    Position = new Vector2(278, 0),
                    Size = new Vector2(300),
                    Texture = getTexture("Menu/logo")
                },
                new Sprite
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(52, -34),
                    Size = new Vector2(128),
                    Texture = getTexture("Menu/fountain-star"),
                    Alpha = 0.75f
                },
                _rail = new FillFlowContainer
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
                    Width = 164,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Padding = new MarginPadding { Right = 26 },
                    Child = new ShearedButton(
                        _audioService,
                        OsuDroid.Game.Localisation.ButtonSystemStrings.Browse,
                        new Color4(86, 126, 219, 255),
                        _onBrowse)
                }
            }
        };

        updateRail();
    }

    private void updateRail()
    {
        _rail.Clear();

        if (_playBranchOpen)
        {
            _rail.Add(new ShearedButton(
                _audioService,
                OsuDroid.Game.Localisation.ButtonSystemStrings.Solo,
                new Color4(230, 106, 169, 255),
                _onSolo));
            _rail.Add(new ShearedButton(
                _audioService,
                OsuDroid.Game.Localisation.ButtonSystemStrings.Back,
                new Color4(51, 63, 99, 255),
                () =>
                {
                    _playBranchOpen = false;
                    updateRail();
                }));
            return;
        }

        _rail.Add(new ShearedButton(
            _audioService,
            OsuDroid.Game.Localisation.ButtonSystemStrings.Play,
            new Color4(230, 106, 169, 255),
            () =>
            {
                _playBranchOpen = true;
                _audioService.PlayMenuSample(MenuSample.LogoSwoosh);
                updateRail();
            }));
    }

    private Texture getTexture(string name) => _resources.GetTexture(name);
}
