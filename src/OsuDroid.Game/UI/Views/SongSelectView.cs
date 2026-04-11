using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OsuDroid.Game.Localisation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using OsuDroid.Game.UI.Controls;

namespace OsuDroid.Game.UI.Views;

public partial class SongSelectView : CompositeDrawable
{
    private readonly IBeatmapLibraryService beatmapLibraryService;
    private readonly IExternalUriLauncher externalUriLauncher;
    private readonly IAudioService audioService;
    private readonly Action onBack;

    private readonly List<BeatmapCard> beatmaps = [];
    private BeatmapCard? selectedBeatmap;

    private FillFlowContainer beatmapList = null!;
    private FillFlowContainer detailsFlow = null!;
    private Container listContent = null!;
    private PanelTextBox searchBox = null!;

    public SongSelectView(
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
                    Colour = new Color4(10, 13, 24, 255)
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
            var items = await beatmapLibraryService.RefreshLocalAsync().ConfigureAwait(false);
            Schedule(() =>
            {
                beatmaps.Clear();
                beatmaps.AddRange(items);
                selectedBeatmap = beatmaps.FirstOrDefault();
                refreshView();
            });
        });
    }

    private GridContainer createTopBar()
    {
        var topBar = new GridContainer
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
                    new PillButton(audioService, ButtonSystemStrings.Back.ToString(), new Color4(50, 61, 96, 255), onBack),
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(12, 0),
                        Children = new Drawable[]
                        {
                            searchBox = new PanelTextBox("search"),
                            new PillButton(audioService, SongSelectStrings.Group.ToString(), new Color4(32, 40, 66, 255), refreshView),
                            new PillButton(audioService, SongSelectStrings.Sort.ToString(), new Color4(32, 40, 66, 255), refreshView)
                        }
                    }
                }
            }
        };

        searchBox.Width = 420;
        searchBox.TextBox.Current.BindValueChanged(_ => refreshView());
        return topBar;
    }

    private Container createDetailsPanel()
    {
        detailsFlow = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Padding = new MarginPadding(24),
            Spacing = new Vector2(0, 14)
        };

        return new Container
        {
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 28,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(18, 24, 40, 246)
                },
                detailsFlow
            }
        };
    }

    private Container createListPanel()
    {
        beatmapList = new FillFlowContainer
        {
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 12)
        };

        listContent = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding(22),
            Child = new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = beatmapList
            }
        };

        return new Container
        {
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 28,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(16, 22, 36, 242)
                },
                listContent
            }
        };
    }

    private void refreshView()
    {
        var filtered = beatmaps
            .Where(matchesSearch)
            .ToArray();

        if (selectedBeatmap is null || !filtered.Contains(selectedBeatmap))
            selectedBeatmap = filtered.FirstOrDefault();

        rebuildDetails(filtered);
        rebuildList(filtered);
    }

    private bool matchesSearch(BeatmapCard beatmap)
    {
        if (string.IsNullOrWhiteSpace(searchBox.Text))
            return true;

        var query = searchBox.Text.Trim();
        return beatmap.Artist.Contains(query, StringComparison.OrdinalIgnoreCase)
               || beatmap.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
               || beatmap.Mapper.Contains(query, StringComparison.OrdinalIgnoreCase)
               || beatmap.DifficultyName.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private void rebuildDetails(IReadOnlyList<BeatmapCard> filtered)
    {
        detailsFlow.Clear();

        if (selectedBeatmap is null)
        {
            detailsFlow.AddRange(new Drawable[]
            {
                new SpriteText
                {
                    Text = SongSelectStrings.NoMatchingBeatmaps,
                    Font = FontUsage.Default.With(size: 30, weight: "Bold")
                },
                new SpriteText
                {
                    Text = SongSelectStrings.NoMatchingBeatmapsDescription,
                    Font = FontUsage.Default.With(size: 19),
                    Colour = new Color4(188, 196, 220, 255)
                },
                new PillButton(audioService, ButtonSystemStrings.Browse.ToString(), new Color4(86, 126, 219, 255), () =>
                    externalUriLauncher.Open(new Uri("https://osu.direct")))
            });
            return;
        }

        detailsFlow.AddRange(new Drawable[]
        {
            new SpriteText
            {
                Text = SongSelectStrings.Details,
                Font = FontUsage.Default.With(size: 18, weight: "Bold"),
                Colour = new Color4(140, 154, 214, 255)
            },
            new SpriteText
            {
                Text = selectedBeatmap.Title,
                Font = FontUsage.Default.With(size: 34, weight: "Bold")
            },
            new SpriteText
            {
                Text = selectedBeatmap.Artist,
                Font = FontUsage.Default.With(size: 20),
                Colour = new Color4(214, 219, 236, 255)
            },
            createMetadataRow(SongSelectStrings.Author.ToString(), selectedBeatmap.Mapper),
            createMetadataRow(SongSelectStrings.Difficulty.ToString(), selectedBeatmap.DifficultyName),
            createMetadataRow(SongSelectStrings.Ranked.ToString(), filtered.Count.ToString(CultureInfo.InvariantCulture)),
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

    private static FillFlowContainer createMetadataRow(string label, string value) =>
        new FillFlowContainer
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
        beatmapList.Clear();

        if (filtered.Count == 0)
        {
            beatmapList.Add(new FillFlowContainer
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

        foreach (var beatmap in filtered)
        {
            var selected = beatmap == selectedBeatmap;
            beatmapList.Add(new BeatmapListCard(beatmap, selected, () =>
            {
                selectedBeatmap = beatmap;
                audioService.PlayMenuSample(MenuSample.SelectDifficulty);
                refreshView();
            }));
        }
    }
}
