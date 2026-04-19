using OsuDroid.Game.Compatibility.Database;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;
using OsuDroid.Game.UI;

namespace OsuDroid.Game;

public sealed class OsuDroidGameCore
{
    private readonly MainMenuScene mainMenu = new();

    public OsuDroidGameCore(GameServices services)
    {
        Services = services;
    }

    public GameServices Services { get; }

    public MainMenuRoute LastRoute { get; private set; }

    public static OsuDroidGameCore Create(string corePath, string buildType)
    {
        var databasePath = DroidDatabaseConstants.GetDatabasePath(corePath, buildType);
        var database = new DroidDatabase(databasePath);
        database.EnsureCreated();
        return new OsuDroidGameCore(new GameServices(database, corePath, buildType));
    }

    public GameFrameSnapshot CurrentFrame => mainMenu.Snapshot;

    public GameFrameSnapshot CreateFrame(VirtualViewport viewport) => mainMenu.CreateSnapshot(viewport);

    public void Update(TimeSpan elapsed) => mainMenu.Update(elapsed);

    public void TapMainMenuCookie() => mainMenu.ToggleCookie();

    public MainMenuRoute HandleMainMenu(MainMenuAction action)
    {
        LastRoute = mainMenu.Handle(action);
        return LastRoute;
    }

    public MainMenuRoute TapMainMenu(MainMenuButtonSlot slot)
    {
        LastRoute = mainMenu.Tap(slot);
        return LastRoute;
    }
}
