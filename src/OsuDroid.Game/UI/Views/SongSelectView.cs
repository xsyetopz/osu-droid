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

public partial class SongSelectView : CompositeDrawable
{
    private readonly ILocalBeatmapLibraryService _beatmapLibraryService;
    private readonly IExternalUriLauncher _externalUriLauncher;
    private readonly IAudioService _audioService;
    private readonly IGameResources _resources;
    private readonly Action _onBack;

    private readonly List<BeatmapCard> _beatmaps = [];
    private BeatmapCard? _selectedBeatmap;

    private FillFlowContainer _beatmapList = null!;
    private FillFlowContainer _detailsFlow = null!;
    private PanelTextBox _searchBox = null!;

    public SongSelectView(
        ILocalBeatmapLibraryService beatmapLibraryService,
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
                    Texture = getTexture("Menu/menu-background-2"),
                    FillMode = FillMode.Fill,
                    Alpha = 0.78f
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(10, 13, 24, 150)
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(28),
                    Spacing = new Vector2(0, 20),
                    Children = new Drawable[]
                    {
                        createTopBar(),
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColumnDimensions =
                            [
                                new Dimension(GridSizeMode.Relative, 0.38f),
                                new Dimension(GridSizeMode.Relative, 0.62f)
                            ],
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    createDetailsPanel(),
                                    createListPanel()
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
        _ = Task.Run(async () =>
        {
            IReadOnlyList<BeatmapCard> items = await _beatmapLibraryService.RefreshAsync().ConfigureAwait(false);
            Schedule(() =>
            {
                _beatmaps.Clear();
                _beatmaps.AddRange(items);
                _selectedBeatmap = _beatmaps.FirstOrDefault();
                refreshView();
            });
        });
    }

    private GridContainer createTopBar()
    {
        GridContainer topBar = new()
        {
            RelativeSizeAxes = Axes.X,
            Height = 62,
            ColumnDimensions =
            [
                new Dimension(GridSizeMode.Relative, 0.22f),
                new Dimension(GridSizeMode.Relative, 0.78f)
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
                            _searchBox = new PanelTextBox(osu.Game.Resources.Localisation.Web.HomeStrings.SearchPlaceholder),
                            new PillButton(_audioService, OsuDroid.Game.Localisation.SongSelectStrings.Group, new Color4(32, 40, 66, 255), refreshView),
                            new PillButton(_audioService, OsuDroid.Game.Localisation.SongSelectStrings.Sort, new Color4(32, 40, 66, 255), refreshView)
                        }
                    }
                }
            }
        };

        _searchBox.Width = 420;
        _searchBox.TextBox.Current.BindValueChanged(_ => refreshView());
        return topBar;
    }

    private Container createDetailsPanel()
    {
        _detailsFlow = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Padding = new MarginPadding(24),
            Spacing = new Vector2(0, 14)
        };

        return panelContainer(_detailsFlow, new Color4(18, 24, 40, 246));
    }

    private Container createListPanel()
    {
        _beatmapList = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 12)
        };

        return panelContainer(
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(22),
                Child = new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = _beatmapList
                }
            },
            new Color4(16, 22, 36, 242));
    }

    private void refreshView()
    {
        BeatmapCard[] filtered = _beatmaps
            .Where(matchesSearch)
            .ToArray();

        if (_selectedBeatmap is null || !filtered.Contains(_selectedBeatmap))
        {
            _selectedBeatmap = filtered.FirstOrDefault();
        }

        rebuildDetails(filtered);
        rebuildList(filtered);
    }

    private bool matchesSearch(BeatmapCard beatmap)
    {
        if (string.IsNullOrWhiteSpace(_searchBox.Text))
        {
            return true;
        }

        string query = _searchBox.Text.Trim();
        return beatmap.Artist.Contains(query, StringComparison.OrdinalIgnoreCase)
               || beatmap.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
               || beatmap.Mapper.Contains(query, StringComparison.OrdinalIgnoreCase)
               || beatmap.DifficultyName.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private void rebuildDetails(IReadOnlyList<BeatmapCard> filtered)
    {
        _detailsFlow.Clear();

        if (_selectedBeatmap is null)
        {
            _detailsFlow.AddRange(new Drawable[]
            {
                new SpriteText
                {
                    Text = OsuDroid.Game.Localisation.SongSelectStrings.NoMatchingBeatmaps,
                    Font = FontUsage.Default.With(size: 30, weight: "Bold")
                },
                new SpriteText
                {
                    Text = OsuDroid.Game.Localisation.SongSelectStrings.NoMatchingBeatmapsDescription,
                    Font = FontUsage.Default.With(size: 19),
                    Colour = new Color4(188, 196, 220, 255)
                },
                new PillButton(_audioService, OsuDroid.Game.Localisation.ButtonSystemStrings.Browse, new Color4(86, 126, 219, 255), () =>
                    _externalUriLauncher.Open(new Uri("https://osu.direct")))
            });
            return;
        }

        _detailsFlow.AddRange(new Drawable[]
        {
            new SpriteText
            {
                Text = OsuDroid.Game.Localisation.SongSelectStrings.Details,
                Font = FontUsage.Default.With(size: 18, weight: "Bold"),
                Colour = new Color4(140, 154, 214, 255)
            },
            new SpriteText
            {
                Text = _selectedBeatmap.Title,
                Font = FontUsage.Default.With(size: 34, weight: "Bold")
            },
            new SpriteText
            {
                Text = _selectedBeatmap.Artist,
                Font = FontUsage.Default.With(size: 20),
                Colour = new Color4(214, 219, 236, 255)
            },
            createMetadataRow(OsuDroid.Game.Localisation.SongSelectStrings.Author.ToString(), _selectedBeatmap.Mapper),
            createMetadataRow(OsuDroid.Game.Localisation.SongSelectStrings.Difficulty.ToString(), _selectedBeatmap.DifficultyName),
            createMetadataRow(OsuDroid.Game.Localisation.SongSelectStrings.RankedStatus.ToString(), _selectedBeatmap.Status ?? OsuDroid.Game.Localisation.SongSelectStrings.StatusUnknown.ToString()),
            createMetadataRow(OsuDroid.Game.Localisation.SongSelectStrings.Source.ToString(), _selectedBeatmap.SourceLabel ?? "-"),
            new Box
            {
                RelativeSizeAxes = Axes.X,
                Height = 2,
                Colour = new Color4(255, 255, 255, 20)
            },
            new SpriteText
            {
                Text = $"{filtered.Count} maps",
                Font = FontUsage.Default.With(size: 18),
                Colour = new Color4(170, 177, 201, 255)
            }
        });
    }

    private static FillFlowContainer createMetadataRow(string label, string value)
        => new()
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 2),
            Children = new Drawable[]
            {
                new SpriteText
                {
                    Text = label,
                    Font = FontUsage.Default.With(size: 16),
                    Colour = new Color4(129, 141, 184, 255)
                },
                new SpriteText
                {
                    Text = value,
                    Font = FontUsage.Default.With(size: 22, weight: "SemiBold")
                }
            }
        };

    private void rebuildList(IReadOnlyList<BeatmapCard> filtered)
    {
        _beatmapList.Clear();

        if (filtered.Count == 0)
        {
            _beatmapList.Add(new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Padding = new MarginPadding { Top = 72 },
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

        foreach (BeatmapCard beatmap in filtered)
        {
            bool selected = beatmap == _selectedBeatmap;
            _beatmapList.Add(new BeatmapListCard(beatmap, selected, () =>
            {
                _selectedBeatmap = beatmap;
                _audioService.PlayMenuSample(MenuSample.SelectDifficulty);
                refreshView();
            }));
        }
    }

    private static Container panelContainer(Drawable child, Color4 colour)
        => new()
        {
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 28,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colour
                },
                child
            }
        };

    private Texture getTexture(string name) => _resources.GetTexture(name);
}
