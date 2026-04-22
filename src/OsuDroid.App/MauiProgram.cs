#if ANDROID || IOS
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using OsuDroid.App.Platform;

namespace OsuDroid.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .Services.AddSingleton<IPlatformPaths, MauiPlatformPaths>();

        return builder.Build();
    }
}
#endif
