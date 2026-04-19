#if ANDROID || IOS
using Microsoft.Maui.Controls;

namespace OsuDroid.App;

public sealed class App : Application
{
    private readonly IServiceProvider services;

    public App(IServiceProvider services)
    {
        this.services = services;
    }

    protected override Window CreateWindow(IActivationState? activationState) => new(new MainPage(services));
}
#endif
