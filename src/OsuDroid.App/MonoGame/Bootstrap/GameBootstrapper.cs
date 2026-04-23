#if ANDROID || IOS
using OsuDroid.Game;
using OsuDroid.Game.Runtime;
using OsuDroid.Game.Scenes;

namespace OsuDroid.App.MonoGame.Bootstrap;

public sealed class GameBootstrapper(Func<OsuDroidGameCore> createCore, Action<OsuDroidGameCore>? attachPlatformRuntime = null)
{
    private readonly object gate = new();
    private Task? bootTask;
    private OsuDroidGameCore? core;
    private Exception? failure;
    private BootstrapLoadingProgress progress = new(0, "Loading skin...");

    public BootstrapLoadingProgress Progress
    {
        get
        {
            lock (gate)
                return progress;
        }
    }

    public void Start()
    {
        lock (gate)
        {
            if (bootTask is not null)
                return;

            bootTask = Task.Run(Boot);
        }
    }

    public bool TryConsumeCore(out OsuDroidGameCore loadedCore)
    {
        lock (gate)
        {
            if (failure is not null)
                throw new InvalidOperationException("osu!droid bootstrap failed.", failure);

            if (core is null)
            {
                loadedCore = null!;
                return false;
            }

            loadedCore = core;
            core = null;
            return true;
        }
    }

    private void Boot()
    {
        try
        {
            SetProgress(8, "Loading data...");
            var createdCore = createCore();
            SetProgress(72, "Loading audio...");
            attachPlatformRuntime?.Invoke(createdCore);
            SetProgress(96, "Almost ready...");

            lock (gate)
            {
                core = createdCore;
                progress = new BootstrapLoadingProgress(100, "Welcome");
            }
        }
        catch (Exception exception)
        {
            lock (gate)
            {
                failure = exception;
                progress = new BootstrapLoadingProgress(progress.Percent, "Loading failed");
            }
        }
    }

    private void SetProgress(int percent, string statusText)
    {
        lock (gate)
            progress = new BootstrapLoadingProgress(percent, statusText);
    }
}
#endif
