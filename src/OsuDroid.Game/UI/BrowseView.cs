using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OsuDroid.Game.Localisation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace OsuDroid.Game.UI;

public partial class BrowseView : CompositeDrawable
{
    private readonly IBeatmapLibraryService beatmapLibraryService;
    private readonly IExternalUriLauncher externalUriLauncher;
    private readonly IAudioService audioService;
    private readonly Action onBack;

    private FillFlowContainer resultsFlow = null!;
    private PanelTextBox searchBox = null!;
    private SpriteText stateText = null!;

    public BrowseView(
        IBeatmapLibraryService beatmapLibraryService,
        IExternalUriLauncher externalUriLauncher,
        IAudioService audioService,
        Action onBack)
    {
        this.beatmapLibraryService = beatmapLibraryService;
        this.externalUriLauncher = externalUriLauncher;
        this.audioService = audioService;
        this.onBack = onBack;

        RelativeSizeAxes = Axes.Both;

        InternalChild = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(11, 16, 28, 255)
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
                                        stateText = new SpriteText
                                        {
                                            Text = ButtonSystemStrings.Browse,
                                            Font = FontUsage.Default.With(size: 28, weight: "Bold")
                                        },
                                        new BasicScrollContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Child = resultsFlow = new FillFlowContainer
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
        searchBox = new PanelTextBox("search");
        searchBox.Width = 440;

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
                    new PillButton(audioService, ButtonSystemStrings.Back.ToString(), new Color4(50, 61, 96, 255), onBack),
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(12, 0),
                        Children = new Drawable[]
                        {
                            searchBox,
                            new PillButton(audioService, ButtonSystemStrings.Browse.ToString(), new Color4(86, 126, 219, 255), () => _ = performSearch(searchBox.Text))
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
            stateText.Text = ButtonSystemStrings.Browse;
            resultsFlow.Clear();
            resultsFlow.Add(new SpriteText
            {
                Text = "loading…",
                Font = FontUsage.Default.With(size: 20),
                Colour = new Color4(181, 190, 215, 255)
            });
        });

        var results = await beatmapLibraryService.SearchOnlineAsync(query).ConfigureAwait(false);

        Schedule(() =>
        {
            resultsFlow.Clear();

            if (results.Count == 0)
            {
                stateText.Text = SongSelectStrings.NoMatchingBeatmaps;
                resultsFlow.Add(new FillFlowContainer
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
                            Text = SongSelectStrings.NoMatchingBeatmaps,
                            Font = FontUsage.Default.With(size: 26, weight: "Bold")
                        },
                        new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = SongSelectStrings.NoMatchingBeatmapsDescription,
                            Font = FontUsage.Default.With(size: 18),
                            Colour = new Color4(184, 192, 216, 255)
                        }
                    }
                });
                return;
            }

            foreach (var row in chunk(results, 2))
            {
                resultsFlow.Add(new GridContainer
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

    private Container createCard(BeatmapCard beatmap) =>
        new Container
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
                                    new PillButton(audioService, SongSelectStrings.Details.ToString(), new Color4(69, 82, 120, 255), () =>
                                        externalUriLauncher.Open(new Uri("https://osu.direct"))),
                                    new PillButton(audioService, ButtonSystemStrings.Browse.ToString(), new Color4(86, 126, 219, 255), () =>
                                        externalUriLauncher.Open(new Uri("https://osu.direct")))
                                }
                            }
                        }
                    }
                }
            }
        };

    private static List<List<BeatmapCard>> chunk(IReadOnlyList<BeatmapCard> results, int size)
    {
        var rows = new List<List<BeatmapCard>>();

        for (var index = 0; index < results.Count; index += size)
            rows.Add(results.Skip(index).Take(size).ToList());

        return rows;
    }
}
