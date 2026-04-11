using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

public partial class BrowseView : CompositeDrawable
{
    private readonly IOnlineBeatmapBrowseService _beatmapLibraryService;
    private readonly IExternalUriLauncher _externalUriLauncher;
    private readonly IAudioService _audioService;
    private readonly IGameResources _resources;
    private readonly Action _onBack;

    private FillFlowContainer _resultsFlow = null!;
    private PanelTextBox _searchBox = null!;
    private SpriteText _stateText = null!;

    public BrowseView(
        IOnlineBeatmapBrowseService beatmapLibraryService,
        IExternalUriLauncher externalUriLauncher,
        IAudioService audioService,
        IGameResources resources,
        Action onBack)
    {
        _beatmapLibraryService = beatmapLibraryService;
        _externalUriLauncher = externalUriLauncher;
        _audioService = audioService;
        _resources = resources;
        _onBack = onBack;

        RelativeSizeAxes = Axes.Both;

        InternalChild = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = getTexture("Menu/menu-background-3"),
                    FillMode = FillMode.Fill,
                    Alpha = 0.76f
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(11, 16, 28, 150)
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(28),
                    Spacing = new Vector2(0, 18),
                    Children = new Drawable[]
                    {
                        createTopBar(),
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 28,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = new Color4(18, 24, 38, 242)
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding(22),
                                    Spacing = new Vector2(0, 16),
                                    Children = new Drawable[]
                                    {
                                        _stateText = new SpriteText
                                        {
                                            Text = OsuDroid.Game.Localisation.ButtonSystemStrings.Browse,
                                            Font = FontUsage.Default.With(size: 28, weight: "Bold")
                                        },
                                        new BasicScrollContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Child = _resultsFlow = new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(0, 12)
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();
        _ = performSearch(string.Empty);
    }

    private GridContainer createTopBar()
    {
        _searchBox = new PanelTextBox(osu.Game.Resources.Localisation.Web.HomeStrings.SearchPlaceholder);
        _searchBox.Width = 440;

        return new GridContainer
        {
            RelativeSizeAxes = Axes.X,
            Height = 62,
            ColumnDimensions =
            [
                new Dimension(GridSizeMode.Relative, 0.18f),
                new Dimension(GridSizeMode.Relative, 0.82f)
            ],
            Content = new[]
            {
                new Drawable[]
                {
                    new PillButton(_audioService, OsuDroid.Game.Localisation.ButtonSystemStrings.Back, new Color4(50, 61, 96, 255), _onBack),
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(12, 0),
                        Children = new Drawable[]
                        {
                            _searchBox,
                            new PillButton(_audioService, osu.Game.Resources.Localisation.Web.HomeStrings.SearchTitle, new Color4(86, 126, 219, 255), () => _ = performSearch(_searchBox.Text))
                        }
                    }
                }
            }
        };
    }

    private async Task performSearch(string query)
    {
        Schedule(() =>
        {
            _stateText.Text = OsuDroid.Game.Localisation.ButtonSystemStrings.Browse;
            _resultsFlow.Clear();
            _resultsFlow.Add(new Box
            {
                RelativeSizeAxes = Axes.X,
                Height = 6,
                Colour = new Color4(226, 113, 165, 255)
            });
        });

        IReadOnlyList<BeatmapCard> results;

        try
        {
            results = await _beatmapLibraryService.SearchAsync(query).ConfigureAwait(false);
        }
        catch
        {
            results = [];
        }

        Schedule(() =>
        {
            _resultsFlow.Clear();

            if (results.Count == 0)
            {
                _stateText.Text = OsuDroid.Game.Localisation.SongSelectStrings.NoMatchingBeatmaps;
                _resultsFlow.Add(new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Top = 80 },
                    Spacing = new Vector2(0, 12),
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = OsuDroid.Game.Localisation.SongSelectStrings.NoMatchingBeatmaps,
                            Font = FontUsage.Default.With(size: 26, weight: "Bold")
                        },
                        new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = OsuDroid.Game.Localisation.SongSelectStrings.NoMatchingBeatmapsDescription,
                            Font = FontUsage.Default.With(size: 18),
                            Colour = new Color4(184, 192, 216, 255)
                        }
                    }
                });
                return;
            }

            foreach (List<BeatmapCard> row in chunk(results, 2))
            {
                _resultsFlow.Add(new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 138,
                    ColumnDimensions =
                    [
                        new Dimension(GridSizeMode.Relative, 0.5f),
                        new Dimension(GridSizeMode.Relative, 0.5f)
                    ],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            createCard(row[0]),
                            row.Count > 1 ? createCard(row[1]) : new Container()
                        }
                    }
                });
            }
        });
    }

    private Container createCard(BeatmapCard beatmap)
        => new()
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding { Right = 8 },
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 24,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Color4(34, 42, 66, 255)
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding(18),
                        Spacing = new Vector2(0, 6),
                        Children = new Drawable[]
                        {
                            new SpriteText
                            {
                                Text = beatmap.Title,
                                Font = FontUsage.Default.With(size: 24, weight: "Bold")
                            },
                            new SpriteText
                            {
                                Text = beatmap.Artist,
                                Font = FontUsage.Default.With(size: 18),
                                Colour = new Color4(206, 214, 235, 255)
                            },
                            new SpriteText
                            {
                                Text = $"{beatmap.DifficultyName}  •  {beatmap.Mapper}",
                                Font = FontUsage.Default.With(size: 17),
                                Colour = new Color4(169, 180, 210, 255)
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(8, 0),
                                Children = new Drawable[]
                                {
                                    new PillButton(_audioService, OsuDroid.Game.Localisation.SongSelectStrings.Details, new Color4(69, 82, 120, 255), () =>
                                        _externalUriLauncher.Open(new Uri("https://osu.direct"))),
                                    new PillButton(_audioService, OsuDroid.Game.Localisation.ButtonSystemStrings.Browse, new Color4(86, 126, 219, 255), () =>
                                        _externalUriLauncher.Open(new Uri("https://osu.direct")))
                                }
                            }
                        }
                    }
                }
            }
        };

    private static List<List<BeatmapCard>> chunk(IReadOnlyList<BeatmapCard> results, int size)
    {
        List<List<BeatmapCard>> rows = [];

        for (int index = 0; index < results.Count; index += size)
        {
            rows.Add(results.Skip(index).Take(size).ToList());
        }

        return rows;
    }

    private Texture getTexture(string name) => _resources.GetTexture(name);
}
