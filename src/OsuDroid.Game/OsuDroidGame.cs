using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OsuDroid.Game.UI.Views;

namespace OsuDroid.Game;

public partial class OsuDroidGame : osu.Framework.Game
{
    private readonly IAudioService audioService;
    private readonly IAccountService accountService;
    private readonly ISessionService sessionService;
    private readonly IBeatmapLibraryService beatmapLibraryService;
    private readonly IExternalUriLauncher externalUriLauncher;
    private readonly IPlatformStorage platformStorage;

    public OsuDroidGame(
        IAudioService audioService,
        IAccountService accountService,
        ISessionService sessionService,
        IBeatmapLibraryService beatmapLibraryService,
        IExternalUriLauncher externalUriLauncher,
        IPlatformStorage platformStorage)
    {
        this.audioService = audioService;
        this.accountService = accountService;
        this.sessionService = sessionService;
        this.beatmapLibraryService = beatmapLibraryService;
        this.externalUriLauncher = externalUriLauncher;
        this.platformStorage = platformStorage;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new Container
        {
            RelativeSizeAxes = Axes.Both,
            Child = new RootView(audioService, sessionService, accountService, beatmapLibraryService, externalUriLauncher)
        });
    }
}
