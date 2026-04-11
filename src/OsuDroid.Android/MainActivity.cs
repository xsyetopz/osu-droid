using Android.App;
using Android.Content.PM;
using Android.OS;
using OsuDroid.Android.Platform.External;
using OsuDroid.Android.Platform.Storage;
using OsuDroid.Game;
using osu.Framework.Android;
using Android.Util;
using System;
using System.Threading.Tasks;

namespace OsuDroid.Android;

[Activity(
    Label = "osu!droid",
    MainLauncher = true,
    Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AndroidGameActivity
{
    private const string LogTag = "OsuDroid.Android";

    static MainActivity()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            Log.Error(LogTag, $"Unhandled AppDomain exception: {args.ExceptionObject}");

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Log.Error(LogTag, $"Unobserved task exception: {args.Exception}");
            args.SetObserved();
        };
    }

    protected override osu.Framework.Game CreateGame() =>
        createGame();

    protected override void OnCreate(global::Android.OS.Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Log.Info(LogTag, "OnCreate");
    }

    protected override void OnStart()
    {
        base.OnStart();
        Log.Info(LogTag, "OnStart");
    }

    protected override void OnResume()
    {
        base.OnResume();
        Log.Info(LogTag, "OnResume");
    }

    protected override void OnPause()
    {
        Log.Info(LogTag, "OnPause");
        base.OnPause();
    }

    protected override void OnStop()
    {
        Log.Info(LogTag, "OnStop");
        base.OnStop();
    }

    protected override void OnDestroy()
    {
        Log.Info(LogTag, "OnDestroy");
        base.OnDestroy();
    }

    private OsuDroidGame createGame()
        => new(new AndroidExternalUriLauncher(this), new AndroidPlatformStorage(this));
}
