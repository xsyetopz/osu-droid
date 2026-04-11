using System.Net.Http;
using OsuDroid.Game.Resources;
using OsuDroid.Game.Services.Audio;
using OsuDroid.Game.Services.Guest;
using OsuDroid.Game.Services.Local;
using OsuDroid.Game.Services.Online;
using OsuDroid.Game.UI.Views;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;

namespace OsuDroid.Game;

public partial class OsuDroidGame : osu.Framework.Game
{
    private readonly IExternalUriLauncher externalUriLauncher;
    private readonly IPlatformStorage platformStorage;

    public OsuDroidGame(IExternalUriLauncher externalUriLauncher, IPlatformStorage platformStorage)
    {
        this.externalUriLauncher = externalUriLauncher;
        this.platformStorage = platformStorage;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Resources.AddStore(new DllResourceStore(typeof(OsuDroid.Game.Localisation.ButtonSystemStrings).Assembly));

        IGameResources gameResources = new GameResources(
            Textures,
            Audio.GetSampleStore(new NamespacedResourceStore<byte[]>(Resources, "Samples")));
        IAudioService audioService = new FrameworkAudioService(gameResources);
        ISessionService sessionService = new GuestSessionService();
        IAccountService accountService = new GuestAccountService();
        ILocalBeatmapLibraryService localBeatmapLibraryService = new FileSystemLocalBeatmapLibraryService(platformStorage);
        IOnlineBeatmapBrowseService onlineBeatmapBrowseService = new OsuDirectBeatmapBrowseService(new HttpClient());

        Add(new Container
        {
            RelativeSizeAxes = Axes.Both,
            Child = new RootView(
                audioService,
                sessionService,
                accountService,
                localBeatmapLibraryService,
                onlineBeatmapBrowseService,
                externalUriLauncher,
                gameResources)
        });
    }
}
