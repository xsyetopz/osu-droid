#if ANDROID || IOS
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Hosting;
using OsuDroid.App.Platform;
using OsuDroid.Game;

namespace OsuDroid.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .Services.AddSingleton<IPlatformPaths, MauiPlatformPaths>()
            .AddSingleton(static services => OsuDroidGameCore.Create(
                services.GetRequiredService<IPlatformPaths>().Roots,
#if DEBUG
                "debug",
#else
                "release",
#endif
                AppInfo.Current.VersionString
            ));

        return builder.Build();
    }
}
#endif
